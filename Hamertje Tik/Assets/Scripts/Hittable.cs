using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public abstract class Hittable : MonoBehaviour
{
    protected MoleController myController;
    public GameObject popupCanvasPrefab;
    public AudioClip soundHit;
    public AudioClip soundMiss;
    public AudioClip soundMoveDown;
    public int rotateMin;
    public int rotateMax;
    public float points = 0f;
    public float moveUpMax;
    public float timeToLive;
    public float defaultTimeToLive;
    protected int rotateAmount;
    protected int index;
    public bool isUp;
    public Vector3 startPosition;

    void Start()
    {
        rotateAmount = Random.Range(rotateMin, rotateMax);
        if (Random.Range(0, 2) == 1)
            rotateAmount *= -1;
        timeToLive = defaultTimeToLive;
        startPosition = transform.position;
    }

    void FixedUpdate()
    {
        transform.Rotate(transform.up, rotateAmount);
        if (GameLogicController.controller.currentGameState == GameState.Running)
        {
            if (isUp)
            {
                timeToLive -= Time.fixedDeltaTime;
                if (timeToLive <= 0)
                {
                    Move(false);
                    if (Vector3.Distance(transform.position, startPosition) <= 0.01)
                    {
                        myController.GetPlayer().AddPoints(Miss());
                        GameUIController.controller.UpdateUI();
                        GameUIController.controller.PlayFX(myController.GetPlayer().playerNumber, soundMoveDown);
                    }
                }
            }
            else
            {
                Move(true);
                if (Vector3.Distance(transform.position, startPosition) >= moveUpMax)
                {
                    
                    isUp = true;
                }
            }
        }
    }

    /// <summary>
    /// USE FOR INITIALISATION
    /// </summary>
    protected abstract void Awake();

    public void SetIndex(int index)
    {
        this.index = index;
    }

    public void SetMachine(MoleController controller)
    { myController = controller; }

    protected float KillMe()
    {
        myController.ClearPosition(this.index);
        Destroy(this.gameObject, 0.01f);
        return points;
    }

    protected void Move(bool moveUp)
    {
        float movement = moveUp ? (float)0.1 : (float)-0.1;
        transform.Translate(Vector3.up * movement);
    }

    public abstract float Hit();

    public abstract float Miss();

    protected void ShowPopup(int points)
    {
        GameObject popupCanvas = (GameObject)Instantiate(popupCanvasPrefab, this.gameObject.transform.position, Quaternion.identity);
        Text t = popupCanvas.GetComponentInChildren<Text>();
        t.text = points.ToString();
        if (points <= 0)
            t.color = Color.red;
        else t.color = Color.green;
        popupCanvas.transform.LookAt(myController.GetMachine().GetCamera().transform);
        popupCanvas.transform.Rotate(Vector3.up, 180f);
        Destroy(popupCanvas, 1.6f);
    }
    
}