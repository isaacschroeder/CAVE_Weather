using System.Collections;
using System.Collections.Generic;
using System; //For Enum.GetNames()
using UnityEngine;
using UnityEngine.Events;
using MiddleVR;

//This should handle all MVR input as to confine it to one script, so all scripts don't have to be dependent on MVR.

//'Press' invoke on press
//'Down' invoke while pressed - IS THIS UNECESSARY AND/OR BAD?
//'Release' invoke on release
public enum MVRInputEventType
{
    TriggerPress,
    TriggerDown,
    TriggerRelease,
    Button1Press,
    Button1Down,
    Button1Release,
    Button2Press,
    Button2Down,
    Button2Release,
    Button3Press,
    Button3Down,
    Button3Release,
    Button4Press,
    Button4Down,
    Button4Release
}

public enum MVRButton
{
    Trigger,
    Button1,
    Button2,
    Button3,
    Button4
}

//Classic singleton style
public class MVRInputHandler : MonoBehaviour
{
    [Header("MVRInuptHandler Settings")]
    public bool debugInput; //for using keyboard/mouse for testing 
    public GameObject hand;


    private static MVRInputHandler _instance;

    public static MVRInputHandler Instance { get { return _instance; } }

    public GameObject seen { get { return _seen; } }
    public Ray handRay { get { return _ray; } }
    private UnityEvent[] inputEvents;
    private bool[] buttonStates;
    private GameObject _seen; //what is currently seen with hand ray
    private Ray _ray; //the ray from the hand itself

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            print("unwanted attempt to create additional mvr input handler!");
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        //creates array entry for each input type in enum
        int inputTypeCount = Enum.GetNames(typeof(MVRInputEventType)).Length;
        inputEvents = new UnityEvent[inputTypeCount];
        for (int i = 0; i < inputEvents.Length; i++)
        {
            inputEvents[i] = new UnityEvent();
        }

        //Array for states of buttons
        int buttonCount = Enum.GetNames(typeof(MVRButton)).Length;
        buttonStates = new bool[buttonCount];
        for (int i = 0; i < buttonStates.Length; i++)
        {
            buttonStates[i] = false;
        }

        _seen = null;
    }

    // Update is called once per frame
    void Update()
    {
        //Send out ray from hand
        if (debugInput)
        {
            //Determine mouse pos for debug ray
            Vector3 mousePos3D = Vector3.zero;
            Plane plane = new Plane(Navigation.Instance.transform.forward,
                Navigation.Instance.transform.position + Navigation.Instance.transform.forward * 2.5f); //CONFIGURED FOR MENU POSITION
            float distance;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (plane.Raycast(ray, out distance))
            {
                mousePos3D = ray.GetPoint(distance);
            }

            //Create new ray
            _ray = new Ray(hand.transform.position, mousePos3D - hand.transform.position);
            RaycastHit hit;
            if (Physics.Raycast(_ray, out hit))
            {
                print(hit.collider.gameObject.name);
                _seen = hit.collider.gameObject;
            }
            else
                _seen = null;

            Debug.DrawRay(_ray.origin, _ray.direction);

            LineRenderer lr = hand.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, _ray.origin);
            lr.SetPosition(1, _ray.origin + _ray.direction * 1000);
        }
        else
        {
            //MVR way to handle rays and object detection

            LineRenderer lr = hand.GetComponent<LineRenderer>();
            lr.positionCount = 2;
            

            //Create new ray and look for hit
            _ray = new Ray(hand.transform.position, hand.transform.forward);
            RaycastHit hit;
            if (Physics.Raycast(_ray, out hit))
            {
                print(hit.collider.gameObject.name);
                _seen = hit.collider.gameObject;
                lr.SetPosition(1, hit.point); 
            }
            else
            {
                _seen = null;
                lr.SetPosition(1, _ray.origin + _ray.direction * 1000);
            }
            lr.SetPosition(0, _ray.origin);

            Debug.DrawRay(_ray.origin, _ray.direction);
        }

        //Test for all input events here through mvr, to limit where it is interacted with
        if (MVR.DeviceMgr != null)
        {
            // Getting state of primary wand button:
            //MVR.DeviceMgr.IsWandButtonPressed(0)
            // Getting toggled state of primary wand button:
            //bool wandButtonToggled0 = MVR.DeviceMgr.IsWandButtonToggled(0);

            //*USING KEYBOARD INPUT SYSTEM FOR TESTING TEMPORARILY*

            //TRIGGER
            if (MVR.DeviceMgr.IsWandButtonPressed(0) || Input.GetKey(KeyCode.Space))
            {
                if (!buttonStates[(int)MVRButton.Trigger]) //if trigger pressed
                {
                    inputEvents[(int)MVRInputEventType.TriggerPress].Invoke();
                    buttonStates[(int)MVRButton.Trigger] = true;
                }

                //While trigger down
                inputEvents[(int)MVRInputEventType.TriggerDown].Invoke();
            }
            else if (buttonStates[(int)MVRButton.Trigger]) //if trigger released
            {
                inputEvents[(int)MVRInputEventType.TriggerRelease].Invoke();
                buttonStates[(int)MVRButton.Trigger] = false;
            }

            //Button 1
            if (MVR.DeviceMgr.IsWandButtonPressed(1) || Input.GetKey(KeyCode.Alpha1))
            {
                if (!buttonStates[(int)MVRButton.Button1]) //if Button 1 pressed
                {
                    inputEvents[(int)MVRInputEventType.Button1Press].Invoke();
                    buttonStates[(int)MVRButton.Button1] = true;
                }

                //While Button 1 down
                inputEvents[(int)MVRInputEventType.Button1Down].Invoke();
            }
            else if (buttonStates[(int)MVRButton.Button1]) //if Button 1 released
            {
                inputEvents[(int)MVRInputEventType.Button1Release].Invoke();
                buttonStates[(int)MVRButton.Button1] = false;
            }

            //Button 2
            if (MVR.DeviceMgr.IsWandButtonPressed(2) || Input.GetKey(KeyCode.Alpha2))
            {
                if (!buttonStates[(int)MVRButton.Button2]) //if Button 1 pressed
                {
                    inputEvents[(int)MVRInputEventType.Button2Press].Invoke();
                    buttonStates[(int)MVRButton.Button2] = true;
                }

                //While Button 1 down
                inputEvents[(int)MVRInputEventType.Button2Down].Invoke();
            }
            else if (buttonStates[(int)MVRButton.Button2]) //if Button 1 released
            {
                inputEvents[(int)MVRInputEventType.Button2Release].Invoke();
                buttonStates[(int)MVRButton.Button2] = false;
            }

            //Button 3
            if (MVR.DeviceMgr.IsWandButtonPressed(3) || Input.GetKey(KeyCode.Alpha3))
            {
                if (!buttonStates[(int)MVRButton.Button3]) //if Button 1 pressed
                {
                    inputEvents[(int)MVRInputEventType.Button3Press].Invoke();
                    buttonStates[(int)MVRButton.Button3] = true;
                }

                //While Button 1 down
                inputEvents[(int)MVRInputEventType.Button3Down].Invoke();
            }
            else if (buttonStates[(int)MVRButton.Button3]) //if Button 1 released
            {
                inputEvents[(int)MVRInputEventType.Button3Release].Invoke();
                buttonStates[(int)MVRButton.Button3] = false;
            }

            //Button 4
            if (MVR.DeviceMgr.IsWandButtonPressed(4) || Input.GetKey(KeyCode.Alpha4))
            {
                if (!buttonStates[(int)MVRButton.Button4]) //if Button 1 pressed
                {
                    inputEvents[(int)MVRInputEventType.Button4Press].Invoke();
                    buttonStates[(int)MVRButton.Button4] = true;
                }

                //While Button 1 down
                inputEvents[(int)MVRInputEventType.Button4Down].Invoke();
            }
            else if (buttonStates[(int)MVRButton.Button4]) //if Button 1 released
            {
                inputEvents[(int)MVRInputEventType.Button4Release].Invoke();
                buttonStates[(int)MVRButton.Button4] = false;
            }
        }
    }

    public static void SubscribeTo(MVRInputEventType inputEventType, UnityAction callback)
    {
        Instance.inputEvents[(int)inputEventType].AddListener(callback);
    }

    public static void UnsubscribeTo(MVRInputEventType inputEventType, UnityAction callback)
    {
        Instance.inputEvents[(int)inputEventType].RemoveListener(callback);
    }

    public static float GetJoystickVerticalAxis()
    {
        return MVR.DeviceMgr.GetWandVerticalAxisValue();
        //return 0f;
    }

    public static float GetJoystickHorizontalAxis()
    {
        return MVR.DeviceMgr.GetWandHorizontalAxisValue();
        //return 0f;
    }
}


