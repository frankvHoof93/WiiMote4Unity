using UnityEngine;
using System.Collections;

public class Dugtrio : Hittable
{
    public int lives = 3;
    public int missDeduction = -50;

    protected override void Awake()
    {
        this.points = 10f;
    }

    public override float Hit()
    {
        if (GameLogicController.controller.currentGameState != GameState.Running)
            return 0;
        lives--;
        if (lives <= 0)
        {
            float points = KillMe();
            if (points != 0)
                ShowPopup((int)points);
            return points;
        }
        else return 0;
    }

    public override float Miss()
    {
        if (GameLogicController.controller.currentGameState != GameState.Running)
            return 0;
        KillMe();
        GameUIController.controller.PlayFX(myController.GetPlayer().playerNumber, soundMiss);
        if (missDeduction != 0)
            ShowPopup(missDeduction);
        return missDeduction;
    }
}
