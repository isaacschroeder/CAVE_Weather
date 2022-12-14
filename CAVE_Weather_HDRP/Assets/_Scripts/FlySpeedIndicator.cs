using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlySpeedIndicator : MonoBehaviour
{
    [Header("FlySpeedIndicator Settings")]
    public Text speedIndicatorTXT;
    public float stayOpenAfterChangeDuration = 2;
    public float transitionSpeed = 5; //for showing and hiding

    private enum UIStatus
    {
        deploying, deployed, concealing, concealed
    }

    //Data
    private UIStatus status; //for concealing and deploying
    private Vector3 initialScale;
    private float transitionStart;
    private float changedTime;

    private void Awake()
    {
        status = UIStatus.deployed; //Start deployed
        initialScale = transform.localScale; //Right kind of scale?
        changedTime = 0;
    }

    private void Start()
    {
        Hide();
    }

    public void SetSpeed(float flySpeed)
    {
        Show();
        speedIndicatorTXT.text = "Speed: " + flySpeed;
        changedTime = Time.time;
    }

    private void Update()
    {
        if (status == UIStatus.deploying)
        {
            float delta = (Time.time - transitionStart) * transitionSpeed;
            if (delta >= 1)
            {
                transform.localScale = initialScale;
                status = UIStatus.deployed;
            }
            else
            {
                transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, delta);
            }
        }
        else if (status == UIStatus.concealing)
        {
            float delta = (Time.time - transitionStart) * transitionSpeed;
            if (delta >= 1)
            {
                transform.localScale = Vector3.zero;
                status = UIStatus.concealed;
            }
            else
            {
                transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, delta);
            }
        }
        else if (status == UIStatus.deployed)
        {
            if (Time.time > changedTime + stayOpenAfterChangeDuration)
            {
                Hide();
            }
        }
    }

    private void Show()
    {
        if (status == UIStatus.concealed)
        {
            status = UIStatus.deploying;
            transitionStart = Time.time;
        }
    }

    private void Hide()
    {
        if (status == UIStatus.deployed)
        {
            status = UIStatus.concealing;
            transitionStart = Time.time;
        }
    }
}
