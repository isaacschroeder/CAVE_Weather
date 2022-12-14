using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowHead : MonoBehaviour
{
    public int segmentCount;
    public GameObject segmentPrefab;
    public Material material;

    private GameObject head;
    private List<GameObject> segments;

    private void Awake()
    {
        head = transform.Find("Head").gameObject;
        segments = new List<GameObject>();
    }

    private void Start()
    {
        GameObject attachPoint = transform.Find("Point").gameObject;
        attachPoint.GetComponent<ArrowBody>().setAttached(head);
        attachPoint.transform.GetChild(0).gameObject.GetComponent<Renderer>().material = material;
        segments.Add(attachPoint);

        for (int i = 0; i < segmentCount; i++)
        {
            //Instantiate segments and attach them together!
            GameObject newSegment = GameObject.Instantiate(segmentPrefab);
            newSegment.transform.position = transform.position;
            newSegment.transform.parent = transform;
            newSegment.GetComponent<ArrowBody>().setAttached(attachPoint);
            newSegment.transform.GetChild(0).gameObject.GetComponent<Renderer>().material = material; //Set mat
            attachPoint = newSegment;
            segments.Add(newSegment);
        }
    }

    public void MoveTo(Vector3 pos)
    {
        head.transform.position = pos;
    }

    public Vector3 GetPos()
    {
        return head.transform.position;
    }

    //Collapses all segments
    public void Collapse()
    {
        for(int i = 0; i < segments.Count; i++)
        {
            segments[i].transform.position = head.transform.position;
        }
    }
}