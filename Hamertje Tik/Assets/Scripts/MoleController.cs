using UnityEngine;
using System.Collections;

public enum Enemies
{
    Digglet,
    Dugtrio,
    Pinsir,
    Voltorb,
    Default
};

public class MoleController : MonoBehaviour {

    public GameObject molePrefab;
    public GameObject bombPrefab;
    public GameObject superPrefab;
    public GameObject specialPrefab;
    public Transform[] molePositions = new Transform[9];
    public AudioClip spawnAudio;
    private bool[] molesSpawned = new bool[9];

    public float[] minSpeeds = new float[4];
    public float[] maxSpeeds = new float[4];

    public float timingMin;
    public float timingMax;

    private float nextSpawn;

    private Machine myMachine;

	// Use this for initialization
	void Start () {
        timingMin = minSpeeds[MachineController.controller.GetDifficulty() - 1] *1000;
        timingMax = maxSpeeds[MachineController.controller.GetDifficulty() - 1] *1000;
	}
	
	// Update is called once per frame
	void Update () {
        if (GameLogicController.controller.currentGameState == GameState.Running)
        {
            nextSpawn -= (Time.deltaTime * 1000); // Millis
            if (nextSpawn <= 0)
            {
                nextSpawn = Random.Range(timingMin, timingMax);
                Spawn();
                GameUIController.controller.PlayFX(myMachine.GetPlayer().GetPlayerNumber(), spawnAudio);
            }
        }
	}

    public void SetMachine(Machine machine)
    {
        myMachine = machine;
    }

    public Machine GetMachine() { return myMachine; }

    public Player GetPlayer() { return myMachine.GetPlayer(); }

    public void Spawn(Enemies enemy = Enemies.Default)
    {
        ArrayList validTransforms = new ArrayList();
        for (int i = 0; i < 9; i++)
        {
            if (!molesSpawned[i])
                validTransforms.Add(this.molePositions[i]);
        }
        if (validTransforms.Count == 0){
            Debug.Log("No positions available");
            return;
        }
        int spawnLoc = Random.Range(0, validTransforms.Count - 1);
        Transform spawnLocation = (Transform)validTransforms[spawnLoc];
        int index = 0;
        for (int i = 0; i < 9; i++ )
        {
            if (molePositions[i] == spawnLocation)
            {
                molesSpawned[i] = true;
                index = i;
            }
        }
        GameObject obj = null;
        if (enemy != Enemies.Default)
        {
            switch (enemy)
            {
                case Enemies.Digglet:
                    obj = (GameObject)Instantiate(molePrefab, spawnLocation.position, Quaternion.identity);
                    break;
                    case Enemies.Dugtrio:
                    obj = (GameObject)Instantiate(superPrefab, spawnLocation.position, Quaternion.identity);
                    break;
                    case Enemies.Pinsir:
                    obj = (GameObject)Instantiate(bombPrefab, spawnLocation.position, Quaternion.identity);
                    break;
                    case Enemies.Voltorb:
                    obj = (GameObject)Instantiate(specialPrefab, spawnLocation.position, Quaternion.identity);
                    break;
            }
        }            
        else{
            int spawn = Random.Range(0, 10);
            switch (spawn)
            {
                case 0:
                case 1:
                case 2:
                    obj = (GameObject)Instantiate(bombPrefab, spawnLocation.position, Quaternion.identity);
                    break;
                case 3:
                    GameObject prefab = (MachineController.controller.IsSinglePlayer() ? molePrefab : specialPrefab);
                    obj = (GameObject)Instantiate(prefab, spawnLocation.position, Quaternion.identity);
                    break;
                default:
                    obj = (GameObject)Instantiate(molePrefab, spawnLocation.position, Quaternion.identity);
                    break;
            }
        }
        try
        {
            Hittable h = obj.GetComponent<Hittable>();
            h.SetIndex(index);
            h.SetMachine(this);            
        }
        catch (UnassignedReferenceException)
        {
            throw new MissingComponentException("Please ensure your moles are 'Hittable'");
        }
    }

    public void ClearPosition(int index)
    {
        molesSpawned[index] = false;
    }

    internal void Reset()
    {
        for (int i = 0; i < 9; i++)
            ClearPosition(i);
    }
}
