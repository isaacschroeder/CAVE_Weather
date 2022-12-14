using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElevationIndicator : MonoBehaviour
{
    [Header("ElevationIndicator Settings")]
    public float unitUnitsSeaLevel = 0;
    public float unityUnits18000ft = 1500;
    public Text measurementTXT;
    public GameObject mvrPlayer;
    public float transitionSpeed = 5; //for showing and hiding

    private enum UIStatus
    {
        deploying, deployed, concealing, concealed
    }

    //Data
    private UIStatus status; //for concealing and deploying
    private Transform headNodeTransform;
    private Vector3 initialScale;
    private float transitionStart;

    private void Awake()
    {
        status = UIStatus.deployed; //Start deployed
        initialScale = transform.localScale; //Right kind of scale?
        headNodeTransform = null;
    }

    void Start()
    {
        //MVR generates this node at runtime, supposedly in its awake, so look for this in start to avoid race conditions.
        headNodeTransform = mvrPlayer.transform.Find("SystemCenter").Find("CenterNode").Find("HeadNode"); 
        if (headNodeTransform)
            print("Found HeadNode");
    }

    void Update()
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
            float yVal = headNodeTransform.transform.position.y;
            float mapped = (yVal * 18000) / (unityUnits18000ft - unitUnitsSeaLevel);
            measurementTXT.text = "" + (int)(mapped) + " ft";
        }
    }

    public void Show()
    {
        if (status == UIStatus.concealed)
        {
            status = UIStatus.deploying;
            transitionStart = Time.time;
        }
    }

    public void Hide()
    {
        if (status == UIStatus.deployed)
        {
            status = UIStatus.concealing;
            transitionStart = Time.time;
        }
    }
}
