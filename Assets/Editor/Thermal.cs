using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Diagnostics; // Stopwatch を使う

public class Thermal : EditorWindow
{
    private struct TemperatureSample
    {
        public double time;
        public float temperature;
    }

    private List<TemperatureSample> temperatureData = new List<TemperatureSample>();
    private const float graphDuration = 10f;
    private float yMin = 0f;
    private float yMax = 40f;

    private float marginLeft = 60f;
    private float marginBottom = 20f;
    private int gridDivisionsX = 10;
    private int gridDivisionsY = 8;

    private SerialMaster serialMaster;
    private bool isInitialized = false;

    private Stopwatch stopwatch = new Stopwatch(); // 実時間計測用

    [MenuItem("Custom/Thermal")]
    static void ShowWindow()
    {
        GetWindow<Thermal>("Thermal Graph");
    }

    private void OnEnable()
    {
        EditorApplication.update += UpdateWindow;
        stopwatch.Reset();
        stopwatch.Start();
        isInitialized = false;
        temperatureData.Clear();
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateWindow;
        stopwatch.Stop();
        if (serialMaster != null)
        {
            serialMaster.OnDataReceived -= OnSerialDataReceived;
        }
    }

    private void UpdateWindow()
    {
        Repaint();
    }

    private void Init()
    {
        if (isInitialized) return;

        serialMaster = GameObject.FindObjectOfType<SerialMaster>();

        if (serialMaster != null)
        {
            UnityEngine.Debug.Log("Thermal: SerialMaster found, subscribing.");
            serialMaster.OnDataReceived += OnSerialDataReceived;
            isInitialized = true;
        }
        else
        {
            UnityEngine.Debug.LogWarning("Thermal: SerialMaster not found in scene.");
        }
    }

    private void OnSerialDataReceived(string message)
    {
        message = message.Trim();

        if (float.TryParse(message, out float tempRaw))
        {
            double now = stopwatch.Elapsed.TotalSeconds;

            // 温度変換: 1.25 → 30°C, 1.45 → 20°C に対応
            float convertedTemp = -50f * tempRaw + 92.5f;

            if (convertedTemp < yMin || convertedTemp > yMax)
            {
                UnityEngine.Debug.LogWarning($"Thermal: Ignored out-of-range temp: {convertedTemp:F2}°C");
                return;
            }

            temperatureData.Add(new TemperatureSample { time = now, temperature = convertedTemp });

            double minTime = now - graphDuration;
            temperatureData.RemoveAll(sample => sample.time < minTime);
        }
        else
        {
            UnityEngine.Debug.LogError($"Thermal: Failed to parse serial data: \"{message}\"");
        }
    }

    private void OnGUI()
    {
        Init();

        GUILayout.Label("Thermal Graph (0 - 40°C)", EditorStyles.boldLabel);

        Rect graphRect = GUILayoutUtility.GetRect(position.width - 10f, position.height - 80f);
        graphRect = new Rect(graphRect.x + marginLeft, graphRect.y, graphRect.width - marginLeft, graphRect.height - marginBottom);

        GUILayout.Label($"Data Count: {temperatureData.Count}");

        DrawGrid(graphRect);
        DrawAxes(graphRect);
        DrawGraph(graphRect);
    }

    private void DrawGrid(Rect rect)
    {
        Handles.color = new Color(0.3f, 0.3f, 0.3f);

        for (int i = 0; i <= gridDivisionsX; i++)
        {
            float x = Mathf.Lerp(rect.x, rect.xMax, i / (float)gridDivisionsX);
            Handles.DrawLine(new Vector3(x, rect.y), new Vector3(x, rect.yMax));
        }

        for (int i = 0; i <= gridDivisionsY; i++)
        {
            float y = Mathf.Lerp(rect.y, rect.yMax, i / (float)gridDivisionsY);
            Handles.DrawLine(new Vector3(rect.x, y), new Vector3(rect.xMax, y));

            float value = Mathf.Lerp(yMin, yMax, 1 - i / (float)gridDivisionsY);
            GUI.Label(new Rect(rect.x - marginLeft + 5, y - 8, marginLeft - 10, 16), value.ToString("F1") + "°C", EditorStyles.miniLabel);
        }
    }

    private void DrawAxes(Rect rect)
    {
        Handles.color = Color.white;
        Handles.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x, rect.yMax));
        Handles.DrawLine(new Vector3(rect.x, rect.yMax), new Vector3(rect.xMax, rect.yMax));

        double now = stopwatch.Elapsed.TotalSeconds;

        for (int i = 0; i <= gridDivisionsX; i++)
        {
            float t = i / (float)gridDivisionsX;
            double timeValue = Mathf.Lerp((float)(now - graphDuration), (float)now, t);
            float x = Mathf.Lerp(rect.x, rect.xMax, t);
            GUI.Label(new Rect(x - 10, rect.yMax + 2, 40, 16), timeValue.ToString("F0") + "s", EditorStyles.miniLabel);
        }
    }

    private void DrawGraph(Rect rect)
    {
        if (temperatureData.Count < 2) return;

        Handles.BeginGUI();
        Handles.color = Color.green;

        double now = stopwatch.Elapsed.TotalSeconds;
        double minTime = now - graphDuration;

        Vector3 prev = DataToGraphPosition(temperatureData[0], minTime, now, rect);
        for (int i = 1; i < temperatureData.Count; i++)
        {
            Vector3 curr = DataToGraphPosition(temperatureData[i], minTime, now, rect);
            Handles.DrawLine(prev, curr);
            prev = curr;
        }

        Handles.EndGUI();
    }

    private Vector3 DataToGraphPosition(TemperatureSample sample, double minTime, double maxTime, Rect rect)
    {
        float normalizedTime = Mathf.InverseLerp((float)minTime, (float)maxTime, (float)sample.time);
        float x = Mathf.Lerp(rect.x, rect.xMax, normalizedTime);

        float normalizedY = Mathf.InverseLerp(yMin, yMax, sample.temperature);
        float y = Mathf.Lerp(rect.yMax, rect.y, normalizedY);

        return new Vector3(x, y, 0);
    }
}
