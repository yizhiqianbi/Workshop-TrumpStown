using System.Collections.Generic;
using UnityEngine;

public static class ElectionMath
{
    /// <summary>
    /// 选民群体对某候选人的“相对支持度”。
    /// 返回的不是百分比，而是后面要归一化的 raw score。
    /// </summary>
    public static float ComputeSupportScore(CandidateData_SO c, VoterGroupData_SO g)
    {
        // 1. 议题匹配度（候选人立场 vs 群体偏好）
        float economyMatch   = 1f - Mathf.Abs(c.economy     - g.prefEconomy)     * 0.5f;
        float socialMatch    = 1f - Mathf.Abs(c.social      - g.prefSocial)      * 0.5f;
        float securityMatch  = 1f - Mathf.Abs(c.security    - g.prefSecurity)    * 0.5f;
        float envMatch       = 1f - Mathf.Abs(c.environment - g.prefEnvironment) * 0.5f;

        economyMatch  = Mathf.Clamp01(economyMatch);
        socialMatch   = Mathf.Clamp01(socialMatch);
        securityMatch = Mathf.Clamp01(securityMatch);
        envMatch      = Mathf.Clamp01(envMatch);

        float issueMatch = (economyMatch + socialMatch + securityMatch + envMatch) * 0.25f;

        // 2. 公众吸引力：全局魅力加成
        float appealFactor = 1f + c.publicAppeal * 0.03f;

        // 3. 政策能力：对理性型选民有额外加成
        float policyFactor = 1f;
        if (g.isPolicyOriented)
        {
            policyFactor += c.policySkill * 0.04f;
        }

        // 4. 诚信：对价值观型选民加成
        float integrityFactor = 1f;
        if (g.isValueOriented)
        {
            integrityFactor += c.integrity * 0.03f;
        }

        float baseScore = issueMatch * appealFactor * policyFactor * integrityFactor;

        // 防止全部 0
        return Mathf.Max(baseScore, 0.01f);
    }

    /// <summary>
    /// 某个候选人在某个群体中的“有效投票率”（用组织力拉高 turnout）
    /// </summary>
    public static float ComputeEffectiveTurnout(CandidateData_SO c, VoterGroupData_SO g)
    {
        float turnout = g.baseTurnout * (1f + c.organization * 0.05f);
        return Mathf.Clamp01(turnout);
    }

    /// <summary>
    /// 计算某州的民调：返回 Candidate -> 百分比 的字典。
    /// </summary>
    public static Dictionary<CandidateData_SO, float> ComputeStatePolling(
        StateData_SO state,
        CandidateData_SO[] candidates)
    {
        var rawSupport = new Dictionary<CandidateData_SO, float>();
        foreach (var c in candidates)
        {
            rawSupport[c] = 0f;
        }

        foreach (var group in state.voterGroups)
        {
            if (group == null) continue;

            foreach (var c in candidates)
            {
                float support = ComputeSupportScore(c, group);
                float turnout = ComputeEffectiveTurnout(c, group);

                // 群体越大、投票率越高、支持度越高，对 raw 分数贡献越大
                float contribution = support * group.size * turnout;
                rawSupport[c] += contribution;
            }
        }

        // 归一化为 0~100 百分比
        float total = 0f;
        foreach (var kv in rawSupport)
        {
            total += kv.Value;
        }

        var result = new Dictionary<CandidateData_SO, float>();
        if (total <= 0f)
        {
            // 极端情况，全平分
            float equal = 100f / candidates.Length;
            foreach (var c in candidates)
            {
                result[c] = equal;
            }
        }
        else
        {
            foreach (var c in candidates)
            {
                result[c] = rawSupport[c] / total * 100f;
            }
        }

        return result;
    }

    /// <summary>
    /// 投票日：在某州根据民调 + 随机波动决定谁赢。
    /// 返回获胜的 Candidate。
    /// </summary>
    public static CandidateData_SO SimulateStateWinner(
        StateData_SO state,
        CandidateData_SO[] candidates,
        float randomSwingRange = 3f)
    {
        var polling = ComputeStatePolling(state, candidates);

        CandidateData_SO winner = null;
        float bestScore = float.NegativeInfinity;

        foreach (var c in candidates)
        {
            float poll = polling[c];
            float swing = Random.Range(-randomSwingRange, randomSwingRange);
            float finalScore = poll + swing;

            if (finalScore > bestScore)
            {
                bestScore = finalScore;
                winner = c;
            }
        }

        return winner;
    }
}
