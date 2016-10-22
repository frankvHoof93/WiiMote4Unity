using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameOverPanel : MonoBehaviour {
    public AudioClip clickSound;
    public Text playerText;
    public Text pointsText;

    public void Initialise(Player playerWhoWon)
    {
        playerText.text = playerWhoWon.GetPlayername();
        pointsText.text = playerWhoWon.GetPoints().ToString();
    }

    public void HandleRestart()
    {
        GameUIController.controller.PlayFX(1, clickSound);
        GameUIController.controller.ShowPause(false);
        GameLogicController.controller.RestartGame();
    }

    public void HandleQuitGame()
    {
        GameUIController.controller.PlayFX(1, clickSound);
        GameUIController.controller.ExitGame();
    }
}
