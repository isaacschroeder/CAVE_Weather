using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Navigation : MonoBehaviour
{
    private enum NavMode
    {
        WorldView,      //Player can pivot around view of environment from a distance and is focused on environment center, good for big picutre experience
        FreeFlight,      //Player can fly freely within the environment, better for seeing and exploring details and interesting perspectives
        Teleporting     //Player is currently in the process of teleporitng
    }

    [Header("Navigation Settings")]
    //The location where the player should spawn upon start
    public Transform spawn, worldViewAnchor, worldViewFocusPoint;
    public bool debugInput;
    public float joystickDeadzone;
    public bool instantTeleport;
    public int horizMapBoundryCorner1, horizMapBoundryCorner2, vertMapBoundryLower, vertMapBoundaryUpper;
    public int initialFlySpeedLevel, totalFlySpeedlevels;
    public float minFlySpeed, speedChangeInterval;
    public float initialRotateSpeed, tpYHeightDistRatio, tpTimeDistRatio, tpMinTime;
    public MVRInputEventType mvrTeleportInputEvent, mvrIncreaseFlightSpeedInputEvent, mvrDecreaseFlightSpeedInputEvent, mvrToggleNavModeInputEvent;
    public float worldViewPivotSpeed;
    public GameObject FlySpeedIndicator;

    //data
    NavMode navMode; //now covers whether currently teleporting or not
    private bool freeFlightFastMode; //Two speeds in free flight, slow speed and fast speed, toggleable with a wand button
    private bool instantTP;
    private float flySpeed, rotateSpeed;
    private int flySpeedLevel;
    private GameObject targetedTPNode; //what ray is looking at
    private GameObject menuRequestTPNode;
    private bool tpButtonPress, toggleNavModeInputEventButtonPress, increaseFlightSpeedButtonPress, decreaseFlightSpeedButtonPress;
    private float tpStartTime, tpDist;
    private Vector3 tpStartPos, tpTargetPos;
    private Quaternion tpStartRot, tpTargetRot;
    private float worldViewVertical, worldViewHorizontal, worldViewZoom;
    private bool tpToWorldView, tpToPointRequest; //to know if a requested tp is to world view point
    private Vector3 tpToPointRequestPoint, posBeforeWorldView; //last position before entering worldview
    private Quaternion tpToPointRequestRotation, rotationBeforeWorldView;

    private static Navigation _instance;

    public static Navigation Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            print("unwanted attempt to create additional navigation module!");
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        transform.position = spawn.position;
        transform.rotation = spawn.rotation; //Spawnpoint effects player rotation aswell
        flySpeedLevel = initialFlySpeedLevel;
        flySpeed = minFlySpeed + speedChangeInterval * flySpeedLevel;
        rotateSpeed = initialRotateSpeed;
        instantTP = instantTeleport;
        targetedTPNode = null;
        menuRequestTPNode = null;
        tpButtonPress = increaseFlightSpeedButtonPress = decreaseFlightSpeedButtonPress = toggleNavModeInputEventButtonPress = false;
        tpToWorldView = tpToPointRequest = false;
        tpStartPos = Vector3.zero;
        tpTargetPos = Vector3.zero;
        tpStartRot = Quaternion.identity;
        tpTargetRot = Quaternion.identity;
        posBeforeWorldView = tpToPointRequestPoint = Vector3.zero;
        rotationBeforeWorldView = tpToPointRequestRotation = Quaternion.identity;
        tpStartTime = 0;
        navMode = NavMode.FreeFlight; //Start player freeflying
        freeFlightFastMode = false; //Start on slow mode
    }

    private void Start()
    {
        //Subscribe to control events
        MVRInputHandler.SubscribeTo(mvrTeleportInputEvent, TPButtonPress);
        MVRInputHandler.SubscribeTo(mvrIncreaseFlightSpeedInputEvent, IncreaseFlightSpeedButtonPress);
        MVRInputHandler.SubscribeTo(mvrDecreaseFlightSpeedInputEvent, DecreaseFlightSpeedButtonPress);
        MVRInputHandler.SubscribeTo(mvrToggleNavModeInputEvent, ToggleNavModeInputEventButtonPress);
    }

    private void Update()
    {
        if (navMode == NavMode.Teleporting) //Teleport proceedures
        {
            HandleTeleport();
        }
        else
        {
            HandleInput();

            CheckHandRayCastForHits();

            CheckForTeleportRequest();
        }
    }

    private void HandleTeleport()
    {
        if (instantTP)
        {
            transform.position = tpTargetPos;
            transform.rotation = tpTargetRot;
            if (tpToWorldView)
            {
                tpToWorldView = false;
                navMode = NavMode.WorldView;
            }
            else
            {
                navMode = NavMode.FreeFlight;
            }
        }
        else
        {
            float delta = (Time.time - tpStartTime) / (tpMinTime + tpTimeDistRatio * tpDist);
            if (delta >= 1)
            {
                transform.position = tpTargetPos;
                transform.rotation = tpTargetRot;
                if (tpToWorldView)
                {
                    tpToWorldView = false;
                    navMode = NavMode.WorldView;
                }
                else
                {
                    navMode = NavMode.FreeFlight;
                }
            }
            else
            {
                Vector3 pos = Vector3.Lerp(tpStartPos, tpTargetPos, delta);
                Quaternion rot = Quaternion.Lerp(tpStartRot, tpTargetRot, delta);
                pos.y = pos.y - Mathf.Pow((Mathf.Abs(delta - 0.5f) * 2), 2) * tpYHeightDistRatio * tpDist + tpYHeightDistRatio * tpDist; //parabola shape to not clip through ground for tp lerp
                transform.position = pos;
                transform.rotation = rot;
            }
        }
    }

    private void HandleInput()
    {
        float vert = MVRInputHandler.GetJoystickVerticalAxis();
        float horiz = MVRInputHandler.GetJoystickHorizontalAxis();
        if (toggleNavModeInputEventButtonPress)
        {
            toggleNavModeInputEventButtonPress = false;
            if (navMode == NavMode.FreeFlight)
            {
                //Make switch to worldview, teleporting to the worldview anchor
                posBeforeWorldView = transform.position; //record current position
                rotationBeforeWorldView = transform.rotation; //and rotation!
                tpToWorldView = true;
                tpToPointRequest = true;
                tpToPointRequestPoint = worldViewAnchor.position;
                tpToPointRequestRotation = worldViewAnchor.rotation;
            }
            else if (navMode == NavMode.WorldView)
            {
                //Make switch to freeflight, teleporting to the last location before switching to worldview
                tpToPointRequest = true;
                tpToPointRequestPoint = posBeforeWorldView;
                tpToPointRequestRotation = rotationBeforeWorldView;
            }
        }
        if (navMode == NavMode.FreeFlight)
        {
            if (increaseFlightSpeedButtonPress)
            {
                increaseFlightSpeedButtonPress = false;
                if (flySpeedLevel < totalFlySpeedlevels)
                {
                    flySpeedLevel++;
                }
                flySpeed = minFlySpeed + speedChangeInterval * flySpeedLevel;
                FlySpeedIndicator.GetComponent<FlySpeedIndicator>().SetSpeed(flySpeed);
            }
            if (decreaseFlightSpeedButtonPress)
            {
                decreaseFlightSpeedButtonPress = false;
                if (flySpeedLevel > 0)
                {
                    flySpeedLevel--;
                }
                flySpeed = minFlySpeed + speedChangeInterval * flySpeedLevel;
                FlySpeedIndicator.GetComponent<FlySpeedIndicator>().SetSpeed(flySpeed);
            }
            if (horiz < joystickDeadzone || Input.GetKey(KeyCode.A))
            {
                if (Input.GetKey(KeyCode.A))
                    transform.Rotate(new Vector3(0, Time.deltaTime * rotateSpeed * (-1), 0));
                else
                    transform.Rotate(new Vector3(0, Time.deltaTime * rotateSpeed * horiz, 0));
            }
            if (horiz > joystickDeadzone || Input.GetKey(KeyCode.D))
            {
                if (Input.GetKey(KeyCode.D))
                    transform.Rotate(new Vector3(0, Time.deltaTime * rotateSpeed * 1, 0));
                else
                    transform.Rotate(new Vector3(0, Time.deltaTime * rotateSpeed * horiz, 0));
            }
            if (vert > joystickDeadzone || Input.GetKey(KeyCode.W))
            {
                if (Input.GetKey(KeyCode.W))
                {
                    //move towards ray direction
                    Vector3 pos = transform.position;
                    pos += MVRInputHandler.Instance.handRay.direction * flySpeed * 1 * Time.deltaTime;
                    transform.position = pos;
                    //Note no clamping in debug
                }
                else
                {
                    //move towards ray direction
                    Vector3 pos = transform.position;
                    pos += MVRInputHandler.Instance.handRay.direction * flySpeed * vert * Time.deltaTime;
                    //Clamp to world bounds before setting
                    pos.x = Mathf.Clamp(pos.x, horizMapBoundryCorner1, horizMapBoundryCorner2);
                    pos.z = Mathf.Clamp(pos.z, horizMapBoundryCorner1, horizMapBoundryCorner2);
                    pos.y = Mathf.Clamp(pos.y, vertMapBoundryLower, vertMapBoundaryUpper);
                    transform.position = pos;
                }
            }
            if (vert < joystickDeadzone || Input.GetKey(KeyCode.S))
            {
                if (Input.GetKey(KeyCode.S))
                {
                    //move opposite ray direction
                    Vector3 pos = transform.position;
                    pos += MVRInputHandler.Instance.handRay.direction * flySpeed * (-1) * Time.deltaTime;
                    transform.position = pos;
                    //Note no clamping in debug
                }
                else
                {
                    //move opposite ray direction
                    Vector3 pos = transform.position;
                    pos += MVRInputHandler.Instance.handRay.direction * flySpeed * vert * Time.deltaTime;
                    //Clamp to world bounds before setting
                    pos.x = Mathf.Clamp(pos.x, horizMapBoundryCorner1, horizMapBoundryCorner2);
                    pos.z = Mathf.Clamp(pos.z, horizMapBoundryCorner1, horizMapBoundryCorner2);
                    pos.y = Mathf.Clamp(pos.y, vertMapBoundryLower, vertMapBoundaryUpper);
                    transform.position = pos;
                }
            }
        }
        else if (navMode == NavMode.WorldView)
        {
            //to be implemented, just stays still at the anchor for now which is perfectly fine for now.
        }
    }

    private void CheckHandRayCastForHits()
    {
        //Look to see if hand ray is pointing towards any TP nodes
        GameObject seen = MVRInputHandler.Instance.seen;
        if (seen != null && seen.CompareTag("TPNode"))
        {
            //If first time looking at new node
            if (targetedTPNode != seen)
            {
                if (targetedTPNode != null)
                {
                    targetedTPNode.GetComponent<TPNode>().Untarget(); //untarget old node?
                }
                targetedTPNode = seen;
                targetedTPNode.GetComponent<TPNode>().Target(); //Start Targeting TP Node
            }
        }
        else
        {
            if (targetedTPNode != null)
            {
                targetedTPNode.GetComponent<TPNode>().Untarget(); //Stop Targeting TP Node
                targetedTPNode = null;
            }
        }
    }

    private void CheckForTeleportRequest()
    {
        //Check for a TP Button Press
        if (tpButtonPress)
        {
            tpButtonPress = false;
            if (targetedTPNode != null)
            {
                targetedTPNode.GetComponent<TPNode>().Activate(); //Activate TP Node
                navMode = NavMode.Teleporting;
                tpStartTime = Time.time;
                tpStartPos = transform.position;
                tpTargetPos = targetedTPNode.transform.position;
                tpStartRot = transform.rotation;
                tpTargetRot = Quaternion.Euler(0, (Quaternion.LookRotation(worldViewFocusPoint.position - targetedTPNode.transform.position)).eulerAngles.y, 0); //rotate to facing worldViewFocusPoint from tp node
                tpDist = Vector3.Distance(tpTargetPos, tpStartPos);
            }
        }
        //else check for menu teleport request
        else if (menuRequestTPNode != null)
        {
            menuRequestTPNode.GetComponent<TPNode>().Activate(); //Activate TP Node
            navMode = NavMode.Teleporting;
            tpStartTime = Time.time;
            tpStartPos = transform.position;
            tpTargetPos = menuRequestTPNode.transform.position;
            tpStartRot = transform.rotation;
            tpTargetRot = Quaternion.Euler(0,(Quaternion.LookRotation(worldViewFocusPoint.position - menuRequestTPNode.transform.position)).eulerAngles.y, 0); //rotate to facing worldViewFocusPoint from tp node
            //Quaternion.identity; //rotate to normal for tpnodes
            tpDist = Vector3.Distance(tpTargetPos, tpStartPos);
            menuRequestTPNode = null; //remove menu request node so doesn't trigger again
        }
        //else an alternative teleport request
        else if (tpToPointRequest)
        {
            tpToPointRequest = false;
            navMode = NavMode.Teleporting;
            tpStartTime = Time.time;
            tpStartPos = transform.position;
            tpTargetPos = tpToPointRequestPoint;
            tpStartRot = transform.rotation;
            tpTargetRot = tpToPointRequestRotation;
            tpDist = Vector3.Distance(tpTargetPos, tpStartPos);
            menuRequestTPNode = null; //remove menu request node so doesn't trigger again
        }
    }

    public void TPButtonPress()
    {
        tpButtonPress = true;
    }

    public void IncreaseFlightSpeedButtonPress()
    {
        increaseFlightSpeedButtonPress = true;
    }

    public void DecreaseFlightSpeedButtonPress()
    {
        decreaseFlightSpeedButtonPress = true;
    }

    public void ToggleNavModeInputEventButtonPress()
    {
        toggleNavModeInputEventButtonPress = true;
    }

    public void MenuTPRequest(GameObject tpNode)
    {
        menuRequestTPNode = tpNode;
    }

    public bool MenuToggleInstantTP()
    {
        instantTP = !instantTP;
        return instantTP;
    }
}

