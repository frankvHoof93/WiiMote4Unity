using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameUIController : MonoBehaviour {
    public static GameUIController controller;
    public GameObject singlePlayerUI;
    public GameObject multiPlayerUI;
    public GameObject multiPlayerClock;
    public GameObject pauseCanvas;
    public GameObject gameOverCanvas;
    public AudioClip music;
    private HUDController[] activeUI;
    private Text activeClock;
    public Dictionary<int, AudioSource> machineSpeakers;
    private AudioSource musicSource;
    private GameObject activePauseCanvas;
    private GameObject activeGameOverCanvas;

	// Use this for initialization
	void Awake () {

        if (controller != null && controller != this)
            Destroy(this.gameObject);
        controller = this;
        machineSpeakers = new Dictionary<int, AudioSource>();
        
	}

    void Start()
    {
        SetAudio();
        SetGUI();
        PlayMusic(music);
    }
	
    public void SetGameOver(Player winner)
    {
        activeClock.text = "Game Over";
        activeGameOverCanvas = Instantiate(gameOverCanvas);
        activeGameOverCanvas.GetComponent<GameOverPanel>().Initialise(winner);
    }

    void SetGUI()
    {
        if (MachineController.controller.IsSinglePlayer())
        {
            activeUI = new HUDController[1];
            activeUI[0] = Instantiate(singlePlayerUI).GetComponent<HUDController>();
            activeUI[0].SetPlayer(MachineController.controller.GetMachines()[0]);
            activeUI[0].SetUI();
            Canvas.ForceUpdateCanvases();
            activeClock = activeUI[0].transform.Find("Clock").GetComponent<Text>();
        }
        else
        {
            int players = MachineController.controller.GetPlayers().Count;
            GameObject clock = Instantiate(multiPlayerClock).transform.FindChild("ClockPanel").gameObject;
            activeClock = clock.GetComponentInChildren<Text>();
            if (players == 3)
            {
                clock.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
                clock.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
                clock.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50f);
            }
            activeUI = new HUDController[players];
            for (int i = 0; i < players; i++)
            {
                activeUI[i] = Instantiate(multiPlayerUI).GetComponent<HUDController>();
                activeUI[i].SetPlayer(MachineController.controller.GetMachines()[i]);
                activeUI[i].SetUI();
            }
            Canvas.ForceUpdateCanvases();
        }
        UpdateUI();
    }

    void SetAudio()
    {
        int players = MachineController.controller.GetPlayers().Count;
        List<Machine> machines = MachineController.controller.GetMachines();
        for (int i = 0; i < players; i++)
        {
            machineSpeakers.Add(i + 1, machines[i].GetComponent<AudioSource>());
        }
        musicSource = MachineController.controller.GetComponent<AudioSource>();
    }

    void Update()
    {
        float time = GameLogicController.controller.timer;
        if (time >= 0)
        {
            if (GameLogicController.controller.currentGameState == GameState.Starting)
            {
                if (activeGameOverCanvas != null) Destroy(activeGameOverCanvas);
                activeClock.text = ((int)time).ToString();
            }
            else
                activeClock.text = ((int)time + 1).ToString();
        }
    }

    public void UpdateUI()
    {
        for (int i = 0; i < activeUI.Length; i++)
            activeUI[i].UpdateUI();
    }

    public void ExitGame()
    {
        Application.LoadLevel(0);
    }

    public void ResetMusic()
    {
        musicSource.Stop();
        musicSource.Play();
    }

    public void PlayMusic(AudioClip music)
    {
        if (musicSource.isPlaying)
            musicSource.Pause();
        musicSource.clip = music;
        musicSource.Play();
    }

    public void PlayFX(int player, AudioClip clip)
    {
        AudioSource source = machineSpeakers[player];
        if (source.isPlaying)
        {
            AudioSource newSource = source.gameObject.AddComponent<AudioSource>();
            newSource.clip = clip;
            newSource.Play();
            Destroy(newSource, clip.length + 0.2f);
        }
        else
        {
            source.clip = clip;
            source.Play();
        }
    }

    public void ShowPause(bool pause, Player playerWhoPaused = null)
    {
        if (pause)
        {
            activePauseCanvas = Instantiate(pauseCanvas);
            activePauseCanvas.GetComponent<PausePanel>().Initialise(playerWhoPaused);
        }
        else
        {
            Destroy(activePauseCanvas);
        }
    }
}
