using UnityEngine;
[CreateAssetMenu(fileName = "StateData_SO", menuName = "StateData_SO", order = 0)]
public class StateData_SO : ScriptableObject {
    public string stateName;
    public int electoralVotes;
    public VoterGroupData_SO[] voterGroups;
}
