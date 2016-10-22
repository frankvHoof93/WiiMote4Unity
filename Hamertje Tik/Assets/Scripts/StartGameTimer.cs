using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class StartGameTimer : MonoBehaviour {
    public GameObject startGameCanvasPrefab;
    bool started = false;
    List<Text> timerTexts;
    public float timer = 3.9f;
    
    public void StartGame()
    {
        timerTexts = new List<Text>();
        started = true;
        timer = 3.9f;
        foreach (Machine m in MachineController.controller.GetMachines())
        {
            Canvas obj = Instantiate(startGameCanvasPrefab).GetComponent<Canvas>();
            obj.worldCamera = m.GetCamera();
            timerTexts.Add(obj.GetComponentInChildren<Text>());
            Destroy(obj.gameObject, 4.1f);
        }
    }
	
	// Update is called once per frame
	void FixedUpdate () {
	    if (started && GameLogicController.controller.currentGameState == GameState.Starting)
        {
            timer -= Time.fixedDeltaTime;
            string text = ((int)timer).ToString();            
            if (timer <= 1f)
            {
                text = "GO!";
                GameLogicController.controller.StartGame();
            }
            foreach (Text t in timerTexts)
                t.text = text;
        }
	}
}
