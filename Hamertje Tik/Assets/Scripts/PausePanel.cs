using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour {
    public AudioClip clickSound;
    public Text playerText;

    public void Initialise(Player playerWhoPausedGame)
    {
        playerText.text = playerWhoPausedGame.GetPlayername();
    }

	public void HandleResume()
    {
        GameUIController.controller.ShowPause(false);
        GameUIController.controller.PlayFX(1, clickSound);
        GameLogicController.controller.PauseGame();
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
