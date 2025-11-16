using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum LogType
{
    System,   // 系统提示：开局、周目开始等
    Event,    // 事件描述
    Choice,   // 玩家做出的选择
    Result,   // 事件结局
    Poll,     // 民调 / 选举结果
    Flavor    // 花絮、候选人内心独白
}

[System.Serializable]
public class LogEntry
{
    public int week;        // 第几周（0 表示开局前）
    public LogType type;
    public string source;   // 来源：比如 "System", "Event:TVDebate", "Candidate:Alice"
    [TextArea] public string text;
}

public class LogManager : MonoBehaviour
{
    public static LogManager Instance { get; private set; }

    public UnityEvent<LogEntry> onLogAdded = new UnityEvent<LogEntry>();

    private readonly List<LogEntry> _entries = new List<LogEntry>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddLog(int week, LogType type, string source, string text)
    {
        var entry = new LogEntry
        {
            week = week,
            type = type,
            source = source,
            text = text
        };
        _entries.Add(entry);

        // 控制台也打一份，方便调试
        Debug.Log($"[Week {week}] [{type}] {source}: {text}");

        // UI 想监听就监听这个事件
        onLogAdded.Invoke(entry);
    }

    public IReadOnlyList<LogEntry> GetAllLogs()
    {
        return _entries;
    }
}
