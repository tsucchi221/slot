using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SerialButtonInvoker : MonoBehaviour
{
    public UnityEngine.UI.Button[] buttons;
    public Serial serial;

    private string previousValue = "0";
    private int currentButtonIndex = 0;

    private bool canPress = true;

    void Start()
    {
        if (serial != null)
        {
            serial.OnDataReceived += OnSerialDataReceived;
        }
        else
        {
            Debug.LogError("SerialButtonInvoker: Serial script not assigned.");
        }

        if (buttons == null || buttons.Length < 4)
        {
            Debug.LogError("SerialButtonInvoker: 4つのボタンを設定してください。");
        }
    }

    void OnDestroy()
    {
        if (serial != null)
        {
            serial.OnDataReceived -= OnSerialDataReceived;
        }
    }

    private void OnSerialDataReceived(string message)
    {
        string currentValue = message.Trim();

        if (previousValue == "0" && currentValue == "1" && canPress)
        {
            Debug.Log("SerialButtonInvoker: 0→1 detected, pressing button " + currentButtonIndex);

            if (buttons != null && buttons.Length > 0 && buttons[currentButtonIndex] != null)
            {
                UnityMainThreadCall(() =>
                {
                    buttons[currentButtonIndex].onClick.Invoke();
                });

                currentButtonIndex = (currentButtonIndex + 1) % buttons.Length;

                canPress = false;
                StartCoroutine(ResetPressCooldown(1.0f));
            }
        }

        previousValue = currentValue;
    }

    private IEnumerator ResetPressCooldown(float delay)
    {
        yield return new WaitForSeconds(delay);
        canPress = true;
    }

    private void UnityMainThreadCall(System.Action action)
    {
#if UNITY_EDITOR
        action();
#else
        action();
#endif
    }
}
