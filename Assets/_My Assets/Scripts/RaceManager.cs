using UnityEngine;
using KartGame.Track;
using System.Collections.Generic;

public abstract class RaceManager : MonoBehaviour
{
    [SerializeField, Tooltip("The name of the track in this scene.  Used for track time records.  Must be unique.")]
    protected string m_trackName;
    [SerializeField, Tooltip("All the checkpoints for the track in the order that they should be completed starting with the start/finish line checkpoint.")]
    protected List<Checkpoint> m_checkpoints = new List<Checkpoint>();

    // Internal
    protected Dictionary<IRacer, Checkpoint> m_RacerNextCheckpoints;
    protected bool m_IsRaceRunning;

    // Events
    protected System.Action<bool> onRaceComplete;

    // Accessors
    public bool IsRaceRunning => m_IsRaceRunning;

    protected virtual void Awake()
    {
        if (m_checkpoints.Count < 3)
        {
            Debug.LogWarning("There are currently " + m_checkpoints.Count + " checkpoints set on the Track Manager.  A minimum of 3 is recommended but kart control will not be enabled with 0.");
        }
    }

    protected virtual void OnEnable()
    {
        for (int i = 0; i < m_checkpoints.Count; i++)
        {
            m_checkpoints[i].OnKartHitCheckpoint += OnCheckRacerHitCheckpoint;
        }
    }

    protected virtual void OnDisable()
    {
        for (int i = 0; i < m_checkpoints.Count; i++)
        {
            m_checkpoints[i].OnKartHitCheckpoint -= OnCheckRacerHitCheckpoint;
        }
    }

    /// <summary>
    /// Initialize racer passing all racers (Generic type to pass interfaces that inherit from IRacer).
    /// </summary>
    public virtual void InitRaceManager(List<IRacer> racers, System.Action<bool> onRaceComplete)
    {
        if (m_checkpoints.Count == 0)
        {
            Debug.LogError("No checkpoints referenced");
            return;
        }

        // Set end race event
        this.onRaceComplete = onRaceComplete;

        m_RacerNextCheckpoints = new Dictionary<IRacer, Checkpoint>(racers.Count);
        for(int i = 0; i < racers.Count; i++)
        {
            m_RacerNextCheckpoints.Add(racers[i], m_checkpoints[0]);
            racers[i].DisableControl();
        }
    }

    /// <summary>
    /// Starts race (timers, etc) and enables control of all racers.
    /// </summary>
    public virtual void StartRace()
    {
        m_IsRaceRunning = true;

        foreach (KeyValuePair<IRacer, Checkpoint> racerNextCheckpoint in m_RacerNextCheckpoints)
        {
            racerNextCheckpoint.Key.EnableControl();
            racerNextCheckpoint.Key.UnpauseTimer();
        }
    }

    /// <summary>
    /// Stops race (timers, etc) and disables control of all racers.
    /// </summary>
    public virtual void StopRace()
    {
        m_IsRaceRunning = false;

        foreach (KeyValuePair<IRacer, Checkpoint> racerNextCheckpoint in m_RacerNextCheckpoints)
        {
            racerNextCheckpoint.Key.DisableControl();
            racerNextCheckpoint.Key.PauseTimer();
        }

        onRaceComplete?.Invoke(true);
    }

    protected abstract bool CanEndRace();

    /// <summary>
    /// Method to get the event of a checkpoint triggered by a racer.
    /// </summary>
    protected abstract void OnCheckRacerHitCheckpoint(IRacer racer, Checkpoint checkpoint);
}
