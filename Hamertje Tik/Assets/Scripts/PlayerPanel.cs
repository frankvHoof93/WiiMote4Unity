using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerPanel : MonoBehaviour {
    public Text playerNameText;
    public Text playerPreviewText;
    public Text controllerText;
    public Button controllerButton;
    public Button moteButton;
    private int playerNumber;

    public void SetPlayerNumber(int player)
    {
        this.playerNumber = player;
        this.playerPreviewText.text = "Player " + player;
    }

    public int GetPlayerNumber()
    { return playerNumber; }

    public void AddWiimote(int controller = 0)
    {
        bool success;
        if (controller == 0)
            success = MenuController.controller.SetWiimote(playerNumber);
        else success = MenuController.controller.SetWiimote(controller);
        if (success)
        {
            controllerButton.gameObject.SetActive(false);
            moteButton.gameObject.SetActive(false);
            controllerText.text = "WiiMote Set";
        }
        else Debug.Log("Error, no (new) Wiimotes found.");
    }

    public void PlaySound(AudioClip sound)
    {
        MenuController.controller.PlayEffect(sound);
    }

    public void AddController(int controller = 0)
    {
        if (controller == 0)
            controller = MenuController.controller.AddController(playerNumber);
        controllerButton.gameObject.SetActive(false);
        moteButton.gameObject.SetActive(false);
        controllerText.text = "Controller " + controller + " Set";
    }
}
