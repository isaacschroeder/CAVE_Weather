using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowWindMotion : MonoBehaviour
{
    public Vector3[] positions;
    public float speed;

    private ArrowHead arrowhead;
    private int targetPosIndex;

    private void Awake()
    {
        arrowhead = GetComponent<ArrowHead>();
        targetPosIndex = 1;
    }

    private void Start()
    {
        arrowhead.MoveTo(positions[0]);
        arrowhead.Collapse();
    }

    private void Update()
    {
        if (arrowhead.GetPos().Equals(positions[targetPosIndex]))
        {
            if (targetPosIndex == positions.Length - 1)
            {
                arrowhead.MoveTo(positions[0]);
                arrowhead.Collapse();
                targetPosIndex = 1;
            }
            else
            {
                targetPosIndex++;
            }
        }
        Vector3 pos = Vector3.MoveTowards(arrowhead.GetPos(), positions[targetPosIndex], speed * Time.deltaTime);

        arrowhead.MoveTo(pos);
    }
}
