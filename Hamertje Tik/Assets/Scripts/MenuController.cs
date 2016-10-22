using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MenuController : MonoBehaviour {
    public static MenuController controller;
    public GameObject optionsLoaderPrefab;
    public GameObject playerPanelPrefab;
    public GameObject pickPlayer;
    public GameObject mainMenu;
    public Canvas canvas;
    public Slider playerSlider;
    public Text playerNumberText;
    public Slider difficultySlider;
    public Text difficultyText;
    public AudioClip menuMusic;
    public AudioSource FXSource;
    public AudioSource musicSource;
    private ArrayList playerPanels = new ArrayList();
    private int players = 0;
    private int connectedControllers = 0;
    private Dictionary<int, int> Controllers = new Dictionary<int,int>();
    private Dictionary<int, int> WiiMotes = new Dictionary<int,int>();

	// Use this for initialization
	void Awake () {
        if (controller != null && controller != this)
            Destroy(this.gameObject);
        controller = this;
	}

    void Start()
    {
        SetDifficulty();
        SetPlayers();
        OpenMainMenu();
        PlayMusic();
    }

    public void OpenPickPlayer()
    {
        mainMenu.SetActive(false);
        foreach (GameObject obj in playerPanels)
        {
            obj.SetActive(true);
        }
        pickPlayer.SetActive(true);
    }

    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        foreach (GameObject obj in playerPanels)
        {
            obj.SetActive(false);
        }
        pickPlayer.SetActive(false);
    }

    public int GetController(int playerNumber)
    {
        int value = 0;
        return Controllers.TryGetValue(playerNumber, out value) ? value : 0;
    }

    public string GetWiimote(int playerNumber)
    {
        string value;
        return WiimoteController.controller.connectedWiimotes.TryGetValue(playerNumber, out value) ? value : null;
    }

    public void SetDifficulty()
    {
        difficultyText.text = difficultySlider.value.ToString();
    }

    public void SetPlayers()
    {
        playerNumberText.text = playerSlider.value.ToString();
        int playerNumber = (int)playerSlider.value;
        if (playerNumber > this.players)
        {
            for (int i = 0; i < playerNumber - players; i++)
            {
                GameObject obj = Instantiate(playerPanelPrefab);
                obj.transform.SetParent(canvas.transform);
                playerPanels.Add(obj);
                obj.GetComponent<PlayerPanel>().SetPlayerNumber(playerPanels.IndexOf(obj) + 1);
            }
        }
        else if (playerNumber < this.players)
        {
            for (int i = 0; i < players - playerNumber; i++)
            {
                Object objToDestroy = (Object)playerPanels[playerPanels.Count - 1];
                playerPanels.Remove(objToDestroy);
                Destroy(objToDestroy);
            }
        }
        this.players = playerNumber;

        int middle = playerNumber / 2;
        if (playerNumber % 2 != 0)
            middle += 1;
        for (int i = 0; i < playerNumber; i++)
        {
            GameObject obj = (GameObject)playerPanels[i];
            RectTransform panelTransform = (RectTransform)obj.transform;
            float x, y;
            if (playerNumber >= 4)
            {
                y = (i >= middle ? 0 : 1);
                if (i >= middle)
                    x = 0 + ((1f / (middle - 1)) * (i - middle));
                else x = 0 + ((1f / (middle - 1)) * i);
            }
            else
            {
                x = 0;
                y = 1;
                switch (playerNumber)
                {
                    case 1:
                        x = 0.5f;
                        break;
                    case 2:
                        if (i == 0) x = 0.25f;
                        else x = 0.75f;
                        break;
                    case 3:
                        if (i == 0) x = 0f;
                        else if (i == 1) x = 0.5f;
                        else x = 1f;
                        break;
                }
            }
            panelTransform.anchorMin = new Vector2(x, y);
            panelTransform.anchorMax = new Vector2(x, y);
            panelTransform.anchoredPosition = new Vector3(0, 0);
            panelTransform.position = Vector3.MoveTowards(new Vector3(panelTransform.position.x, panelTransform.position.y, 0), new Vector3(panelTransform.position.x, Screen.height / 2, 0), 100f);
            if (x == 1f/4f || x == 3f/4f || x == 1f/3f || x == 2f/3f)
                panelTransform.position = Vector3.MoveTowards(new Vector3(panelTransform.position.x, panelTransform.position.y, 0), new Vector3(Screen.width / 2, panelTransform.position.y, 0), 33f);
            else
                panelTransform.position = Vector3.MoveTowards(new Vector3(panelTransform.position.x, panelTransform.position.y, 0), new Vector3(Screen.width / 2, panelTransform.position.y, 0), 100f);
        }
        Canvas.ForceUpdateCanvases();
    }

    void PlayMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
        }
        musicSource.clip = menuMusic;
        musicSource.Play();
    }

    public void PlayEffect(AudioClip clip)
    {
        FXSource.clip = clip;
        FXSource.Play();
    }

    public bool SetWiimote(int playerNumber)
    {
        return WiimoteController.controller.GetNewWiiMote(playerNumber);
    }

    public void StartGame()
    {
        OptionsLoader options = Instantiate(optionsLoaderPrefab).GetComponent<OptionsLoader>();
        options.SetPlayers((int)playerSlider.value);
        options.SetDifficulty((int)difficultySlider.value);
        options.SetControllers(Controllers);
        options.SetWiiMotes(WiimoteController.controller.connectedWiimotes);
        foreach (GameObject obj in playerPanels)
        {
            PlayerPanel panel = obj.GetComponent<PlayerPanel>();
            if (panel != null)
            if (panel.playerNameText.text != null && panel.playerNameText.text != "")
            {
                options.AddName(panel.GetPlayerNumber(), panel.playerNameText.text);
            }
        }
        DontDestroyOnLoad(options);
        Application.LoadLevel(1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public int AddController(int playerNumber)
    {
        connectedControllers += 1;
        Controllers.Add(playerNumber, connectedControllers);
        return connectedControllers;
    }
}
