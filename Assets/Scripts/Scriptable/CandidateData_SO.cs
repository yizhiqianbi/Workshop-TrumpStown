using UnityEngine;

using UnityEngine;

[CreateAssetMenu(fileName = "CandidateData_SO", menuName = "CandidateData_SO", order = 0)]
public class CandidateData_SO : ScriptableObject {
    

    [Header("Basic Info")]
    public string candidateName;
    public string party;

    [Header("Core Stats (0~10)")]
    [UnityEngine.Range(0, 10)] public int publicAppeal;   // 公众吸引力
    [UnityEngine.Range(0, 10)] public int policySkill;    // 政策能力
    [UnityEngine.Range(0, 10)] public int organization;   // 组织动员
    [UnityEngine.Range(0, 10)] public int integrity;      // 诚信

    [Header("Ideology [-1, 1]")]
    [UnityEngine.Range(-1f, 1f)] public float economy;
    [UnityEngine.Range(-1f, 1f)] public float social;
    [UnityEngine.Range(-1f, 1f)] public float security;
    [UnityEngine.Range(-1f, 1f)] public float environment;

    [Header("Slogan")]
    public string sloganText;
    public SloganTheme sloganTheme;    // Change / Stability / ...
    public SloganIssue mainIssue;      // Economy / Security / ...
    public SloganEmotion emotion;      // Hope / Anger / ...

    // 可以再加一些 helper，比如返回 ideology 字典等
}

public enum SloganTheme { Change, Stability, Unity, Security, Prosperity }
public enum SloganIssue { None, Economy, Security, Environment }
public enum SloganEmotion { Neutral, Hope, Anger, Fear, Pride }
