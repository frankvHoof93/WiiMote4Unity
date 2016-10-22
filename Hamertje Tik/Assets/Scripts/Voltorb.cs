using UnityEngine;
using System.Collections;

public class Voltorb : Hittable {

    protected override void Awake()
    {
        this.points = 100;
    }

    public override float Hit()
    {
        if (GameLogicController.controller.currentGameState != GameState.Running)
            return 0;
        float points = KillMe();
        if (points != 0)
            ShowPopup((int)points);
        return points;
    }

    public override float Miss()
    {
        if (GameLogicController.controller.currentGameState != GameState.Running)
            return 0;
        GameUIController.controller.PlayFX(myController.GetPlayer().playerNumber, soundMiss);
        KillMe();
        return 0;
    }
}
