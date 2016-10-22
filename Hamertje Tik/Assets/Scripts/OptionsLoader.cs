using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OptionsLoader : MonoBehaviour {
    private int players = 1;
    private int difficulty = 1;
    private Dictionary<int, int> setControllers = new Dictionary<int, int>();
    private Dictionary<int, string> setMotes = new Dictionary<int, string>();
    private Dictionary<int, string> setNames = new Dictionary<int, string>();

    public int GetPlayers()
    { return players; }

    public int GetDifficulty()
    { return difficulty; }

    public void SetPlayers(int players)
    {
        this.players = players;
    }

    public void SetDifficulty(int difficulty)
    {
        this.difficulty = difficulty;
    }

    public Dictionary<int, string> GetWiimotes()
    {
        return setMotes;
    }

    public void SetWiiMotes(Dictionary<int, string> wiimotes)
    {
        this.setMotes = wiimotes;
    }

    public Dictionary<int, int> GetControllers()
    {
        return setControllers;
    }

    public void SetControllers(Dictionary<int, int> controllers)
    {
        this.setControllers = controllers;
    }

    public void AddName(int playerno, string name)
    {
        setNames.Add(playerno, name);
    }

    public string GetName(int playerno)
    {
        string value;
        if (!setNames.TryGetValue(playerno, out value)) return null;
        return value;
    }
}
