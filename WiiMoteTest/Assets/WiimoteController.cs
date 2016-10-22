using UnityEngine;
using System.Collections.Generic;
using Assets;

public class WiimoteController : MonoBehaviour {
    private const string VID_NINTENDO = "vid&0002057e";
    private const string PID_WIIMOTE = "pid&0306"; // BOTH NORMAL AND PLUS ARE GIVING SAME PID (WHICH THEY SHOULDNT)
    public List<string> connectedWiimotes {get; private set;}
    private HIDAPI api;

	// Use this for initialization
	void Start () {
        api = HIDAPI.GetAPI();
        connectedWiimotes = new List<string>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void GetNewWiiMote()
    {
        List<HIDDevice> devices = api.GetDevices();
        List<WiiMote> availableMotes = FindWiimotes(devices);
        for (int i = 0; i < availableMotes.Count; i++)
        {
            if (!connectedWiimotes.Contains(availableMotes[i].devicePath))
            {
                connectedWiimotes.Add(availableMotes[i].devicePath);
                Debug.Log("Wiimote Added");
                break;
            }
        }
    }

    public static List<WiiMote> FindWiimotes(List<HIDDevice> devices)
    {
        List<WiiMote> wiiMotes = new List<WiiMote>();
        foreach (HIDDevice dev in devices)
        {
            Debug.Log(dev.devicePath);
            if (dev.devicePath.Contains(VID_NINTENDO))
                if (dev.devicePath.Contains(PID_WIIMOTE)) // TODO ADD OTHER PID's (IF EVER FOUND)
                    wiiMotes.Add(new WiiMote(dev.devicePath, HIDAPI.GetAPI()));
        }
        return wiiMotes;
    }
}
