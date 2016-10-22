using UnityEngine;
using System.Collections;

public class Machine : MonoBehaviour {

    private Player myPlayer;
    private MoleController myMoleController;
    private Camera myCamera;

	// Use this for initialization
	void Awake () {
        myPlayer = gameObject.GetComponent<Player>();
        myMoleController = gameObject.GetComponent<MoleController>();
        myMoleController.SetMachine(this);
	}

    public void Reset()
    {
        myMoleController.Reset();
        myPlayer.Reset();
    }

    public Player GetPlayer()
    {
        return myPlayer;
    }

    public void SetCamera(Camera c)
    {
        myCamera = c;
    }

    public Camera GetCamera()
    {
        return myCamera;
    }

    public MoleController GetMoleController()
    {
        return myMoleController;
    }

    public void SetPlayerNumber(int playernumber)
    {
        myPlayer.SetPlayerNumber(playernumber);
    }
}
