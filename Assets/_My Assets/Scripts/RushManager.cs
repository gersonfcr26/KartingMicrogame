using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using KartGame.Track;

public class RushManager : RaceManager
{
    [Header("Rush mode")]
    [SerializeField, Tooltip("Initial time to start countdown")]
    private float m_initialTime;
    [SerializeField]
    private int m_lapsRequired = 1;
    [SerializeField]
    private CountdownView m_countdownView;

    /// <summary>
    /// Initialize race manager.
    /// </summary>
    public override void InitRaceManager(List<IRacer> racers, System.Action<bool> onRaceComplete)
    {
        base.InitRaceManager(racers, onRaceComplete);

        for (int i = 0; i < racers.Count; i++)
        {
            // Set countdown time
            racers[i].SetCountdownTimer(m_initialTime, i == 0 ? OnUpdateCountdown : (Action<float>)null);
        }
    }

    /// <summary>
    /// Starts the countdown and enables control of all racers.
    /// </summary>
    public override void StartRace()
    {
        base.StartRace();
    }

    /// <summary>
    /// Stops the countdown and disables control of all racers.
    /// </summary>
    public override void StopRace()
    {
        base.StopRace();

        onRaceComplete?.Invoke(true);
    }

    protected override void OnCheckRacerHitCheckpoint(IRacer racer, Checkpoint checkpoint)
    {
        // Wait for race to start
        if (!m_IsRaceRunning)
        {
            StartCoroutine(CallWhenRaceStarts(racer, checkpoint));
            return;
        }

        // Lap completed
        if (checkpoint.isStartFinishLine)
        {
            racer.HitStartFinishLine();

            // Stop timer and disable racer
            if (racer.GetCurrentLap() > m_lapsRequired)
            {
                racer.DisableControl();
                racer.PauseTimer(); 
            }

            // Check if all racers completed the race
            if (CanEndRace())
            {
                StopRace();
            }
        }
    }

    protected override bool CanEndRace()
    {
        foreach (KeyValuePair<IRacer, Checkpoint> racerNextCheckpoint in m_RacerNextCheckpoints)
        {
            if (racerNextCheckpoint.Key == m_racerPlayer && racerNextCheckpoint.Key.GetCurrentLap() > m_lapsRequired)
                return true;
        }

        return false;
    }

    private IEnumerator CallWhenRaceStarts(IRacer racer, Checkpoint checkpoint)
    {
        while (!m_IsRaceRunning)
        {
            yield return null;
        }

        OnCheckRacerHitCheckpoint(racer, checkpoint);
    }

    private void OnUpdateCountdown(float value)
    {
        // Game  over
        if(value == 0)
        {
            StopRace();
        }

        m_countdownView.SetCountdownView(value);
    }
}
