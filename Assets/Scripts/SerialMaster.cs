using UnityEngine;
using System; // Actionデリゲートのために追加
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Security; // SecurityException のために追加

public class SerialMaster : MonoBehaviour
{
    // シリアルデータ受信イベントのデリゲートとイベント定義
    public delegate void SerialDataReceivedEventHandler(string message);
    public event SerialDataReceivedEventHandler OnDataReceived;

    [Tooltip("使用するシリアルポート名 (例: COM1, /dev/ttyUSB0)")]
    public string portName = "COM10";
    [Tooltip("ボーレート (Arduinoなどと合わせる)")]
    public int baudRate = 115200;

    private SerialPort serialPort_;
    private Thread thread_;
    private bool isRunning_ = false; // スレッドの実行状態を制御するフラグ

    // メッセージをキューに格納し、メインスレッドで安全に取り出すためのキューとロックオブジェクト
    private Queue<string> messageQueue = new Queue<string>();
    private readonly object queueLock = new object(); // キューへのアクセスをスレッドセーフにするためのロック

    // シリアルポートを開く
    void Start()
    {
        Open();
    }

    // メインスレッドでキューからメッセージを取り出し、イベントを発火
    void Update()
    {
        // 1フレームにつきキューから最大1つのメッセージを処理することで、UIの応答性を保つ
        // 大量のデータが一度に来た場合でも、キューが処理しきれないほど膨大にならないよう調整
        // または、必要に応じてwhileループで複数処理することも検討（ただし負荷に注意）
        string message = null;
        lock (queueLock) // キューからの取り出し時にロック
        {
            if (messageQueue.Count > 0)
            {
                message = messageQueue.Dequeue();
            }
        }

        if (message != null)
        {
            // Debug.Log($"SerialMaster: Processing message in Update: \"{message}\""); // 受信データを確認（頻繁ならコメントアウト）
            OnDataReceived?.Invoke(message); // サブスクライブしているThermalウィンドウにデータを渡す
        }
    }

    // オブジェクトが破棄されるときにシリアルポートとスレッドを確実に閉じる
    void OnDestroy()
    {
        Close();
    }

    /// <summary>
    /// シリアルポートを開き、データ読み取りスレッドを開始します。
    /// </summary>
    private void Open()
    {
        if (serialPort_ != null && serialPort_.IsOpen)
        {
            Debug.LogWarning($"SerialMaster: Port {portName} is already open.");
            return;
        }

        try
        {
            // SerialPortの初期化
            serialPort_ = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
            serialPort_.ReadTimeout = 500; // データの読み取りタイムアウト設定（ミリ秒）
            serialPort_.WriteTimeout = 500; // データの書き込みタイムアウト設定（任意）
            serialPort_.Open(); // ポートを開く

            isRunning_ = true; // スレッド実行フラグをtrueに
            thread_ = new Thread(Read); // 読み取りスレッドを生成
            thread_.Start(); // スレッド開始
            Debug.Log($"SerialMaster: Serial port '{portName}' opened successfully at {baudRate} baud.");
        }
        catch (System.IO.IOException e)
        {
            // ポートが存在しない、他のアプリケーションで使用中などのI/Oエラー
            Debug.LogError($"SerialMaster: Failed to open serial port '{portName}' (IOException): {e.Message}. Please check if the port exists and is not in use.");
            Close(); // エラー時は確実にリソースを閉じる
        }
        catch (System.UnauthorizedAccessException e)
        {
            // アクセス拒否エラー（権限不足など）
            Debug.LogError($"SerialMaster: Failed to open serial port '{portName}' (UnauthorizedAccessException): {e.Message}. Check application permissions or if another program is using the port.");
            Close();
        }
        catch (System.Exception e)
        {
            // その他の予期せぬエラー
            Debug.LogError($"SerialMaster: An unexpected error occurred while opening serial port '{portName}': {e.Message}");
            Close();
        }
    }

    /// <summary>
    /// シリアルポートとデータ読み取りスレッドを閉じます。
    /// </summary>
    private void Close()
    {
        isRunning_ = false; // スレッド実行フラグをfalseに設定し、スレッドに終了を促す

        // スレッドが動作中であれば終了を待つ
        if (thread_ != null && thread_.IsAlive)
        {
            // スレッドを安全に終了させるためのJoin
            // ReadTimeoutを設定しているため、Joinでタイムアウトを待つことで例外を捕捉し、クリーンに終了できる可能性が高まる
            thread_.Join(100); // 100ミリ秒待つ
            if (thread_.IsAlive)
            {
                // まだスレッドが終了していない場合は強制終了を試みる（最終手段）
                Debug.LogWarning("SerialMaster: Serial read thread did not terminate gracefully. Attempting interrupt.");
                try
                {
                    thread_.Interrupt(); // スレッドを強制終了（ThreadAbortExceptionを発生させる）
                }
                catch (ThreadStateException)
                {
                    // スレッドが既に中断されているか、別の状態の場合
                    Debug.LogWarning("SerialMaster: ThreadState was not suitable for Interrupt.");
                }
                catch (SecurityException)
                {
                    // セキュリティポリシーによりInterruptが拒否された場合
                    Debug.LogError("SerialMaster: SecurityException during thread Interrupt.");
                }
            }
            thread_ = null; // スレッド参照をクリア
        }

        // シリアルポートがオープン状態であれば閉じる
        if (serialPort_ != null && serialPort_.IsOpen)
        {
            try
            {
                serialPort_.Close();
                serialPort_.Dispose(); // リソースを解放
                Debug.Log($"SerialMaster: Serial port '{portName}' closed.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SerialMaster: Error closing serial port '{portName}': {e.Message}");
            }
        }
        serialPort_ = null; // シリアルポート参照をクリア
    }

    /// <summary>
    /// 別スレッドでシリアルポートからデータを読み取るループ。
    /// </summary>
    private void Read()
    {
        Debug.Log("SerialMaster: Serial read thread started.");
        while (isRunning_ && serialPort_ != null && serialPort_.IsOpen)
        {
            try
            {
                // シリアルポートから1行読み込む（ReadLineは改行コードまで読み込む）
                string receivedLine = serialPort_.ReadLine();
                // 受信したデータをキューに追加（スレッドセーフに）
                lock (queueLock)
                {
                    messageQueue.Enqueue(receivedLine);
                }
                // Debug.Log($"SerialMaster: Read thread received: \"{receivedLine}\""); // スレッドからの受信をログ（頻繁ならコメントアウト）
            }
            catch (System.TimeoutException)
            {
                // ReadTimeoutによって発生する正常なタイムアウト。頻繁なのでログは控えめに。
                // Debug.Log($"SerialMaster: Read timeout on '{portName}'. No data received within timeout.");
            }
            catch (System.IO.IOException e)
            {
                // ポート切断など、読み取り中のI/Oエラー
                Debug.LogError($"SerialMaster: Serial port I/O error during read on '{portName}': {e.Message}");
                isRunning_ = false; // エラーが発生したら読み取りループを終了
            }
            catch (System.InvalidOperationException e)
            {
                // ポートが閉じた後に読み取ろうとしたなど
                Debug.LogError($"SerialMaster: Invalid operation during serial read on '{portName}': {e.Message}. Port might have been closed.");
                isRunning_ = false;
            }
            catch (System.Threading.ThreadAbortException)
            {
                // スレッドがInterruptされた場合
                Debug.Log("SerialMaster: Serial read thread was aborted (ThreadAbortException).");
                Thread.ResetAbort(); // ThreadAbortException をリセットしてスレッドが終了できるようにする
                isRunning_ = false;
            }
            catch (System.Exception e)
            {
                // その他の予期せぬエラー
                Debug.LogError($"SerialMaster: An unexpected error occurred in serial read thread: {e.Message}");
                isRunning_ = false; // 予期せぬエラーが発生したら読み取りを停止
            }
        }
        Debug.Log("SerialMaster: Serial read thread stopped.");
    }

    /// <summary>
    /// シリアルポートにデータを書き込みます。
    /// </summary>
    /// <param name="message">送信する文字列。</param>
    public void Write(string message)
    {
        if (serialPort_ != null && serialPort_.IsOpen)
        {
            try
            {
                serialPort_.Write(message);
                Debug.Log($"SerialMaster: Wrote message to '{portName}': \"{message}\"");
            }
            catch (System.TimeoutException)
            {
                Debug.LogWarning($"SerialMaster: Write timeout to port '{portName}'. Message: \"{message}\"");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SerialMaster: Failed to write to serial port '{portName}': {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("SerialMaster: Serial port is not open. Cannot write message.");
        }
    }

    /// <summary>
    /// 現在のシリアルポートの接続状態を返します。
    /// </summary>
    public bool IsPortOpen()
    {
        return serialPort_ != null && serialPort_.IsOpen;
    }

    /// <summary>
    /// 利用可能なシリアルポート名の一覧を取得します。
    /// </summary>
    public static string[] GetPortNames()
    {
        return SerialPort.GetPortNames();
    }
}