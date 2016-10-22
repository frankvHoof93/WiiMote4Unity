using UnityEngine;
using System.Collections;

public class Pinsir : Hittable {

    public int hitDeduction = -5;

    protected override void Awake()
    {
        
    }
    
    public override float Hit()
    {
        if (GameLogicController.controller.currentGameState != GameState.Running)
            return 0;
        myController.GetPlayer().DamageHammer();
        KillMe();
        if (hitDeduction != 0)
            ShowPopup(hitDeduction);
        return hitDeduction;
    }

    public override float Miss()
    {
        if (GameLogicController.controller.currentGameState != GameState.Running)
            return 0;
        GameUIController.controller.PlayFX(myController.GetPlayer().playerNumber, soundMiss);
        float points = KillMe();
        if (points != 0)
            ShowPopup((int)points);
        return points;
    }
}
