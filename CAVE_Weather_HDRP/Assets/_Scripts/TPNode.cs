using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TPNode : MonoBehaviour
{
    [Header("TPNode Settings")]
    public float regAnimSpeed;
    public float activeAnimSpeed;
    public float transitionSpeed;
    public float deactivationProximity; //how close the player has to be to deactivate this TPNode
    public float transitionScaleMult;
    public Color targetedColor, activatedColor, deactivatedColor;

    private BoxCollider bc;
    private float transitionStart, delta;
    private float animSpeed, currentAnimSpeed;
    private float initialScaleMult, currentScaleMult, actualScaleMult;
    private Color initialColor, currentColor;
    private Material mat;
    private float playerDist;

    private enum TPNodeStatus
    {
        Idle,
        ReturningToIdle,
        Targeting,
        Targeted,
        Activating,
        Activated,
        Deactivating,
        Deactivated       //When player arrives at and/or is near TPnode, it should not be annoying and targetable
    }

    //data
    private TPNodeStatus status;

    private void Awake()
    {
        status = TPNodeStatus.Idle;
        bc = GetComponent<BoxCollider>();
        initialScaleMult = currentScaleMult = actualScaleMult = 1;
        mat = GetComponent<Renderer>().material;
        initialColor = mat.color;
        animSpeed = regAnimSpeed;
    }

    public void Target()
    {
        status = TPNodeStatus.Targeting;
        TransitionPrep();
    }

    public void Untarget()
    {
        status = TPNodeStatus.ReturningToIdle;
        TransitionPrep();
    }

    public void Activate()
    {
        status = TPNodeStatus.Activating;
        TransitionPrep();
    }

    private void TransitionPrep()
    {
        transitionStart = Time.time;
        currentColor = mat.color;
        currentAnimSpeed = animSpeed;
        currentScaleMult = actualScaleMult;
    }

    private void Update()
    {
        //update playerDist
        playerDist = Vector3.Magnitude(Navigation.Instance.transform.position - transform.position);

        //Test for deactivation, ?only deactivate if not in process of moving to node?
        if ((status != TPNodeStatus.Deactivated && status != TPNodeStatus.Deactivating)
            && playerDist <= deactivationProximity)
        {
            //deactivate
            status = TPNodeStatus.Deactivating;
            TransitionPrep();
            //Turn off collision detection so rays don't see
            bc.enabled = false;
        }
        else if ((status == TPNodeStatus.Deactivating || status == TPNodeStatus.Deactivated)
            && playerDist > deactivationProximity)
        {
            //reactivate
            status = TPNodeStatus.ReturningToIdle;
            TransitionPrep();
            //Turn collision detection back on
            bc.enabled = true;
        }

        //COULD HAVE COLLIDER UPDATE SIZE BASED ON PLAYER DISTANCE SO TARGETING SCALES WITH DISTANCE PREVENTING IT FROM BEING DIFFICULT

        //Animate
        switch (status)
        {
            case TPNodeStatus.Idle:
                break;
            case TPNodeStatus.ReturningToIdle:
                delta = (Time.time - transitionStart) * transitionSpeed;
                if (delta >= 1)
                {
                    actualScaleMult = initialScaleMult;
                    animSpeed = regAnimSpeed;
                    mat.color = initialColor;
                    status = TPNodeStatus.Idle;
                }
                else
                {
                    actualScaleMult = Mathf.Lerp(currentScaleMult, initialScaleMult, delta);
                    mat.color = Color.Lerp(currentColor, initialColor, delta);
                    animSpeed = Mathf.Lerp(currentAnimSpeed, regAnimSpeed, delta);
                }
                break;
            case TPNodeStatus.Targeting:
                delta = (Time.time - transitionStart) * transitionSpeed;
                if (delta >= 1)
                {
                    actualScaleMult = transitionScaleMult;
                    mat.color = targetedColor;
                    status = TPNodeStatus.Targeted;
                }
                else
                {
                    actualScaleMult = Mathf.Lerp(currentScaleMult, transitionScaleMult, delta);
                    mat.color = Color.Lerp(currentColor, targetedColor, delta);
                }
                break;
            case TPNodeStatus.Targeted:
                break;
            case TPNodeStatus.Activating:
                delta = (Time.time - transitionStart) * transitionSpeed;
                if (delta >= 1)
                {
                    actualScaleMult = transitionScaleMult;
                    animSpeed = activeAnimSpeed;
                    mat.color = activatedColor;
                    status = TPNodeStatus.Activated;
                }
                else
                {
                    actualScaleMult = Mathf.Lerp(currentScaleMult, transitionScaleMult, delta);
                    mat.color = Color.Lerp(currentColor, activatedColor, delta);
                    animSpeed = Mathf.Lerp(currentAnimSpeed, activeAnimSpeed, delta);
                }
                break;
            case TPNodeStatus.Activated:
                break;
            case TPNodeStatus.Deactivating:
                delta = (Time.time - transitionStart) * transitionSpeed;
                if (delta >= 1)
                {
                    actualScaleMult = initialScaleMult;
                    animSpeed = 0;
                    mat.color = deactivatedColor;
                    status = TPNodeStatus.Deactivated;
                }
                else
                {
                    actualScaleMult = Mathf.Lerp(currentScaleMult, initialScaleMult, delta);
                    mat.color = Color.Lerp(currentColor, deactivatedColor, delta);
                    animSpeed = Mathf.Lerp(currentAnimSpeed, 0, delta);
                }
                break;
            case TPNodeStatus.Deactivated:
                break;
            default:
                break;
        }
    }

    //Using FixedUpdate for normalized "Animation" speeds
    private void FixedUpdate()
    {
        //Rotate
        transform.Rotate(new Vector3(Time.deltaTime * Random.Range(animSpeed / 2f, animSpeed),
            Time.deltaTime * Random.Range(animSpeed / 2f, animSpeed),
            Time.deltaTime * Random.Range(animSpeed / 2f, animSpeed)));

        //Update scale based on player distance so is easily visible from all distances
        Vector3 scale = Vector3.one;
        scale *= actualScaleMult * (1 + Mathf.Sqrt((Mathf.Max(playerDist - deactivationProximity, 0)) * 0.1f));
        transform.localScale = scale;
    }
}
