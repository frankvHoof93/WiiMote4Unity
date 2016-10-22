using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDController : MonoBehaviour {
    public Text PointsText;
    public Text BombsText;
    public Text SuperText;

    public Machine player;

    public void SetPlayer(Machine player)
    {
        this.player = player;
        this.transform.SetParent(player.transform);
        RectTransform rect = GetComponent<RectTransform>();
        rect.localPosition = new Vector3(-0.06f, 1.89f, 6.4f) + Vector3.back * 0.4f;
        rect.localScale = new Vector3(0.016f, 0.016f, 0.016f);
        rect.LookAt(player.GetCamera().transform.position);
        rect.Rotate(Vector3.up, 180f);
    }

    public void SetUI()
    {
        Camera camera = player.GetCamera();
        this.GetComponent<Canvas>().worldCamera = camera;
    }
    
    public void UpdateUI()
    {
        PointsText.text = player.GetPlayer().GetPoints().ToString();
        BombsText.text = player.GetPlayer().GetBombs().ToString();
        if (SuperText != null)
            SuperText.text = player.GetPlayer().GetSuper().ToString();
    }
}
