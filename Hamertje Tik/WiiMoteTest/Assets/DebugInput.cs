﻿using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Text;
using Assets;
using System.IO;
using System.Collections.Generic;

public class DebugInput : MonoBehaviour {
    static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
    private WiiMote mote;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (mote != null)
        {
            Debug.Log(mote.wiiMoteState.currentState.A);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log(SystemInfo.operatingSystem);
        }
	if (Input.GetKeyDown(KeyCode.F12))
	{
	    HIDAPI api = HIDAPI.GetAPI();
	    api.Connect("/dev/input/js0");
	}
        if (Input.GetKeyDown(KeyCode.F1))
        {
            HIDAPI api = HIDAPI.GetAPI();
            Debug.Log("Checking");
            List<HIDDevice> list = api.GetDevices();
            List<WiiMote> motes = WiiMote.FindWiimotes(list);
            mote = motes[0];
            Debug.Log(motes.Count);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            mote.Initialise();            
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            mote.RequestStatus();
            mote.SetDataReportingMode(false, ReportingState.ButtonsAndAccel);
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            mote.StartReading();
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            mote.SetRumble(true);
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            mote.SetRumble(false);
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            mote.StopReading();
            Debug.Log("Disconnect: " + mote.Disconnect());
        }
	}
}
