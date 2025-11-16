using UnityEngine;
using System.Collections.Generic;
public class ElectionSimulator : MonoBehaviour
{
    [Header("Config")]
    public CandidateData_SO playerCandidate;
    public CandidateData_SO[] aiCandidates;
    public StateData_SO[] states;
    public ElectionEvent_SO[] randomEvents;
    public ElectionEvent_SO[] scheduledEvents;

    public int totalWeeks = 12;

    private int _currentWeek = 0;

    private void Start()
    {
        StartSimulation();
    }

    public void StartSimulation()
    {
        _currentWeek = 0;
        LogManager.Instance.AddLog(_currentWeek, LogType.System, "Simulator", "你完成加点，竞选正式开始。");

        // 这里你可以记录一下初始人物卡摘要
        LogManager.Instance.AddLog(_currentWeek, LogType.System, "Candidate",
            $"你的属性：公众吸引力 {playerCandidate.publicAppeal}，政策能力 {playerCandidate.policySkill}，组织动员 {playerCandidate.organization}，诚信 {playerCandidate.integrity}。");

        // 可以改成按钮每次推进一周；现在先直接跑完
        for (int week = 1; week <= totalWeeks; week++)
        {
            RunWeek(week);
        }

        RunElectionDay();
    }

    private void RunWeek(int week)
    {
        _currentWeek = week;
        LogManager.Instance.AddLog(_currentWeek, LogType.System, "Simulator", $"第 {week} 周开始。");

        // 1. 候选人选择本周策略（先写死 / 随机，之后再细化）
        // TODO: Player策略/AI策略

        // 2. 检查并触发事件（含随机事件 + 固定事件）
        TryTriggerEventsForWeek();

        // 3. 计算本周民调
        ComputePollingForWeek();

        // 4. （可选）写一条玩家候选人的“内心独白”
        // LogManager.Instance.AddLog(_currentWeek, LogType.Flavor, playerCandidate.candidateName, "本周我要争取中部州的工人阶级选民。");
    }

    private void RunElectionDay()
    {
        var candidatesAll = GetAllCandidates();
        if (candidatesAll.Length == 0 || states == null || states.Length == 0)
            return;

        int electionDayWeek = _currentWeek + 1;
        LogManager.Instance.AddLog(electionDayWeek, LogType.System, "Simulator", "投票日到来，开始结算选举人票……");

        // 统计每个候选人拿到的 EV
        var electoralVotesMap = new Dictionary<CandidateData_SO, int>();
        foreach (var c in candidatesAll)
        {
            electoralVotesMap[c] = 0;
        }

        // 逐州结算
        foreach (var state in states)
        {
            if (state == null) continue;

            var winner = ElectionMath.SimulateStateWinner(state, candidatesAll);
            electoralVotesMap[winner] += state.electoralVotes;

            LogManager.Instance.AddLog(electionDayWeek, LogType.Result, "StateResult",
                $"{state.stateName} -> 胜者：{winner.candidateName}（{state.electoralVotes} 张选举人票）");
        }

        // 计算总票 & 找大赢家
        CandidateData_SO finalWinner = null;
        int bestEV = int.MinValue;

        string totalLine = "总选举人票：";
        foreach (var kv in electoralVotesMap)
        {
            var c = kv.Key;
            int ev = kv.Value;
            totalLine += $"{c.candidateName}={ev}  ";

            if (ev > bestEV)
            {
                bestEV = ev;
                finalWinner = c;
            }
        }

        LogManager.Instance.AddLog(electionDayWeek, LogType.Result, "Simulator", totalLine);

        if (finalWinner != null)
        {
            LogManager.Instance.AddLog(electionDayWeek, LogType.Result, "Simulator",
                $"最终结果：{finalWinner.candidateName} 赢得大选！");
        }
    }
    private void TryTriggerEventsForWeek()
    {
        // 1. 固定周事件（比如 TV 辩论）
        foreach (var ev in scheduledEvents)
        {
            if (ev == null) continue;
            if (ev.fixedWeek && ev.fixedWeekNumber == _currentWeek)
            {
                HandleEvent(ev);
            }
        }

        // 2. 随机事件
        foreach (var ev in randomEvents)
        {
            if (ev == null) continue;
            if (_currentWeek < ev.earliestWeek || _currentWeek > ev.latestWeek) continue;

            if (Random.value < ev.baseChance)
            {
                HandleEvent(ev);
            }
        }
    }

    private void HandleEvent(ElectionEvent_SO ev)
    {
        // 事件发生日志
        LogManager.Instance.AddLog(_currentWeek, LogType.Event, $"Event:{ev.eventId}",
            $"{ev.title}\n{ev.description}");

        if (ev.choices == null || ev.choices.Length == 0)
        {
            return;
        }

        // TODO: 这里未来应该弹 UI 让玩家选
        // 现在先自动选第一个选项，方便你调试数值
        EventChoice chosen = ev.choices[0];

        // 记一条“玩家选择了 xxx”
        LogManager.Instance.AddLog(_currentWeek, LogType.Choice, "PlayerChoice",
            $"你选择了：{chosen.label}\n{chosen.description}");

        // 应用效果到玩家候选人（可以扩展到多候选人）
        ApplyChoiceEffectsToCandidate(playerCandidate, chosen);
    }

    private void ApplyChoiceEffectsToCandidate(CandidateData_SO c, EventChoice choice)
    {
        c.publicAppeal   = Mathf.Clamp(c.publicAppeal   + choice.deltaPublicAppeal,   0, 10);
        c.policySkill    = Mathf.Clamp(c.policySkill    + choice.deltaPolicySkill,    0, 10);
        c.organization   = Mathf.Clamp(c.organization   + choice.deltaOrganization,   0, 10);
        c.integrity      = Mathf.Clamp(c.integrity      + choice.deltaIntegrity,      0, 10);

        // TODO: opinionBoost / scandalRiskChange 可以写到别的系统里

        LogManager.Instance.AddLog(_currentWeek, LogType.Result, "Simulator",
            $"事件影响：你的属性变为——公众吸引力 {c.publicAppeal}，政策 {c.policySkill}，组织 {c.organization}，诚信 {c.integrity}。");
    }

    private void ComputePollingForWeek()
    {
        var candidatesAll = GetAllCandidates();
        if (candidatesAll.Length == 0 || states == null || states.Length == 0)
            return;

        // 统计全国民调（按选举人票加权）
        var nationalEVWeighted = new Dictionary<CandidateData_SO, float>();
        float totalEV = 0f;
        foreach (var c in candidatesAll)
        {
            nationalEVWeighted[c] = 0f;
        }

        // 每个州算一次民调，并写入 log
        foreach (var state in states)
        {
            if (state == null) continue;

            var polling = ElectionMath.ComputeStatePolling(state, candidatesAll);

            // 写一条州内民调日志
            string pollLine = $"{state.stateName} 民调：";
            foreach (var c in candidatesAll)
            {
                float percent = polling[c];
                pollLine += $"{c.candidateName}={percent:F1}%  ";
            }

            LogManager.Instance.AddLog(_currentWeek, LogType.Poll, "StatePolling", pollLine);

            // 累加到全国（选举人票加权）
            totalEV += state.electoralVotes;
            foreach (var c in candidatesAll)
            {
                nationalEVWeighted[c] += polling[c] * state.electoralVotes;
            }
        }

        // 计算并输出全国民调
        if (totalEV > 0f)
        {
            string nationalLine = "全国民调（按选举人票加权）：";
            foreach (var c in candidatesAll)
            {
                float percent = nationalEVWeighted[c] / totalEV;
                nationalLine += $"{c.candidateName}={percent:F1}%  ";
            }
            LogManager.Instance.AddLog(_currentWeek, LogType.Poll, "NationalPolling", nationalLine);
        }
    }

    private CandidateData_SO[] GetAllCandidates()
    {
        // 玩家 + AI 拼在一起
        var list = new List<CandidateData_SO>();
        if (playerCandidate != null)
            list.Add(playerCandidate);
        if (aiCandidates != null)
            list.AddRange(aiCandidates);
        return list.ToArray();
    }


}
