using UnityEngine;

public enum ElectionEventCategory
{
    Debate,        // 辩论
    Scandal,       // 丑闻
    Economy,       // 经济
    Security,      // 安全 / 外交
    Personal,      // 候选人个人故事
    Misc
}

// using UnityEngine;

[CreateAssetMenu(fileName = "ElectionEvent_SO", menuName = "ElectionEvent_SO", order = 0)]
public class ElectionEvent_SO : ScriptableObject {
    
    public string eventId;
    public string title;
    [TextArea] public string description;

    public ElectionEventCategory category;

    [Header("When to trigger")]
    public int earliestWeek = 1;
    public int latestWeek = 12;
    public float baseChance = 0.2f;   // 每周基础触发概率（随机事件用）

    [Header("Optional: fixed schedule")]
    public bool fixedWeek;
    public int fixedWeekNumber;       // 比如某个 TV 辩论固定第 4 周

    [Header("Choices")]
    public EventChoice[] choices;
}
[System.Serializable]
public class EventChoice
{
    public string choiceId;
    public string label;         // 选项标题（UI按钮文本）
    [TextArea] public string description; // 选项说明（Log 用）

    // 简化版数值效果：对候选人属性 / 好感度的直接改动
    public int deltaPublicAppeal;
    public int deltaPolicySkill;
    public int deltaOrganization;
    public int deltaIntegrity;

    public float opinionBoost;      // 对全国支持度小幅加成
    public float scandalRiskChange; // 丑闻风险变动（可选）
}
