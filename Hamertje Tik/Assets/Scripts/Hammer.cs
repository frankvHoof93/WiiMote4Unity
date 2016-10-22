using UnityEngine;
using System.Collections;
using System;

enum ControllerMode
{
    None,
    Controller,
    Wiimote
};

public class Hammer : MonoBehaviour {

    public float speed = 0.25f;
    public float yMoveMax = 2.5f;
    public float xMoveMax;
    public float zMoveMax;
    public AudioClip swingSound;
    Vector3 prevMousePos;
    Player myPlayer;
    bool isHitting;
    bool movingDown;
    Vector3 defaultPos;
    WiiMote mote;
    bool damaged = false;
    ControllerMode mode = ControllerMode.None;
    int controller = 0;
    bool prevStart = false;
    float vibrate = 0;

	// Use this for initialization
	void Awake () {
        prevMousePos = Input.mousePosition;
        defaultPos = transform.position;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        Vector3 toMove = Vector3.zero;
        if (vibrate < 300)
            vibrate++;
        if (mode == ControllerMode.None) // DEBUGINPUT.. REMOVE THIS
        {
            if (vibrate > 10)
                damaged = false;
            Vector3 currMousePos = Input.mousePosition;
            if (Input.GetKeyDown(KeyCode.Escape) && !prevStart)
            {
                GameLogicController.controller.PauseGame();
                if (GameLogicController.controller.currentGameState == GameState.Paused)
                    GameUIController.controller.ShowPause(true, myPlayer);
                else GameUIController.controller.ShowPause(false);
            }
            prevStart = Input.GetKeyDown(KeyCode.Escape);
            if (!damaged)
            {
                toMove = new Vector3((float)((currMousePos.x - prevMousePos.x) * 0.1), 0, (float)((currMousePos.y - prevMousePos.y) * 0.1));
                if (GameLogicController.controller.currentGameState == GameState.Running)
                {
                    if (Input.GetMouseButtonDown(0)) DoHit();
                }
            }
            prevMousePos = currMousePos;            
        }
        else if (mode == ControllerMode.Controller)
        {
            if (vibrate > 10)
                damaged = false;
            string controller = "Contr" + this.controller;
            bool pressStart = Input.GetAxis(controller + "Start") == 1f;

            if (pressStart && !prevStart)
            {
                GameLogicController.controller.PauseGame();
                if (GameLogicController.controller.currentGameState == GameState.Paused)
                    GameUIController.controller.ShowPause(true, myPlayer);
                else GameUIController.controller.ShowPause(false);
            }
            if (!damaged)
            {
                float Xmove = Input.GetAxis(controller + "Hor") * 0.25f;
                float Ymove = Input.GetAxis(controller + "Vert") * 0.25f;
                bool pressX = Input.GetAxis(controller + "X") == 1f;
                toMove = new Vector3(-Xmove, 0, Ymove);
                if (GameLogicController.controller.currentGameState == GameState.Running)
                { if (pressX) DoHit(); }
            }
            prevStart = pressStart;
        }
        else if (mode == ControllerMode.Wiimote)
        {
            ButtonState prevState = mote.wiiMoteState.GetPreviousState();
            ButtonState currState = mote.wiiMoteState.GetCurrentState();
            toMove = new Vector3(0, 0, 0);
            if (mote.wiiMoteState.RumbleEnabled && vibrate > 10)
            {
                damaged = false;
                mote.SetRumble(false);
            }

            if (!damaged)
            {
                if (GameLogicController.controller.currentGameState == GameState.Running && currState.B && !prevState.B)
                    DoHit();
                if (currState.Up)
                {
                    toMove.z = 0.25f;
                }
                else if (currState.Down)
                {
                    toMove.z = -0.25f;
                }
                if (currState.Left)
                {
                    toMove.x = -0.25f;
                }
                else if (currState.Right)
                {
                    toMove.x = 0.25f;
                }
            }
            if (currState.Plus && !prevState.Plus)
            {
                GameLogicController.controller.PauseGame();
                if (GameLogicController.controller.currentGameState == GameState.Paused)
                    GameUIController.controller.ShowPause(true, myPlayer);
                else GameUIController.controller.ShowPause(false);
            }

        }

        transform.Translate(toMove);
        float x = Mathf.Clamp(transform.localPosition.x, defaultPos.x - xMoveMax, defaultPos.x + xMoveMax);
        float z = Mathf.Clamp(transform.localPosition.z, defaultPos.z - zMoveMax, defaultPos.z + zMoveMax);
        transform.localPosition = new Vector3(x, transform.localPosition.y, z);

        if (isHitting)
        {
            if (movingDown)
            {
                if (transform.position.y + yMoveMax >= defaultPos.y)
                    transform.Translate(Vector3.down * speed);
                else movingDown = false;
            }
            else
            {
                if (transform.position.y < defaultPos.y)
                    transform.Translate(Vector3.up * speed);
                else isHitting = false;
            }
        }
	}

    public void SetPosition(Vector3 position)
    {
        defaultPos = position;
        transform.position = position;
    }

    void OnTriggerEnter(Collider col)
    {
        Hittable t = col.gameObject.GetComponent<Hittable>();
        if (GameLogicController.controller.currentGameState == GameState.Running)
        {
            if (t.GetType() == typeof(Dugtrio))
                myPlayer.AddSuper();
            else if (t.GetType() == typeof(Voltorb))
                MachineController.controller.SpawnAtOthers(Enemies.Dugtrio, myPlayer);
            else if (t.GetType() == typeof(Pinsir))
                myPlayer.AddBomb();
            GameUIController.controller.PlayFX(myPlayer.GetPlayerNumber(), t.soundHit);
        }
        if (movingDown && t != null)
        {
            AddPoints(t.Hit());
            movingDown = false;
            if (mode == ControllerMode.Wiimote)
            {
                if (vibrate > 10)
                    vibrate = 0;
                else vibrate -= 10;
                mote.SetRumble(true);
            }
        }
        GameUIController.controller.UpdateUI();
    }

    void AddPoints(float points)
    {
        myPlayer.AddPoints(points);
    }

    public void SetPlayer(Player player)
    {
        this.myPlayer = player;
        defaultPos = this.transform.position;
    }

    public void DoHit()
    {
        if (isHitting) return;
        GameUIController.controller.PlayFX(myPlayer.playerNumber, swingSound);
        isHitting = true;
        movingDown = true;
    }

    public void SetWiimote(WiiMote wiimote) {
        
        string playerLeds = Convert.ToString(this.myPlayer.playerNumber, 2);
        playerLeds.PadLeft(4, '0');
        this.mote = wiimote;
        this.mode = ControllerMode.Wiimote;
        mote.Initialise();
        mote.SetDataReportingMode(true, ReportingState.ButtonsAndAccel);
        mote.SetPlayerNumber(this.myPlayer.playerNumber);
        mote.StartReading();
    }

    public void SetController(int number) 
    {
        this.controller = number;
        this.mode = ControllerMode.Controller;
    }

    public void Damage() 
    { 
        damaged = true;
        vibrate = -170;
    }

    void OnDestroy()
    {
        if (mote != null)
        {
            mote.StopReading();
            mote.Disconnect();
        }
    }
}
