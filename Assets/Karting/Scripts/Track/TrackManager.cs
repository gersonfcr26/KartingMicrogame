using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KartGame.KartSystems;
using UnityEngine;

namespace KartGame.Track
{
    /// <summary>
    /// A MonoBehaviour to deal with all the time and positions for the racers.
    /// </summary>
    public class TrackManager : RaceManager
    {
        [Header("Timetrial")]
        [Tooltip ("Number of laps for the race.")]
        public int raceLapTotal;
        [SerializeField, Tooltip("Reference to an object responsible for repositioning karts.")]
        protected KartRepositioner kartRepositioner;

        TrackRecord m_SessionBestLap = TrackRecord.CreateDefault ();
        TrackRecord m_SessionBestRace = TrackRecord.CreateDefault ();
        TrackRecord m_HistoricalBestLap;
        TrackRecord m_HistoricalBestRace;


        /// <summary>
        /// Returns the best lap time recorded this session.  If no record is found, -1 is returned.
        /// </summary>
        public float SessionBestLap
        {
            get
            {
                if (m_SessionBestLap != null && m_SessionBestLap.time < float.PositiveInfinity)
                    return m_SessionBestLap.time;
                return -1f;
            }
        }

        /// <summary>
        /// Returns the best race time recorded this session.  If no record is found, -1 is returned.
        /// </summary>
        public float SessionBestRace
        {
            get
            {
                if (m_SessionBestRace != null && m_SessionBestRace.time < float.PositiveInfinity)
                    return m_SessionBestRace.time;
                return -1f;
            }
        }

        /// <summary>
        /// Returns the best lap time ever recorded.  If no record is found, -1 is returned.
        /// </summary>
        public float HistoricalBestLap
        {
            get
            {
                if (m_HistoricalBestLap != null && m_HistoricalBestLap.time < float.PositiveInfinity)
                    return m_HistoricalBestLap.time;
                return -1f;
            }
        }

        /// <summary>
        /// Returns the best race time ever recorded.  If no record is found, -1 is returned.
        /// </summary>
        public float HistoricalBestRace
        {
            get
            {
                if (m_HistoricalBestRace != null && m_HistoricalBestRace.time < float.PositiveInfinity)
                    return m_HistoricalBestRace.time;
                return -1f;
            }
        }

        protected override void Awake ()
        {
            base.Awake();

            m_HistoricalBestLap = TrackRecord.Load (m_trackName, 1);
            m_HistoricalBestRace = TrackRecord.Load (m_trackName, raceLapTotal);
        }

        /// <summary>
        /// Initialize race manager.
        /// </summary>
        public override void InitRaceManager(List<IRacer> racers, System.Action<bool> onRaceComplete)
        {
            base.InitRaceManager(racers, onRaceComplete);
            print($"racers: {racers.Count}");
        }

        /// <summary>
        /// Starts the timers and enables control of all racers.
        /// </summary>
        public override void StartRace ()
        {
            base.StartRace();
        }

        /// <summary>
        /// Stops the timers and disables control of all racers, also saves the historical records.
        /// </summary>
        public override void StopRace ()
        {
            base.StopRace();

            TrackRecord.Save (m_HistoricalBestLap);
            TrackRecord.Save (m_HistoricalBestRace);

            onRaceComplete?.Invoke(true);
        }

        protected override void OnCheckRacerHitCheckpoint (IRacer racer, Checkpoint checkpoint)
        {
            if (!m_IsRaceRunning)
            {
                StartCoroutine (CallWhenRaceStarts (racer, checkpoint));
                return;
            }

            if (m_RacerNextCheckpoints.ContainsKeyValuePair (racer, checkpoint))
            {
                m_RacerNextCheckpoints[racer] = m_checkpoints.GetNextInCycle (checkpoint);
                RacerHitCorrectCheckpoint (racer, checkpoint);
            }
            else
            {
                RacerHitIncorrectCheckpoint (racer, checkpoint);
            }
        }

        IEnumerator CallWhenRaceStarts (IRacer racer, Checkpoint checkpoint)
        {
            while (!m_IsRaceRunning)
            {
                yield return null;
            }

            OnCheckRacerHitCheckpoint(racer, checkpoint);
        }

        void RacerHitCorrectCheckpoint (IRacer racer, Checkpoint checkpoint)
        {
            if (checkpoint.isStartFinishLine)
            {
                int racerCurrentLap = racer.GetCurrentLap ();
                if (racerCurrentLap > 0)
                {
                    float lapTime = racer.GetLapTime ();

                    if (m_SessionBestLap.time > lapTime)
                        m_SessionBestLap.SetRecord (m_trackName, 1, racer, lapTime);

                    if (m_HistoricalBestLap.time > lapTime)
                        m_HistoricalBestLap.SetRecord (m_trackName, 1, racer, lapTime);

                    if (racerCurrentLap == raceLapTotal)
                    {
                        float raceTime = racer.GetRaceTime ();

                        if (m_SessionBestRace.time > raceTime)
                            m_SessionBestRace.SetRecord (m_trackName, raceLapTotal, racer, raceTime);

                        if (m_HistoricalBestRace.time > raceTime)
                            m_HistoricalBestLap.SetRecord (m_trackName, raceLapTotal, racer, raceTime);

                        racer.DisableControl ();
                        racer.PauseTimer ();
                    }
                }

                if (CanEndRace ())
                    StopRace ();

                racer.HitStartFinishLine ();
            }
        }

        protected override bool CanEndRace ()
        {
            foreach (KeyValuePair<IRacer, Checkpoint> racerNextCheckpoint in m_RacerNextCheckpoints)
            {
                if (racerNextCheckpoint.Key.GetCurrentLap () < raceLapTotal)
                    return false;
            }

            return true;
        }

        private void RacerHitIncorrectCheckpoint (IRacer racer, Checkpoint checkpoint)
        {
            // No implementation required by template.
        }

        /// <summary>
        /// This function should be called when a kart gets stuck or falls off the track.
        /// It will find the last checkpoint the kart went through and reposition it there.
        /// </summary>
        /// <param name="movable">The movable representing the kart.</param>
        public void ReplaceMovable (IMovable movable)
        {
            IRacer racer = movable.GetRacer ();
            
            if(racer == null)
                return;
            
            Checkpoint nextCheckpoint = m_RacerNextCheckpoints[racer];
            int lastCheckpointIndex = (m_checkpoints.IndexOf (nextCheckpoint) + m_checkpoints.Count - 1) % m_checkpoints.Count;
            Checkpoint lastCheckpoint = m_checkpoints[lastCheckpointIndex];

            bool isControlled = movable.IsControlled ();
            movable.DisableControl ();
            kartRepositioner.OnRepositionComplete += ReenableControl;

            kartRepositioner.Reposition (lastCheckpoint, movable, isControlled);
        }

        private void ReenableControl (IMovable movable, bool doEnableControl)
        {
            if(doEnableControl)
                movable.EnableControl ();
            kartRepositioner.OnRepositionComplete -= ReenableControl;
        }
    }
}