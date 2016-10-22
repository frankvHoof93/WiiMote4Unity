using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public GameObject hammerPrefab;
    Hammer myHammer;
    public float points;
    int bombs;
    float super;
    string playerName;
    public int playerNumber;

    void Awake()
    {
        if (myHammer != null)
            Destroy(myHammer.gameObject);
        GameObject obj = (GameObject)Instantiate(hammerPrefab, this.transform.position, this.transform.rotation);
        myHammer = obj.GetComponent<Hammer>();
        myHammer.SetPlayer(this);
    }

	void Start () {
        Reset();
	}

    public void Reset()
    {
        bombs = 0;
        super = 0;
        points = 0f;
        myHammer.transform.position = this.transform.position + Vector3.up * 10f - Vector3.forward * 3f;
        myHammer.transform.rotation = this.transform.rotation;
        myHammer.SetPlayer(this);
    }

    public void AddPoints(float points)
    {
        this.points = Mathf.Clamp(this.points + points, 0, 100000);
    }

    public float GetPoints()
    {
        return this.points;
    }

    public int GetBombs()
    {
        return this.bombs;
    }

    public int GetSuper()
    {
        return (int)(this.super/3);
    }

    public void AddBomb()
    {
        this.bombs++;
    }

    public void AddSuper()
    {
        this.super++;
    }

    public void SetHammerPos(Vector3 position)
    {
        myHammer.SetPosition(position);
    }

    public void SetWiiMote(WiiMote wiiMote) 
    {
        this.myHammer.SetWiimote(wiiMote);
    }

    public void DamageHammer()
    {
        this.myHammer.Damage();
    }

    public void SetPlayerName(string name)
    {
        this.playerName = name;
    }

    public string GetPlayername()
    {
        if (this.playerName == null)
            return "Player " + this.playerNumber;
        return this.playerName;
    }

    public void SetController(int number)
    {
        this.myHammer.SetController(number);
    }

    public void SetPlayerNumber(int number)
    {
        this.playerNumber = number;
    }

    public int GetPlayerNumber()
    {
        return this.playerNumber;
    }
}
