using UnityEngine;
using System.Collections.Generic;

public enum AIDifficulty
{
    Easy,
    Medium,
    Hard,
    // 
    Count
}

// AI model
[System.Serializable]
public class AIModel
{
    public AIDifficulty difficulty;
    [Range(0f, 1f)]
    public float value = 0.5f;
}

[CreateAssetMenu(fileName = "AISetup", menuName = "AI/New AI Setup", order = 1)]
public class AISetup : ScriptableObject
{
    // List of options
    [SerializeField]
    private List<AIModel> m_AI;

    /// <summary>
    /// Function to get object AI presets.
    /// </summary>
    public AIModel GetAI(AIDifficulty difficulty)
    {
        return m_AI.Find(x => x.difficulty == difficulty);
    }
}
