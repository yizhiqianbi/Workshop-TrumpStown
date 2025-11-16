using UnityEngine;


using UnityEngine;

[CreateAssetMenu(fileName = "VoterGroupData_SO", menuName = "VoterGroupData_SO", order = 0)]
public class VoterGroupData_SO : ScriptableObject {
    public string groupName;
    public int size;

    [Header("Issue Preferences [-1, 1]")]
    [Range(-1f, 1f)] public float prefEconomy;
    [Range(-1f, 1f)] public float prefSocial;
    [Range(-1f, 1f)] public float prefSecurity;
    [Range(-1f, 1f)] public float prefEnvironment;

    [Range(0f, 1f)] public float baseTurnout;
    public bool isPolicyOriented;   // 理性型
    public bool isValueOriented;    // 重价值观
}
