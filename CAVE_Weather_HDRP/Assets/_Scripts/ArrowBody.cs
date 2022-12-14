using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowBody : MonoBehaviour
{
    public float maxDistBtw;
    private float dist;
    private GameObject attachedTo;

    public void setAttached(GameObject target)
    {
        attachedTo = target;
    }

    // Update is called once per frame
    void Update()
    {
        //dist btw attached and self
        dist = Vector3.Distance(transform.position, attachedTo.transform.position);

        if (dist > maxDistBtw)
        {
            //back towards self from attached position
            Vector3 dir = Vector3.Normalize(transform.position - attachedTo.transform.position);
            transform.LookAt(attachedTo.transform, Vector3.up);

            Vector3 pos = attachedTo.transform.position + dir * maxDistBtw;
            transform.position = pos;
        }

    }
}
