using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuizMode : MonoBehaviour
{
    [Header("Quiz Mode Settings")]
    public GameObject WarmArrow, ColdArrow, DryArrow;
    public Material neutralMaterial; //set arrows to this mat upon entering quiz mode
    public GameObject RainZone, SnowZone, ThunderstormZone;
    public MVRInputEventType mvrQuizModeSelectInputEvent;
    public Text quizModeDialogTxt;
    public float transitionSpeed = 5; //for showing and hiding
    public float questionResultDisplayTime = 5;
    public GameObject playerMenu;

    private enum UIStatus
    {
        deploying, deployed, concealing, concealed
    }

    private enum QuestionStatus
    {
        unanswered, correct, incorrect
    }

    //One way to enter quiz mode: from the menu.
    //Two ways to exit quiz mode: from the menu, or by completing it.
    //Upon entering, information is removed from the environment (color of arrows and type of percipitation from clouds).
    //Upon exiting, all information removed needs to be restored.

    //Data
    private UIStatus status; //for concealing and deploying
    private Vector3 initialScale; //for concealing and deploying
    private float transitionStart; //for concealing and deploying

    private bool quizModeActive, quizEndRecap;
    private int currentQuestionNum, totalQuestionsAnsweredCorrect, totalQuestionCount;
    private QuestionStatus currentQuestionStatus;
    private Material warmArrowMat, coldArrowMat, dryArrowMat;
    private float questionResultDisplayStart;

    private void Awake()
    {
        status = UIStatus.deployed; //Start deployed
        initialScale = transform.localScale; //Right kind of scale?

        quizModeActive = quizEndRecap = false;
        totalQuestionCount = 6;
        totalQuestionsAnsweredCorrect = 0;

        warmArrowMat = WarmArrow.GetComponentInChildren<MeshRenderer>().material;
        coldArrowMat = ColdArrow.GetComponentInChildren<MeshRenderer>().material;
        dryArrowMat = DryArrow.GetComponentInChildren<MeshRenderer>().material;
    }

    private void Start()
    {
        //Subscribe to target MVR wand button event, in start to avoid race conditions with input handler!
        MVRInputHandler.SubscribeTo(mvrQuizModeSelectInputEvent, SelectionMade);

        //Start hidden
        Hide();
    }

    // Update is called once per frame
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

        //move to next question or end of quiz after displaying results
        if (quizModeActive)
        {
            if (quizEndRecap && Time.time > questionResultDisplayStart + questionResultDisplayTime)
            {
                ExitQuizMode(); //EXIT QUIZ MODE HERE
                playerMenu.GetComponent<PlayerMenu>().QuizModeCompleted();
            }
            else if (currentQuestionStatus != QuestionStatus.unanswered && Time.time > questionResultDisplayStart + questionResultDisplayTime)
            {
                currentQuestionStatus = QuestionStatus.unanswered;
                currentQuestionNum++;
                UpdateDialog();
                if (currentQuestionNum == totalQuestionCount)
                {
                    questionResultDisplayStart = Time.time;
                    quizEndRecap = true;
                    quizModeDialogTxt.text = "Quiz finished, " + totalQuestionsAnsweredCorrect + "/" + totalQuestionCount + " questions answered correctly";
                }
            }
        }
    }

    private void UpdateDialog()
    {
        string specificQuestionStr = "";
        switch (currentQuestionNum)
        {
            case 0:
                specificQuestionStr = "cold conveyor belt";
                break;
            case 1:
                specificQuestionStr = "warm conveyor belt";
                break;
            case 2:
                specificQuestionStr = "dry conveyor belt";
                break;
            case 3:
                specificQuestionStr = "cloud zone generating rain";
                break;
            case 4:
                specificQuestionStr = "cloud zone generating snow";
                break;
            case 5:
                specificQuestionStr = "cloud zone generating thunderstorms";
                break;
        }
        switch (currentQuestionStatus)
        {
            case QuestionStatus.unanswered:
                quizModeDialogTxt.color = Color.white;
                quizModeDialogTxt.text = "Select the " + specificQuestionStr; 
                break;
            case QuestionStatus.correct:
                quizModeDialogTxt.color = Color.green;
                quizModeDialogTxt.text = "Correct";
                break;
            case QuestionStatus.incorrect:
                quizModeDialogTxt.color = Color.red;
                quizModeDialogTxt.text = "Incorrect";
                break;
        }
    }

    public void SelectionMade()
    {
        //Only acknowledge selection if quizmode is infact active and the current question is unanswered.
        if (quizModeActive && currentQuestionStatus == QuestionStatus.unanswered)
        {
            switch (currentQuestionNum)
            {
                case 0:
                    if (MVRInputHandler.Instance.seen.tag == "coldArrow" || MVRInputHandler.Instance.seen.tag == "warmArrow" || MVRInputHandler.Instance.seen.tag == "dryArrow")
                    {
                        if (MVRInputHandler.Instance.seen.tag == "coldArrow")
                        {
                            CorrectAnswer();
                        }
                        else
                        {
                            IncorrectAnswer();
                        }
                        ColdArrow.transform.Find("Cyl").gameObject.GetComponent<MeshRenderer>().material = coldArrowMat;
                        ColdArrow.transform.Find("Cone").gameObject.GetComponent<MeshRenderer>().material = coldArrowMat;
                        ColdArrow.transform.Find("SecondArrowhead").gameObject.GetComponent<MeshRenderer>().material = coldArrowMat;
                        ColdArrow.transform.Find("Cone1").gameObject.GetComponent<MeshRenderer>().material = coldArrowMat;
                    }
                    break;
                case 1:
                    if (MVRInputHandler.Instance.seen.tag == "coldArrow" || MVRInputHandler.Instance.seen.tag == "warmArrow" || MVRInputHandler.Instance.seen.tag == "dryArrow")
                    {
                        if (MVRInputHandler.Instance.seen.tag == "warmArrow")
                        {
                            CorrectAnswer();
                        }
                        else
                        {
                            IncorrectAnswer();
                        }
                        WarmArrow.transform.Find("Cyl").gameObject.GetComponent<MeshRenderer>().material = warmArrowMat;
                        WarmArrow.transform.Find("Cone").gameObject.GetComponent<MeshRenderer>().material = warmArrowMat;
                    }
                    break;
                case 2:
                    if (MVRInputHandler.Instance.seen.tag == "coldArrow" || MVRInputHandler.Instance.seen.tag == "warmArrow" || MVRInputHandler.Instance.seen.tag == "dryArrow")
                    {
                        if (MVRInputHandler.Instance.seen.tag == "dryArrow")
                        {
                            CorrectAnswer();
                        }
                        else
                        {
                            IncorrectAnswer();
                        }
                        DryArrow.transform.Find("Cyl").gameObject.GetComponent<MeshRenderer>().material = dryArrowMat;
                        DryArrow.transform.Find("Cone").gameObject.GetComponent<MeshRenderer>().material = dryArrowMat;
                    }
                    break;
                case 3:
                    if (MVRInputHandler.Instance.seen.tag == "rainZone" || MVRInputHandler.Instance.seen.tag == "snowZone" || MVRInputHandler.Instance.seen.tag == "thunderstormZone")
                    {
                        if (MVRInputHandler.Instance.seen.tag == "rainZone")
                        {
                            CorrectAnswer();
                        }
                        else
                        {
                            IncorrectAnswer();
                        }
                        RainZone.transform.Find("ParticleSystem").gameObject.SetActive(true);
                    }
                    break;
                case 4:
                    if (MVRInputHandler.Instance.seen.tag == "rainZone" || MVRInputHandler.Instance.seen.tag == "snowZone" || MVRInputHandler.Instance.seen.tag == "thunderstormZone")
                    {
                        if (MVRInputHandler.Instance.seen.tag == "snowZone")
                        {
                            CorrectAnswer();
                        }
                        else
                        {
                            IncorrectAnswer();
                        }
                        SnowZone.transform.Find("ParticleSystem").gameObject.SetActive(true);
                    }
                    break;
                case 5:
                    if (MVRInputHandler.Instance.seen.tag == "rainZone" || MVRInputHandler.Instance.seen.tag == "snowZone" || MVRInputHandler.Instance.seen.tag == "thunderstormZone")
                    {
                        if (MVRInputHandler.Instance.seen.tag == "thunderstormZone")
                        {
                            CorrectAnswer();
                        }
                        else
                        {
                            IncorrectAnswer();
                        }
                        ThunderstormZone.transform.Find("ParticleSystem").gameObject.SetActive(true);
                        ThunderstormZone.transform.Find("ParticleSystem2").gameObject.SetActive(true);
                    }
                    break;
            }
        }
    }

    private void IncorrectAnswer()
    {
        questionResultDisplayStart = Time.time;
        currentQuestionStatus = QuestionStatus.incorrect;
        UpdateDialog();
    }

    private void CorrectAnswer()
    {
        questionResultDisplayStart = Time.time;
        totalQuestionsAnsweredCorrect++;
        currentQuestionStatus = QuestionStatus.correct;
        UpdateDialog();
    }

    public void EnterQuizMode()
    {
        quizModeActive = true;
        currentQuestionNum = 0;
        currentQuestionStatus = QuestionStatus.unanswered;

        //Hide arrow colors and precip zones:
        WarmArrow.transform.Find("Cyl").gameObject.GetComponent<MeshRenderer>().material = neutralMaterial;
        WarmArrow.transform.Find("Cone").gameObject.GetComponent<MeshRenderer>().material = neutralMaterial;
        ColdArrow.transform.Find("Cyl").gameObject.GetComponent<MeshRenderer>().material = neutralMaterial;
        ColdArrow.transform.Find("Cone").gameObject.GetComponent<MeshRenderer>().material = neutralMaterial;
        ColdArrow.transform.Find("SecondArrowhead").gameObject.GetComponent<MeshRenderer>().material = neutralMaterial;
        ColdArrow.transform.Find("Cone1").gameObject.GetComponent<MeshRenderer>().material = neutralMaterial;
        DryArrow.transform.Find("Cyl").gameObject.GetComponent<MeshRenderer>().material = neutralMaterial;
        DryArrow.transform.Find("Cone").gameObject.GetComponent<MeshRenderer>().material = neutralMaterial;
        RainZone.transform.Find("ParticleSystem").gameObject.SetActive(false);
        SnowZone.transform.Find("ParticleSystem").gameObject.SetActive(false);
        ThunderstormZone.transform.Find("ParticleSystem").gameObject.SetActive(false);
        ThunderstormZone.transform.Find("ParticleSystem2").gameObject.SetActive(false);

        UpdateDialog();
        Show();
    }

    public void ExitQuizMode()
    {
        quizEndRecap = false;
        quizModeActive = false;
        quizModeActive = quizEndRecap = false;
        totalQuestionsAnsweredCorrect = 0;

        //Restore arrow colors and precip zones:
        WarmArrow.transform.Find("Cyl").gameObject.GetComponent<MeshRenderer>().material = warmArrowMat;
        WarmArrow.transform.Find("Cone").gameObject.GetComponent<MeshRenderer>().material = warmArrowMat;
        ColdArrow.transform.Find("Cyl").gameObject.GetComponent<MeshRenderer>().material = coldArrowMat;
        ColdArrow.transform.Find("Cone").gameObject.GetComponent<MeshRenderer>().material = coldArrowMat;
        ColdArrow.transform.Find("SecondArrowhead").gameObject.GetComponent<MeshRenderer>().material = coldArrowMat;
        ColdArrow.transform.Find("Cone1").gameObject.GetComponent<MeshRenderer>().material = coldArrowMat;
        DryArrow.transform.Find("Cyl").gameObject.GetComponent<MeshRenderer>().material = dryArrowMat;
        DryArrow.transform.Find("Cone").gameObject.GetComponent<MeshRenderer>().material = dryArrowMat;
        RainZone.transform.Find("ParticleSystem").gameObject.SetActive(true);
        SnowZone.transform.Find("ParticleSystem").gameObject.SetActive(true);
        ThunderstormZone.transform.Find("ParticleSystem").gameObject.SetActive(true);
        ThunderstormZone.transform.Find("ParticleSystem2").gameObject.SetActive(true);

        Hide();
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

    public bool IsActive()
    {
        return quizModeActive;
    }

}
