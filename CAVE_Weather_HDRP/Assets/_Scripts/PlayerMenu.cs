using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMenu : MonoBehaviour
{
    [Header("PlayerMenu Settings")]
    public float transitionSpeed = 5;
    public Color btnNormalColor, btnHoveredColor;
    public MVRInputEventType mvrMenuToggleInputEvent, mvrActivateButtonInputEvent;
    public Button closeMenuUIBtn, tp1UIBtn, tp2UIBtn, tp3UIBtn, tp4UIBtn, instantTPBtn, cloudToggleBtn, elevIndicatorToggleBtn, labelToggleBtn, quizModeToggleBtn;
    public GameObject closeMenuBtnColliderObj, tpBtn1ColliderObj, tpBtn2ColliderObj;
    public GameObject tpBtn3ColliderObj, tpBtn4ColliderObj, instantTPToggleBtnColliderObj, cloudToggleBtnColliderObj, elevIndicatorToggleBtnColliderObj;
    public GameObject labelToggleBtnColliderObj, quizModeToggleBtnColliderObj;
    public GameObject tpNode1, tpNode2, tpNode3, tpNode4;
    public GameObject clouds1, clouds2;
    public GameObject elevIndicator;
    public GameObject label1, label2, label3;
    public GameObject rainZone, snowZone, thunderstormZone;
    public GameObject quizModeDialog;

    //whether or not the player menu is visible and interactable or not, or in a transition state
    private enum UIStatus
    {
        deploying, deployed, concealing, concealed
    }

    //Data
    private UIStatus status;
    private CanvasGroup group;
    private Vector3 initialScale;
    private float transitionStart;
    private GameObject targetButtonCollider;
    private Button targetUIButton;
    private bool cloudsOn, elevIndShown, labelsShown;
    private QuizMode quizMode;

    private void Awake()
    {
        status = UIStatus.deployed; //Start deployed
        group = transform.Find("Canvas").GetComponent<CanvasGroup>();
        initialScale = transform.localScale; //Right kind of scale?
        targetButtonCollider = null;
        targetUIButton = null;
        cloudsOn = elevIndShown = labelsShown = true;
        quizMode = quizModeDialog.GetComponent<QuizMode>();
    }

    private void Start()
    {
        //Subscribe to target MVR wand button event, in start to avoid race conditions with input handler!
        MVRInputHandler.SubscribeTo(mvrMenuToggleInputEvent, ToggleUI);
        MVRInputHandler.SubscribeTo(mvrActivateButtonInputEvent, ActivateMenuButton);
    }

    private void ToggleUI()
    {
        if (status == UIStatus.concealed)
        {
            status = UIStatus.deploying;
            transitionStart = Time.time;
        }
        else if (status == UIStatus.deployed)
        {
            status = UIStatus.concealing;
            transitionStart = Time.time;
            //Disable interaction upon conceal request
            group.interactable = false;
            group.blocksRaycasts = false;

            if (targetButtonCollider != null) //reset current button if minimized
            {
                ColorBlock tmp = targetUIButton.colors;
                tmp.normalColor = btnNormalColor;
                targetUIButton.colors = tmp;
                targetButtonCollider = null;
                targetUIButton = null;
            }
        }
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
                //Enable interaction upon finished deployment
                group.interactable = true;
                group.blocksRaycasts = true;
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
            tpBtn1ColliderObj.transform.position = tp1UIBtn.transform.position;
            tpBtn2ColliderObj.transform.position = tp2UIBtn.transform.position;
            tpBtn3ColliderObj.transform.position = tp3UIBtn.transform.position;
            tpBtn4ColliderObj.transform.position = tp4UIBtn.transform.position;
            closeMenuBtnColliderObj.transform.position = closeMenuUIBtn.transform.position;
            instantTPToggleBtnColliderObj.transform.position = instantTPBtn.transform.position;
            cloudToggleBtnColliderObj.transform.position = cloudToggleBtn.transform.position;
            elevIndicatorToggleBtnColliderObj.transform.position = elevIndicatorToggleBtn.transform.position;
            labelToggleBtnColliderObj.transform.position = labelToggleBtn.transform.position;
            quizModeToggleBtnColliderObj.transform.position = quizModeToggleBtn.transform.position;

            //Look to see if hand ray is pointing towards any buttons
            GameObject seen = MVRInputHandler.Instance.seen;
            if (seen != null && seen.CompareTag("Button"))
            {
                if (targetButtonCollider != null && targetButtonCollider != seen) //if different button previously seen
                {
                    ColorBlock tmp = targetUIButton.colors;
                    tmp.normalColor = btnNormalColor;
                    targetUIButton.colors = tmp;
                    targetButtonCollider = null;
                    targetUIButton = null;
                }
                if (targetButtonCollider == null)
                {
                    targetUIButton = ButtonMatchingCollider(seen);
                    ColorBlock tmp = targetUIButton.colors;
                    tmp.normalColor = btnHoveredColor;
                    targetUIButton.colors = tmp;
                }
                targetButtonCollider = seen;
            }
            else if (targetButtonCollider != null)
            {
                //Change color back
                ColorBlock tmp = targetUIButton.colors;
                tmp.normalColor = btnNormalColor;
                targetUIButton.colors = tmp;
                targetButtonCollider = null;
                targetUIButton = null;
            }
        }
    }

    private Button ButtonMatchingCollider(GameObject buttonCollider)
    {
        if (buttonCollider == tpBtn1ColliderObj)
        {
            return tp1UIBtn;
        }
        else if (buttonCollider == tpBtn2ColliderObj)
        {
            return tp2UIBtn;
        }
        else if (buttonCollider == tpBtn3ColliderObj)
        {
            return tp3UIBtn;
        }
        else if (buttonCollider == tpBtn4ColliderObj)
        {
            return tp4UIBtn;
        }
        else if (buttonCollider == instantTPToggleBtnColliderObj)
        {
            return instantTPBtn;
        }
        else if (buttonCollider == cloudToggleBtnColliderObj)
        {
            return cloudToggleBtn;
        }
        else if (buttonCollider == elevIndicatorToggleBtnColliderObj)
        {
            return elevIndicatorToggleBtn;
        }
        else if (buttonCollider == labelToggleBtnColliderObj)
        {
            return labelToggleBtn;
        }
        else if (buttonCollider == quizModeToggleBtnColliderObj)
        {
            return quizModeToggleBtn;
        }
        else
        {
            return closeMenuUIBtn;
        }
    }

    public void ActivateMenuButton()
    {
        if (targetButtonCollider != null)
        {
            if (targetButtonCollider == tpBtn1ColliderObj)
            {
                Navigation.Instance.MenuTPRequest(tpNode1);
            }
            else if (targetButtonCollider == tpBtn2ColliderObj)
            {
                Navigation.Instance.MenuTPRequest(tpNode2);
            }
            else if (targetButtonCollider == tpBtn3ColliderObj)
            {
                Navigation.Instance.MenuTPRequest(tpNode3);
            }
            else if (targetButtonCollider == tpBtn4ColliderObj)
            {
                Navigation.Instance.MenuTPRequest(tpNode4);
            }
            else if (targetButtonCollider == closeMenuBtnColliderObj)
            {
                ToggleUI();
            }
            else if (targetButtonCollider == instantTPToggleBtnColliderObj)
            {
                if (Navigation.Instance.MenuToggleInstantTP())
                    instantTPBtn.GetComponentInChildren<Text>().text = "Turn Instant TP OFF";
                else
                    instantTPBtn.GetComponentInChildren<Text>().text = "Turn Instant TP ON";
            }
            else if (targetButtonCollider == cloudToggleBtnColliderObj)
            {
                if (!quizMode.IsActive()) //dont allow cloud toggle while in quizmode!
                {
                    if (!cloudsOn)
                    {
                        TurnCloudsOn();
                    }
                    else
                    {
                        TurnCloudsOff();
                    }
                }
            }
            else if (targetButtonCollider == elevIndicatorToggleBtnColliderObj)
            {
                if (elevIndShown)
                {
                    elevIndicator.GetComponent<ElevationIndicator>().Hide();
                    elevIndicatorToggleBtn.GetComponentInChildren<Text>().text = "Show Elevation Indicator";
                    elevIndShown = false;
                }
                else
                {
                    elevIndicator.GetComponent<ElevationIndicator>().Show();
                    elevIndicatorToggleBtn.GetComponentInChildren<Text>().text = "Hide Elevation Indicator";
                    elevIndShown = true;
                }
            }
            else if (targetButtonCollider == labelToggleBtnColliderObj)
            {
                if (!quizMode.IsActive()) //dont allow label toggle while in quizmode!
                {
                    if (labelsShown)
                    {
                        HideLabels();
                    }
                    else
                    {
                        ShowLabels();
                    }
                }
            }
            else if (targetButtonCollider == quizModeToggleBtnColliderObj)
            {
                if (!quizMode.IsActive())
                {
                    quizMode.EnterQuizMode();
                    quizModeToggleBtn.GetComponentInChildren<Text>().text = "Exit Quiz Mode";

                    //Disable buttons which will alter quiz mode and ensure clouds are on and labels are off

                    if (!cloudsOn)
                    {
                        TurnCloudsOn();
                    }
                    if (labelsShown)
                    {
                        HideLabels();
                    }

                }
                else
                {
                    quizMode.ExitQuizMode();
                    quizModeToggleBtn.GetComponentInChildren<Text>().text = "Enter Quiz Mode";
                }
                //Define activation/deactivation of quiz mode here

            }
        }
    }

    private void TurnCloudsOff()
    {
        clouds1.SetActive(false);
        clouds2.SetActive(false);
        rainZone.SetActive(false);
        snowZone.SetActive(false);
        thunderstormZone.SetActive(false);
        cloudToggleBtn.GetComponentInChildren<Text>().text = "Turn Clouds ON";
        cloudsOn = false;
    }

    private void TurnCloudsOn()
    {
        clouds1.SetActive(true);
        clouds2.SetActive(true);
        rainZone.SetActive(true);
        snowZone.SetActive(true);
        thunderstormZone.SetActive(true);
        cloudToggleBtn.GetComponentInChildren<Text>().text = "Turn Clouds OFF";
        cloudsOn = true;
    }

    private void HideLabels()
    {
        label1.SetActive(false);
        label2.SetActive(false);
        label3.SetActive(false);
        labelToggleBtn.GetComponentInChildren<Text>().text = "Show Labels";
        labelsShown = false;
    }

    private void ShowLabels()
    {
        label1.SetActive(true);
        label2.SetActive(true);
        label3.SetActive(true);
        labelToggleBtn.GetComponentInChildren<Text>().text = "Hide Labels";
        labelsShown = true;
    }

    public void QuizModeCompleted()
    {
        quizModeToggleBtn.GetComponentInChildren<Text>().text = "Enter Quiz Mode";
    }
}
