using UnityEngine;
using System.Collections;

public enum GameState
{
    Starting,
    Running,
    Paused,
    Gameover
};

public class GameLogicController : MonoBehaviour {
    public static GameLogicController controller;
    public float timer;// { get; private set; }
    public AudioClip gameStartAudio;
    public AudioClip gameOverAudio;
    public GameState currentGameState;// { get; private set; }

	void Awake () {
        if (controller != null && controller != this)
            Destroy(this.gameObject);
        timer = 60;
        controller = this;
        currentGameState = GameState.Starting;
	}

    void Start()
    {
        gameObject.GetComponent<StartGameTimer>().StartGame();
        GameUIController.controller.PlayFX(1, gameStartAudio);
    }
	
	void FixedUpdate () {
        switch (currentGameState)
        {
            case GameState.Starting:
                break;
            case GameState.Paused:
                break;
            case GameState.Gameover:
                break;
            case GameState.Running:
                timer -= Time.fixedDeltaTime;
                if (timer <= 0f)
                    HandleGameOver();
                break;
        }
	}

    public void StartGame()
    {
        if (currentGameState == GameState.Starting || currentGameState == GameState.Gameover)
            currentGameState = GameState.Running;
    }

    public void PauseGame()
    {
        if (currentGameState == GameState.Running)
            currentGameState = GameState.Paused;
        else if (currentGameState == GameState.Paused)
            currentGameState = GameState.Running;
    }

    public void RestartGame()
    {
        MachineController.controller.ResetMachines();
        currentGameState = GameState.Starting;
        timer = 60;
        if (gameObject.GetComponent<StartGameTimer>() == null)
            gameObject.AddComponent<StartGameTimer>();
        Start();
        GameUIController.controller.UpdateUI();
        GameUIController.controller.ResetMusic();
    }

    void HandleGameOver()
    {
        GameUIController.controller.PlayFX(1, gameOverAudio);
        currentGameState = GameState.Gameover;
        float highestScore = -1f;
        Player winner = null;
        foreach (Player player in MachineController.controller.GetPlayers())
        {
            if (player.GetPoints() > highestScore)
            {
                highestScore = player.GetPoints();
                winner = player;
            }
        }
        GameUIController.controller.SetGameOver(winner);
    }
}
