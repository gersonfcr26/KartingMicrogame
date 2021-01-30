using KartGame.Track;
using UnityEngine;

namespace KartGame.UI
{
    /// <summary>
    /// A MonoBehaviour for controlling the different panels of the main menu.
    /// </summary>
    public class MainUIController : MonoBehaviour
    {
        [SerializeField, Tooltip("A collection of UI panels, one of which will be active at a time.")]
        private GameObject[] panels;
        [SerializeField]
        private TimeDisplay m_timeDisplay;
        [SerializeField]
        private CountdownView m_countdownView;

        // Internal
        private RaceManager m_raceManager;
        private GameMode m_gameMode;

        public bool isPausable { get; set; }

        private void OnEnable()
        {
            SetActivePanel(0);
            m_timeDisplay.gameObject.SetActive(false);
            m_countdownView.gameObject.SetActive(false);
        }

        /// <summary>
        /// Turns off all the panels except the one at the given index which is turned on.
        /// </summary>
        public void SetActivePanel(int index)
        {
            for (var i = 0; i < panels.Length; i++)
            {
                bool active = i == index;
                GameObject panel = panels[i];
                if (panel.activeSelf != active)
                    panel.SetActive(active);
            }
        }

        /// <summary>
        /// Set view for timetrial race mode.
        /// </summary>
        public void EnableTimetrialView(IRacer racer)
        {
            m_timeDisplay.gameObject.SetActive(true);
            m_timeDisplay.RebindRacer(racer);
        }

        /// <summary>
        /// Set view for rush race mode.
        /// </summary>
        public void EnableCountdownView()
        {
            m_countdownView.gameObject.SetActive(true);
        }
    }
}