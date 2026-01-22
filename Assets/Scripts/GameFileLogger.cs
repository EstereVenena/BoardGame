using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class GameFileLogger : MonoBehaviour
{
    public static GameFileLogger I { get; private set; }

    [Header("File")]
    [Tooltip("Base filename. Timestamp + session id are appended.")]
    public string filePrefix = "BoardGame_Log";

    [Tooltip("Also write to persistentDataPath (good for builds that can't access Desktop).")]
    public bool alsoWriteToPersistentDataPath = true;

    [Header("Capture")]
    [Tooltip("Capture Unity Debug.Log/Warning/Error/Exceptions into the file.")]
    public bool captureUnityLogs = true;

    [Tooltip("Include stack trace for errors/exceptions.")]
    public bool includeStackTrace = true;

    [Header("Flush")]
    [Tooltip("Write to disk every N lines (smaller = safer, bigger = faster).")]
    [Range(1, 200)]
    public int flushEveryLines = 10;

    [Tooltip("Also flush every X seconds even if not enough lines yet.")]
    [Range(0f, 10f)]
    public float flushEverySeconds = 1f;

    private string desktopPath;
    private string filePathDesktop;
    private string filePathPersistent;
    private readonly StringBuilder buffer = new StringBuilder(16_384);
    private int pendingLines;
    private float lastFlushTime;
    private readonly object fileLock = new object();

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        desktopPath = GetDesktopPath();
        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var sessionId = Guid.NewGuid().ToString("N")[..8];

        var fileName = $"{filePrefix}_{stamp}_{sessionId}.txt";
        filePathDesktop = Path.Combine(desktopPath, fileName);
        filePathPersistent = Path.Combine(Application.persistentDataPath, fileName);

        // Write header
        WriteLine("=== SESSION START ===");
        WriteLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        WriteLine($"Unity: {Application.unityVersion}");
        WriteLine($"Platform: {Application.platform}");
        WriteLine($"Product: {Application.productName}");
        WriteLine($"Version: {Application.version}");
        WriteLine($"PersistentDataPath: {Application.persistentDataPath}");
        WriteLine($"DesktopPath: {desktopPath}");
        WriteLine($"DesktopLogFile: {filePathDesktop}");
        if (alsoWriteToPersistentDataPath) WriteLine($"PersistentLogFile: {filePathPersistent}");
        WriteLine("=====================");

        SafeEnsureFile(filePathDesktop);
        if (alsoWriteToPersistentDataPath) SafeEnsureFile(filePathPersistent);

        if (captureUnityLogs)
            Application.logMessageReceived += OnUnityLog;

        lastFlushTime = Time.unscaledTime;
        FlushNow(); // write header immediately
    }

    void OnDestroy()
    {
        if (captureUnityLogs)
            Application.logMessageReceived -= OnUnityLog;

        WriteLine("=== SESSION END ===");
        FlushNow();
    }

    void Update()
    {
        // periodic flush
        if (flushEverySeconds > 0f && Time.unscaledTime - lastFlushTime >= flushEverySeconds)
        {
            FlushNow();
        }
    }

    private void OnUnityLog(string condition, string stackTrace, LogType type)
    {
        // Avoid infinite loop if our own file writing causes logs
        if (condition != null && condition.Contains("GameFileLogger")) return;

        if (type == LogType.Log)
            WriteLine($"[UNITY][LOG] {condition}");
        else if (type == LogType.Warning)
            WriteLine($"[UNITY][WARN] {condition}");
        else if (type == LogType.Error)
            WriteLine($"[UNITY][ERROR] {condition}");
        else if (type == LogType.Assert)
            WriteLine($"[UNITY][ASSERT] {condition}");
        else if (type == LogType.Exception)
            WriteLine($"[UNITY][EXCEPTION] {condition}");

        if (includeStackTrace && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert))
        {
            if (!string.IsNullOrEmpty(stackTrace))
                WriteLine(stackTrace);
        }
    }

    public static void Log(string msg) => I?.WriteLine($"[APP] {msg}");

    public static void LogKV(string key, object value) => I?.WriteLine($"[APP] {key} = {value}");

    public static void LogEvent(string evt, params (string k, object v)[] fields)
    {
        if (I == null) return;
        var sb = new StringBuilder();
        sb.Append("[EVT] ").Append(evt);
        if (fields != null)
        {
            for (int i = 0; i < fields.Length; i++)
                sb.Append(" | ").Append(fields[i].k).Append("=").Append(fields[i].v);
        }
        I.WriteLine(sb.ToString());
    }

    public void WriteLine(string line)
    {
        var ts = DateTime.Now.ToString("HH:mm:ss.fff");
        var full = $"{ts} {line}";

        lock (fileLock)
        {
            buffer.AppendLine(full);
            pendingLines++;
        }

        if (pendingLines >= flushEveryLines)
            FlushNow();
    }

    public void FlushNow()
    {
        string chunk;
        lock (fileLock)
        {
            if (buffer.Length == 0) { lastFlushTime = Time.unscaledTime; return; }
            chunk = buffer.ToString();
            buffer.Clear();
            pendingLines = 0;
        }

        SafeAppend(filePathDesktop, chunk);
        if (alsoWriteToPersistentDataPath)
            SafeAppend(filePathPersistent, chunk);

        lastFlushTime = Time.unscaledTime;
    }

    private void SafeEnsureFile(string path)
    {
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(path))
                File.WriteAllText(path, ""); // create
        }
        catch (Exception e)
        {
            // fallback to persistent path only
            Debug.LogWarning($"GameFileLogger could not create file at {path}: {e.Message}");
        }
    }

    private void SafeAppend(string path, string text)
    {
        try
        {
            File.AppendAllText(path, text, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"GameFileLogger could not write to {path}: {e.Message}");
        }
    }

    private string GetDesktopPath()
    {
        try
        {
            // Works for Windows/macOS/Linux in most cases
            return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        }
        catch
        {
            return Application.persistentDataPath;
        }
    }
}
