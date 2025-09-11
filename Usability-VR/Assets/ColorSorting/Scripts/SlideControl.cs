using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class SlideControlUI : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────
    //                       PUBLIC INSPECTOR FIELDS
    // ──────────────────────────────────────────────────────────


    [Header("GUI elements")]
    public GameObject canvas;                   // GUI to hide after tasks (black screen)
    public GameObject background;               // blue background
    public Transform slideContainer;            // Color slides
    public Transform slideContainerTutorial;    // Tutorial slides
    public GameObject successIndicator;         // Show success window
    public GameObject canvasA;                  // GUI shown in Phase A
    public GameObject canvasB;                  // GUI shown in Phase B
    public ProximityDetect proximityDetect;

    [Header("GUI elements for mode with Usability Errors")]
    public GameObject backgroundErrors;         // shown if withErrors=yes
    public TMP_Text moveCounterText;            // textfield to show move count
    public GameObject canvasBSortErrors;        // GUI shown in Phase B error mode
    public Transform slideContainerErrors;      // Color slides for error mode

    [Header("References")]
    public DataExport dataExport;               // data csv
    public AudioSource endA;                    // sound End phase A
    public AudioSource endB;                    // sound End phase B
    public AudioSource end4Sessions;            // sound end 4 sessions
    public AudioSource falseNegSound;           // sound warnings
    public float successWindowTime = 2f;        // Time for success window

    [Header("STUDY SETTINGS")]
    public string userID = "user01";            // current user identifier

    // Select mode: tutorial - normal gui - gui with errors
    public enum Mode { tutorial, normal, withErrors } 
    public Mode mode = Mode.normal;
    [NonSerialized] public int numberPhaseA = 0;
    [NonSerialized] public int numberPhaseB = 0;
    [NonSerialized] public int falseNegA;
    [NonSerialized] public int falseNegB;
    [NonSerialized] public int falseNegRoundCounter = 0;         // max. 3 errors per runcount (A + B)
    [NonSerialized] public bool tutorialPhaseA = false;
    

    // ─────────────────────────────────────────────────────────────────────────
    //                         PRIVATE STATE FIELDS
    // ─────────────────────────────────────────────────────────────────────────

    private enum Phase { None, Tutorial, A, B }           // phase A = Table movement, B = Display sort, none = between tests
    private Phase currentPhase = Phase.None;
    private int runCount = 0;                    // Counts completed AB cycles
    private int taskNr = 0;                      // Incremental per user
    private List<RectTransform> originalOrder;  // The “solved” order
    private List<RectTransform> slides;         // Current order (mutable)
    private int cursorIndex;
    private int selectedIndex;
    private int stepCount = 0;
    private int movesPhaseA;
    private int movesPhaseB;
    private float taskStartTime;
    private DateTime phaseStartDateTime;        // tima at phase start
    private bool inputEnabled;
    private bool goalReached;
    private int currentSortOrderIndex;          // to save the used array
    private int previousSortOrderIndex = -1;    // -1 for a hit the first time
    private float firstKeypressTime = -1f;      // time for first key press
    private float RandIgnore;
    private List<float> phaseADistances = new List<float>();     // List for the measured distances

    private readonly int[][] predefinedOrders = new int[][]    // Unsorted sequences for 8-12 moves - selected randomly
    {
        // {Order of Colors}    distances for A (table movement), gestures for B (sorting)
        new[] {1, 4, 3, 2},  // 12, 12  
        new[] {2, 4, 1, 3},  // 16, 12
        new[] {2, 4, 3, 1},  // 12, 16
        new[] {3, 1, 4, 2},  // 13, 12
        new[] {3, 2, 1, 4},  // 14, 12
        new[] {3, 2, 4, 1},  // 12, 16
        new[] {3, 4, 1, 2},  // 14, 16
        new[] {4, 1, 2, 3},  // 13, 12
        new[] {4, 1, 3, 2},  // 13, 16
        new[] {4, 2, 1, 3},  // 14, 16
    };


    void Start()
    {
        // hide all UI on default
        canvas.SetActive(false);
        canvasA.SetActive(false);
        canvasB.SetActive(false);
        canvasBSortErrors.SetActive(false);
        backgroundErrors.SetActive(false);
        background.SetActive(false);
        slideContainerTutorial.gameObject.SetActive(false);
        slideContainer.gameObject.SetActive(false);
        slideContainerErrors.gameObject.SetActive(false);

        currentPhase = Phase.None;
        if (mode == Mode.tutorial) StartCoroutine(RunTutorial());    // start Tutorial if check active
        else          StartCoroutine(RunPhaseA());        // otherwise start with A
    }

    void Update()
    {
        // start timer on first relevant keypress
        if (inputEnabled && firstKeypressTime < 0f)
        {
            bool relevantKey = Input.GetKeyDown(KeyCode.UpArrow)
                            || Input.GetKeyDown(KeyCode.DownArrow)
                            || Input.GetKeyDown(KeyCode.Return)
                            || Input.GetKeyDown(KeyCode.Space);

            if (relevantKey)
                firstKeypressTime = Time.time;  // remember moment of first key
        }

        // Tutorial control
        if (currentPhase == Phase.Tutorial)
        {
            if (!inputEnabled) return;

            // Space ends tutorial - if sorted correctly
            if (goalReached && Input.GetKeyDown(KeyCode.Space))
            {
                inputEnabled = false;
                StartCoroutine(EndTutorial());
                return;
            }

            // Tutorial - A part: Move through table with Return
            if (stepCount < 3)
            {
                tutorialPhaseA = true;
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    MoveCursor(1);
                    stepCount++;
                }
                return;
            }

            // Movement for sorting (Tutorial - B part)
            // reset cursor once when entering B-part
            if (stepCount == 3)
            {
                tutorialPhaseA = false;
                MoveCursor(-2);
                stepCount++;  // ensure this block runs only once
            }

            // now same controls as in Phase B
            if (selectedIndex < 0)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow)) MoveCursor(-1);
                if (Input.GetKeyDown(KeyCode.DownArrow)) MoveCursor(1);
                if (Input.GetKeyDown(KeyCode.Return)) SelectSlide();
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Return)) DeselectSlide();
                else if (Input.GetKeyDown(KeyCode.UpArrow)) SwapSlide(-1);
                else if (Input.GetKeyDown(KeyCode.DownArrow)) SwapSlide(1);
            }

            return;
        }

        // update move counter UI
        if (moveCounterText)
            moveCounterText.text = $"A:{movesPhaseA} B:{movesPhaseB}";

        // start Phase A on Space when idle
        if (!inputEnabled && Input.GetKeyDown(KeyCode.Space) && currentPhase == Phase.None)
        {
            StartCoroutine(RunPhaseA());
            return;
        }

        if (!inputEnabled) return;
        CountGestures();

        // Phase A controls
        if (currentPhase == Phase.A)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // After 4 Moves, End A and start B
                if (cursorIndex == slides.Count - 1)
                {
                    phaseADistances.Add(proximityDetect.CheckProximity(cursorIndex, predefinedOrders[currentSortOrderIndex]));  //Measures the distance from the target color
                    EndTaskPhaseA();
                    numberPhaseA++;
                    endA.Play();
                    StartCoroutine(RunPhaseB());
                }
                else // otherwise move cursor with enter
                {
                    phaseADistances.Add(proximityDetect.CheckProximity(cursorIndex, predefinedOrders[currentSortOrderIndex]));

                    if (mode == Mode.withErrors)
                    {
                        StartCoroutine(DelayedCursorMove());
                    }
                    else
                    {
                        MoveCursor(1);
                    }
                }
            }
            return;
        }

        // Phase B controls
        if (currentPhase == Phase.B)
        {
            RandIgnore = UnityEngine.Random.value;  // determine if this input will be ignored
            // ignore input only if under the 3-ignore limit and count
            if (mode == Mode.withErrors && RandIgnore < 0.15f && falseNegRoundCounter < 3
                && (Input.GetKeyDown(KeyCode.UpArrow)
                || Input.GetKeyDown(KeyCode.DownArrow)
                || Input.GetKeyDown(KeyCode.Return)))
            {
                falseNegRoundCounter++; // ignore this input, count as false negative
                falseNegB++;
                // ignoreNextURelease = selectedIndex<0; 
            }
            else
            {
                if (selectedIndex < 0) // move cursor
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        if (mode == Mode.withErrors) StartCoroutine(DelayedAction(() => MoveCursor(-1))); // every if is just added for the delay
                        else MoveCursor(-1);
                    }
                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        if (mode == Mode.withErrors) StartCoroutine(DelayedAction(() => MoveCursor(1)));
                        else MoveCursor(1);
                    }
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        if (mode == Mode.withErrors) StartCoroutine(DelayedAction(() => SelectSlide()));
                        else SelectSlide();
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        if (mode == Mode.withErrors) StartCoroutine(DelayedAction(() => DeselectSlide()));
                        else DeselectSlide();
                    }
                    else if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        if (mode == Mode.withErrors) StartCoroutine(DelayedAction(() => SwapSlide(-1)));
                        else SwapSlide(-1);
                    }
                    else if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        if (mode == Mode.withErrors) StartCoroutine(DelayedAction(() => SwapSlide(1)));
                        else SwapSlide(1);
                    }
                }

            }

            if (goalReached && Input.GetKeyDown(KeyCode.Space))
            {
                EndTaskPhaseB();
                numberPhaseB++;
            }
        }
    }
    private IEnumerator RunTutorial() // Tutorial start
    {
        currentPhase = Phase.Tutorial;
        canvas.SetActive(true);
        slideContainerTutorial.gameObject.SetActive(true);

        // build slides from the Tutorial-Container
        originalOrder = new List<RectTransform>();
        for (int i = 0; i < slideContainerTutorial.childCount; i++)
            originalOrder.Add(slideContainerTutorial.GetChild(i).GetComponent<RectTransform>());

        // fix order 3, 1, 2
        slides = new List<RectTransform>
        {
            originalOrder[2], // 3
            originalOrder[0], // 1
            originalOrder[1]  // 2
        };

        ApplyOrder(slides);

        selectedIndex = -1;
        cursorIndex = 0;
        SetAllCursors(false);
        SetCursor(0, true);

        phaseStartDateTime = DateTime.Now; // track the phase start time
        inputEnabled = true; taskStartTime = Time.time;
        yield break;
    }
    private IEnumerator EndTutorial()
    {
        // hide the tutorial GUI
        slideContainerTutorial.gameObject.SetActive(false);

        // show success window
        successIndicator.SetActive(true);
        yield return new WaitForSeconds(successWindowTime);
        successIndicator.SetActive(false);

        canvas.SetActive(false); // black screen

        // export tutorial data
        inputEnabled =false;
        float duration=Time.time-taskStartTime;
        dataExport?.LogTaskEnd(
            phaseStartDateTime,        // Date und Start time for A
            userID,                    // User ID
            mode,                      // Errors: yes/no
            0,                         // Task Nr
            "(3-1-2)",                 // which index of array
            "Tutorial",                // Phase
            duration,                  // Task time in seconds
            movesPhaseA,               // all Gestures
            0                          // False negative count
        );
    }
    private IEnumerator RunPhaseA()
    {
        currentPhase = Phase.A;
        taskNr++;
        falseNegRoundCounter = 0;  // reset error count for this round
        if (mode == Mode.withErrors && backgroundErrors) backgroundErrors.SetActive(true);
        else background.SetActive(true);
        canvas.SetActive(true);
        canvasA.SetActive(true);
        
        ApplyPredefinedOrder();  // select the sorting

        cursorIndex = 0; selectedIndex = -1; stepCount = 0;
        movesPhaseA = 0; movesPhaseB = 0; falseNegA = 0; falseNegB = 0; goalReached = false;
        SetAllCursors(false); SetCursor(0, true);

        phaseADistances.Clear();

        phaseStartDateTime = DateTime.Now; // track the phase start time
        inputEnabled = true;
        // taskStartTime = Time.time;
        firstKeypressTime = -1f; // reset timer
        yield break;
    }

    private void EndTaskPhaseA()      //export data for A
    {
        inputEnabled=false;
        // float duration=Time.time-taskStartTime;
        float duration = (firstKeypressTime >= 0f) ? Time.time - firstKeypressTime : 0f;
        firstKeypressTime = -1f; // reset
        string currentSortOrderString = string.Join("-", predefinedOrders[currentSortOrderIndex]); 

        dataExport?.LogTaskEndA(
            phaseStartDateTime,       // Date und Start time for A
            userID,                   // User ID
            mode,                     // Errors: yes/no
            taskNr,                   // Task Nr
            currentSortOrderString,   // sorting from array
            "A-Table",                // Phase
            duration,                 // Task time in seconds
            movesPhaseA-falseNegA,    // all Gestures - ignored
            falseNegA,                // False negative count
            phaseADistances           // List of all the measured distances
        );
    }

    private IEnumerator RunPhaseB()
    {
        currentPhase = Phase.B;
        canvasA.SetActive(false);
        // Show error GUI if true
        if (mode == Mode.withErrors) { canvasB.SetActive(false); canvasBSortErrors.SetActive(true); }
        else { canvasB.SetActive(true); canvasBSortErrors.SetActive(false); }

        selectedIndex=-1; cursorIndex=0; stepCount=0;
        movesPhaseB=0; goalReached=false;
        SetAllCursors(false); SetCursor(0,true);

        phaseStartDateTime = DateTime.Now; // track the phase start time
        inputEnabled =true;
        // taskStartTime =Time.time;
        firstKeypressTime = -1f; // reset timer
        yield break;
    }

    private void EndTaskPhaseB()    // export data for B
    {
        inputEnabled = false;
        // float duration = Time.time - taskStartTime;
        float duration = (firstKeypressTime >= 0f) ? Time.time - firstKeypressTime : 0f;
        firstKeypressTime = -1f; // reset
        string currentSortOrderString = string.Join("-", predefinedOrders[currentSortOrderIndex]); 


        dataExport?.LogTaskEnd(
            phaseStartDateTime,     // Date und Start time for B
            userID,                 // User ID
            mode,                   // Errors: yes/no
            taskNr,                 // Task Nr
            currentSortOrderString, // sorting from array
            "B-Screen",             // Phase
            duration,               // Task time in seconds
            movesPhaseB-falseNegB,  // all Gestures in B excl. ignored gestures
            falseNegB               // False negative count
        );
        StartCoroutine(AfterPhaseB());
    }
    private IEnumerator AfterPhaseB()
    {
        if (runCount < 4)  endB.Play(); 
        else               end4Sessions.Play(); // hint that thats the 4th round     
        
        if (successIndicator) { successIndicator.SetActive(true); yield return new WaitForSeconds(successWindowTime); successIndicator.SetActive(false); }

        runCount++; canvas.SetActive(false); canvasA.SetActive(false); canvasB.SetActive(false); canvasBSortErrors.SetActive(false);
  
        backgroundErrors.SetActive(false);
        background.SetActive(false);
        currentPhase =Phase.None;
    }
    private IEnumerator DelayedCursorMove() // delay for A with Errors
    {
        yield return new WaitForSeconds(1f);
        MoveCursor(1);
    }
    private IEnumerator DelayedAction(Action action)  // delay for B with Errors
    {
        yield return new WaitForSeconds(1f);
        action.Invoke();
    }

    void MoveCursor(int dir)
    {
        SetCursor(cursorIndex, false);

        int nextIndex = cursorIndex + dir; // calculate new Index

        // normal cursor movement
        cursorIndex = Mathf.Clamp(nextIndex, 0, slides.Count - 1);
        SetCursor(cursorIndex, true);
    }

    void SelectSlide()
    {
        selectedIndex=cursorIndex;
        SetCursor(selectedIndex,true);
    }

    void DeselectSlide()
    {
        SetCursor(selectedIndex,false);
        selectedIndex=-1;
        SetCursor(cursorIndex,true);
    }

    void SwapSlide(int dir)
    {
        int tgt=selectedIndex+dir; if(tgt<0||tgt>=slides.Count) return;
        var a=slides[selectedIndex]; var b=slides[tgt]; slides[selectedIndex]=b; slides[tgt]=a;
        ApplyOrder(slides);
        selectedIndex=tgt; cursorIndex=tgt;
        SetAllCursors(false); SetCursor(cursorIndex,true);
        stepCount++; if(IsRestored()) goalReached=true;
    }

    void ApplyPredefinedOrder()
    {
         // pick a new slide order thats NOT same as last one
        int newIndex;
        do
        {
            newIndex = UnityEngine.Random.Range(0, predefinedOrders.Length);
        }
        while (newIndex == previousSortOrderIndex);

        currentSortOrderIndex = newIndex; // save which array was used
        previousSortOrderIndex = newIndex;

        // only show slide container we need
        slideContainerTutorial.gameObject.SetActive(mode == Mode.tutorial);
        slideContainer.gameObject.SetActive(mode == Mode.normal);
        slideContainerErrors.gameObject.SetActive(mode == Mode.withErrors);

        // choose container
        Transform container;
        if (mode == Mode.tutorial)
            container = slideContainerTutorial;
        else if (mode == Mode.normal)
            container = slideContainer;
        else // Mode.withError
            container = slideContainerErrors;
        
        // rebuild originalOrder from that container
        originalOrder = new List<RectTransform>();
        for (int i = 0; i < container.childCount; i++)
            originalOrder.Add(container.GetChild(i).GetComponent<RectTransform>());

        // choose Array with index    
        int[] order = predefinedOrders[currentSortOrderIndex];                              

        // apply mapping to slides
        slides = new List<RectTransform>();
        foreach (int idx in order)
            slides.Add(originalOrder[idx - 1]);
            
        ApplyOrder(slides);
    }

    void ApplyOrder(List<RectTransform> order)
    {
        for(int i=0;i<order.Count;i++) order[i].SetSiblingIndex(i);
    }

    bool IsRestored()
    {
        for(int i=0;i<slides.Count;i++) if(slides[i]!=originalOrder[i]) return false;
        return true;
    }

    void SetCursor(int idx,bool active)
    {
        var img=slides[idx].Find("Cursor")?.GetComponent<Image>();
        if (img == null) return;
        img.gameObject.SetActive(active);

        // Phase B: Colors for cursor and logged color
        if (mode == Mode.withErrors)
            img.color = (idx == selectedIndex) ? new Color32(180, 180, 180, 255) : new Color32(100, 100, 100, 255);
        else
            img.color = (idx == selectedIndex) ? new Color32(20, 190, 255, 255) : new Color32(20, 90, 255, 255);
    }

    void SetAllCursors(bool on)
    {
        foreach(var s in slides) s.Find("Cursor")?.gameObject.SetActive(on);
    }


    void CountGestures()      
    {
        // Count gestures (pressed keys) and handle false negatives
        if (!Input.anyKeyDown) return;
        bool relevantKey = Input.GetKeyDown(KeyCode.UpArrow)
                        || Input.GetKeyDown(KeyCode.DownArrow)
                        || Input.GetKeyDown(KeyCode.Return)
                        || Input.GetKeyDown(KeyCode.Space);

        if (!relevantKey) return;

        if (currentPhase == Phase.A)        // counting in phase A (table)
        {
            movesPhaseA++;
        }
        else if (currentPhase == Phase.B)   // counting in phase B (sorting)
        {
            movesPhaseB++;
        }
    }

    void PlayErrorSound()
    {
        if (mode == Mode.withErrors)
            falseNegSound.Play();
    }
}
