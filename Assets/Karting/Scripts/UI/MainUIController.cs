using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using KartGame.Track;
using TMPro;

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

        [Header("Spec Sliders")]
        [SerializeField]
        private Slider m_maxSpeedSlider;
        [SerializeField]
        private Slider m_accelSlider;
        [SerializeField]
        private Slider m_brakesSlider;
        [SerializeField]
        private Slider m_steeringSlider;

        [Header("Dropdown AI")]
        [SerializeField]
        private TMP_Dropdown m_AIDropdown;

        [Header("Dropdown Custom")]
        [SerializeField]
        private TMP_Dropdown m_wheelsDropdown;
        [SerializeField]
        private TMP_Dropdown m_hatDropdown;
        [SerializeField]
        private TMP_Dropdown m_kartDropdown;
        [SerializeField]
        private TMP_Dropdown m_characterDropdown;

        // Internal
        private RaceManager m_raceManager;
        private GameMode m_gameMode;

        public Action<float> onMaxSpeedChange;
        public Action<float> onAccelChange;
        public Action<float> onBrakingChange;
        public Action<float> onSteeringChange;
        public Action<int> onAISelected;

        public bool isPausable { get; set; }

        // Kart specs accessors
        public float currMaxSpeedValue => m_maxSpeedSlider.value;
        public float currAccelValue => m_accelSlider.value;
        public float currBrakingValue => m_brakesSlider.value;
        public float currSteeringValue => m_steeringSlider.value;
        
        // AI accessors
        public int currAI => m_AIDropdown.value;

        // Custom accessors
        public int wheelsSelectedIndex => m_wheelsDropdown.value;
        public int hatSelectedIndex => m_hatDropdown.value;
        public int kartSelectedIndex => m_kartDropdown.value;
        public int characterSelectedIndex => m_characterDropdown.value;

        private void Awake()
        {
            // Set AI dropdown
            m_AIDropdown.ClearOptions();
            List<string> options = new List<string>((int)AIDifficulty.Count);
            for(int i = 0; i < (int)AIDifficulty.Count; i++)
            {
                options.Add(((AIDifficulty)i).ToString());
            }
            m_AIDropdown.AddOptions(options);
        }

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

        /// <summary>
        /// Unity event from settings menu.
        /// </summary>
        public void OnMaxSpeedChanged(float value)
        {
            onMaxSpeedChange?.Invoke(value);
        }

        /// <summary>
        /// Unity event from settings menu.
        /// </summary>
        public void OnAccelChanged(float value)
        {
            onAccelChange?.Invoke(value);
        }

        /// <summary>
        /// Unity event from settings menu.
        /// </summary>
        public void OnBrakingChanged(float value)
        {
            onBrakingChange?.Invoke(value);
        }

        /// <summary>
        /// Unity event from settings menu.
        /// </summary>
        public void OnSteeringChanged(float value)
        {
            onSteeringChange?.Invoke(value);
        }

        /// <summary>
        /// Unity event from settings menu.
        /// </summary>
        public void OnAISelected(int selection)
        {
            onAISelected?.Invoke(selection);
        }
    }
}