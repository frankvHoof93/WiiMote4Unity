using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MachineController : MonoBehaviour {

    public static MachineController controller;
    public GameObject machinePrefab;
    public Transform absenteeCameraTransform;

    private List<Machine> machines = new List<Machine>();
    private int playerNumber;
    private int difficulty;

    void Awake()
    {
        if (controller != null && controller != this)
            Destroy(this.gameObject);
        controller = this;
        OptionsLoader options = GameObject.FindObjectOfType<OptionsLoader>();
        this.difficulty = options.GetDifficulty();
        this.playerNumber = options.GetPlayers();
        for (int i = 0; i < playerNumber; i++)
        {
            GameObject obj = (GameObject)Instantiate(machinePrefab, Vector3.back * ((playerNumber + 1) * 5), Quaternion.identity);
            obj.transform.Rotate(Vector3.up, -180 + i * (360f / playerNumber));
            Machine machine = obj.GetComponent<Machine>();
            machine.SetCamera(machine.GetComponentInChildren<Camera>());
            machine.SetPlayerNumber(i + 1);
            machines.Add(machine);            
            machine.transform.Translate(Vector3.back * (playerNumber * 3 + 30));
            machine.GetPlayer().SetPlayerName(options.GetName(i + 1));
            machine.GetPlayer().SetHammerPos(machine.transform.position);
        }
        ArrayList cameras = new ArrayList(Camera.allCameras);
        if (cameras.Count != playerNumber)
            throw new MissingComponentException("Invalid amount of Cameras");
        if (playerNumber <= 3)
        {
            for (int i = 0; i < playerNumber; i++)
            {
                float x = 0 + 1f / playerNumber * i;
                float y = 0;
                float width = 1f / playerNumber;
                float height = 1f;
                ((Camera)(cameras[i])).rect = new Rect(x, y, width, height);
            }
        }
        else
        {
            int middle = playerNumber / 2;
            if (playerNumber % 2 != 0)
            {
                middle += 1;
                Camera absenteeCamera = gameObject.AddComponent<Camera>();
                absenteeCamera.transform.position = absenteeCameraTransform.position;
                absenteeCamera.transform.rotation = absenteeCameraTransform.rotation;
                cameras.Add(absenteeCamera);
            }
            float width = 1f / middle;
            float height = .5f;
            for (int i = 0; i < cameras.Count; i++)
            {
                float x = 0;
                if (i >= middle)
                    x = 0 + ((1f / middle) * (i - middle));
                else x = 0 + ((1f / middle) * i);
                float y = (i >= middle ? 0f : 0.5f);
                ((Camera)(cameras[i])).rect = new Rect(x, y, width, height);
            }
        }
        Dictionary<int, int> controllers = options.GetControllers();
        foreach (int i in controllers.Keys)
        {
            int controllertoSet = 0;
            if (controllers.TryGetValue(i, out controllertoSet))
                machines[i - 1].GetPlayer().SetController(controllertoSet);
        }
        Dictionary<int, string> wiimotes = options.GetWiimotes();
        HIDAPI api = WiimoteController.controller.api;
        foreach (int i in wiimotes.Keys)
        {
            string devPath = null;
            if (wiimotes.TryGetValue(i, out devPath))
                machines[i - 1].GetPlayer().SetWiiMote(new WiiMote(devPath, api));
        }
        Destroy(options.gameObject);
    }

    public int GetDifficulty()
    {
        return difficulty;
    }

    public List<Player> GetPlayers()
    {
        List<Player> returnval = new List<Player>();
        foreach (Machine m in machines)
        {
            returnval.Add(m.GetPlayer());
        }
        return returnval;
    }

    public List<Machine> GetMachines()
    {
        return new List<Machine>(machines);
    }

    public void SpawnAtOthers(Enemies enemy, Player initiator)
    {
        foreach (Machine m in machines)
        {
            if (m.GetPlayer() == initiator) continue;
            m.GetMoleController().Spawn(enemy);
        }
    }

    public void ResetMachines()
    {
        foreach (Machine m in machines)
        {
            m.Reset();
        }
        Hittable[] spawnedObjects = GameObject.FindObjectsOfType<Hittable>();
        foreach (Hittable h in spawnedObjects)
            Destroy(h.gameObject);
    }

    public bool IsSinglePlayer()
    {
        return this.playerNumber == 1; 
    }
}
