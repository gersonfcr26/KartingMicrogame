using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using KartGame.Track;
using KartGame.UI;
using KartGame.KartSystems;

public enum GameMode
{
    Timetrial,
    Rush
}

[Serializable]
public class RaceManagerModel
{
    public GameMode gameMode;
    public RaceManager raceManager;
}

public class LevelBuilder : MonoBehaviour
{
    [SerializeField]
    private List<RaceManagerModel> m_raceManagers;
    [SerializeField]
    private MetaGameController m_gameController;
    [SerializeField]
    private MainUIController m_mainUIController;
    [SerializeField]
    private CinemachineVirtualCamera m_virtualCam;
    [SerializeField]
    private GameObject m_racerPrefab;
    [SerializeField]
    private GameObject m_collectableTimersParent;
    [SerializeField]
    private SlotsController m_slotsCtrl;
    [SerializeField]
    private Transform m_racerParent;
    [SerializeField]
    private int m_opponentsAmount;

    // Internal 
    private int m_racersAmount;
    private List<GameObject> m_racerInstances;
    private List<IRacer> m_racers;
    private RaceManager m_raceManager;

    private void Awake()
    {
        m_racersAmount = m_opponentsAmount + 1;
        // Create reference lists
        m_racerInstances = new List<GameObject>(m_racersAmount);
        m_racers = new List<IRacer>(m_racersAmount);
    }

    private void BuildLevel()
    {
        // Get slot (grid) info
        var slots = m_slotsCtrl.GetSlots();
        if (m_racersAmount > slots.Count)
        {
            Debug.LogError("Not enough slots.");
            return;
        }

        // Destroy previous racers
        for (int i = 0; i < m_racerInstances.Count; i++)
        {
            Destroy(m_racerInstances[i]);
        }

        // Clear references lists
        m_racerInstances.Clear();
        m_racers.Clear();

        for (int i = 0; i < m_racersAmount; i++)
        {
            // Set racers grid position
            var newRacer = Instantiate(m_racerPrefab, slots[i].position, slots[i].rotation, m_racerParent);
            
            // Set player features
            if (i == 0)
            {
                // Set specs delegates
                var specsModifier = newRacer.GetComponent<KartSpecsModifier>();
                m_mainUIController.onMaxSpeedChange = specsModifier.SetMaxSpeed;
                m_mainUIController.onAccelChange = specsModifier.SetAccelaration;
                m_mainUIController.onBrakingChange = specsModifier.SetBraking;
                m_mainUIController.onSteeringChange = specsModifier.SetSteering;
                // Apply specs initial values
                specsModifier.SetMaxSpeed(m_mainUIController.currMaxSpeedValue);
                specsModifier.SetAccelaration(m_mainUIController.currAccelValue);
                specsModifier.SetBraking(m_mainUIController.currBrakingValue);
                specsModifier.SetSteering(m_mainUIController.currSteeringValue);
            }
            // Set opponents AI input
            else
            {
                // Set AI 
                var kartMovement = newRacer.GetComponent<KartMovement>();
                var simpleAIinput = newRacer.GetComponent<SimpleAIInput>();
                kartMovement.SetInput(simpleAIinput);
                // Set AI delegate
                m_mainUIController.onAISelected = simpleAIinput.SetAIDifficulty;
                // Apply initial AI
                simpleAIinput.SetAIDifficulty(m_mainUIController.currAI);
            }
            // Add racer
            m_racerInstances.Add(newRacer);
            m_racers.Add(m_racerInstances[i].GetComponent<IRacer>());

            // Set camera to first racer (player)
            if (i == 0)
            {
                m_virtualCam.LookAt = newRacer.transform;
                m_virtualCam.Follow = newRacer.transform;
            }
        }

        // Disable collectable timers
        m_collectableTimersParent.SetActive(false);

        // Reset start countdown
        m_gameController.ToggleMainMenu(false);
        m_gameController.raceCountdownTrigger.ResetTrigger();
        m_gameController.raceCountdownTrigger.TriggerDirector();
    }
    /// <summary>
    /// Unity Event called when player choose game mode.
    /// </summary>
    public void SetRaceTimetrialMode()
    {
        BuildLevel();
        // Get manager
        m_raceManager = m_raceManagers.Find(x => x.gameMode == GameMode.Timetrial).raceManager;
        // Initialize manager
        m_raceManager.InitRaceManager(m_racers, OnRaceComplete);
        // Enable timetrial view
        m_mainUIController.EnableTimetrialView(m_racers[0]);
    }

    /// <summary>
    /// Unity Event called when player choose game mode.
    /// </summary>
    public void SetRaceRushMode()
    {
        BuildLevel();
        // Get manager
        m_raceManager = m_raceManagers.Find(x => x.gameMode == GameMode.Rush).raceManager;
        // Initialize manager
        m_raceManager.InitRaceManager(m_racers, OnRaceComplete);
        // Enable collectable timers
        m_collectableTimersParent.SetActive(true);
        // Enable rush countdown view
        m_mainUIController.EnableCountdownView();
    }

    /// <summary>
    /// Unity Event called when countdown to start is complete.
    /// </summary>
    public void OnCountdownComplete()
    {
        m_raceManager.StartRace();
    }

    private void OnRaceComplete(bool isVictory)
    {
        StartCoroutine(ShowMainMenu());
    }

    private IEnumerator ShowMainMenu()
    {
        yield return new WaitForSeconds(1f);

        m_gameController.ToggleMainMenu(true);
        m_mainUIController.SetActivePanel(0);
    } 
}
