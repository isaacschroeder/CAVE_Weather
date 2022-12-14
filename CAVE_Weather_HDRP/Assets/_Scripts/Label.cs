using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Label : MonoBehaviour
{
    [Header("Label Settings")]
    public GameObject mvrPlayer; //player to look at
    public float sizeRef; //reference for size of label at a distance of 1?

    //Data
    private Transform headNodeTransform;
    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale; //Right kind of scale?
        headNodeTransform = null;
    }

    private void Start()
    {
        //MVR generates this node at runtime, supposedly in its awake, so look for this in start to avoid race conditions.
        headNodeTransform = mvrPlayer.transform.Find("SystemCenter").Find("CenterNode").Find("HeadNode");
    }

    void Update()
    {
        //Look at player
        transform.LookAt(headNodeTransform, headNodeTransform.up);
        transform.Rotate(0, 180, 0);

        //Adjust scale to appear as constant size
        Vector3 scale = (sizeRef*Vector3.one) * Vector3.Distance(headNodeTransform.position, transform.position);
        scale.z = 1;
        transform.localScale = scale;
    }
}
