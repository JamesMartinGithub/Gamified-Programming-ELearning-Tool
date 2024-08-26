using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrainLevelController : MonoBehaviour
{
    public int[,] trackGrid; // 0=empty 1=track
    public List<Transform>[,] possibleTrackGrid; // n = number of possible connections
    public List<Transform>[,] possibleStopGrid; // n = number of possible connections
    public Track initialTrack;
    public Transform initialTrackPossible;
    public TrackSpawner spawner;
    public float speedMult = 1f;
    public GameObject border;
    public InfoUI infoUI;
    private TrackInterpreter interpreter;
    public GameObject tutorial;
    public AudioSource errorSound;
    public AudioSource victorySound;
    public GameObject victoryPanel;
    //
    private List<string> taskList;
    public TextMeshProUGUI task;
    public TextMeshProUGUI constraint;
    public Text hint;
    private int id;
    private string mustHaveCode;
    private string limitsCode;

    void Start() {
        trackGrid = new int[8, 13];
        possibleTrackGrid = new List<Transform>[8, 13];
        possibleStopGrid = new List<Transform>[8, 13];
        for (int i = 0; i < 8; i++) {
            for (int j = 0; j < 13; j++) {
                possibleTrackGrid[i, j] = new List<Transform>();
                possibleStopGrid[i, j] = new List<Transform>();
            }
        }
        possibleTrackGrid[0, 0] = new List<Transform>{initialTrackPossible};
        interpreter = GetComponent<TrackInterpreter>();
        speedMult = 1f;
        //Setup task
        LevelInfo.Level level = GameObject.Find("Level Information").GetComponent<LevelInfo>().level;
        taskList = level.taskcode;
        if (level.showTutorial) {
            tutorial.SetActive(true);
        }
        task.text = level.task;
        constraint.text = level.constraints;
        hint.text = level.hint;
        id = level.id;
        mustHaveCode = level.musthavecode;
        limitsCode = level.limitscode;

        victoryPanel.SetActive(false);
    }

    public void InterpreterSuccesful(TrainVariables variables) {
        //Convert variables to string list
        List<string> varCode = new List<string>();
        if (variables.GetInitialised("red")) {
            varCode.Add("r" + variables.GetType("red")[0] + variables.GetValue("red").ToLower());
        }
        if (variables.GetInitialised("green")) {
            varCode.Add("g" + variables.GetType("green")[0] + variables.GetValue("green").ToLower());
        }
        if (variables.GetInitialised("blue")) {
            varCode.Add("b" + variables.GetType("blue")[0] + variables.GetValue("blue").ToLower());
        }
        //Compare variable list to task list
        bool matches = true;
        foreach (string s in taskList) {
            if (!varCode.Contains(s)) {
                matches = false;
            }
        }
        //Compare tracks to musthavelist
        if (mustHaveCode.Length > 1) {
            List<string> nodeCode = CreateNodeCode();
            foreach (string code in mustHaveCode.Split('&')) {
                if (!nodeCode.Contains(code)) {
                    matches = false;
                }
            }
        }
        //Compare tracks to limitlist
        if (limitsCode.Length > 1) {
            Dictionary<char, int> limitDict = CreateLimitCode();
            foreach (string code in limitsCode.Split('&')) {
                if (limitDict[code[0]] > int.Parse(code[1].ToString())) {
                    matches = false;
                }
            }
        }
        if (matches) {
            //Task complete
            print("Level complete");
            //Play sound
            victorySound.Play();
            //Show victory screen
            victoryPanel.SetActive(true);
            //Save data
            if (id != -1) {
                GameObject.Find("Level Information").GetComponent<DataSaver>().CompletedLevel(id, true);
            }
        } else {
            print("Interpret successful, but task not complete");
        }
    }

    public void Play() {
        if (!spawner.placing) {
            spawner.spawnable = false;
            interpreter.Interpret();
            border.SetActive(true);
            infoUI.ResetError();
        }
    }

    public void Stop() {
        spawner.spawnable = true;
        interpreter.Clear();
        border.SetActive(false);
    }

    public void SetSpeed(int speedIndex) {
        switch (speedIndex) {
            case 0:
                speedMult = 1f;
                break;
            case 1:
                speedMult = 2f;
                break;
            case 2:
                speedMult = 3f;
                break;
        }
    }

    public void ShowError(string E) {
        infoUI.ShowError(E);
        errorSound.Play();
    }

    public void Exit() {
        SceneManager.LoadScene("Menu");
    }

    private List<string> CreateNodeCode() {
        string CompString(Track track) {
            string value = "";
            switch (track.forBranchOp.type) {
                case "int":
                    value = track.forBranchOp.valueI.ToString(); break;
                case "bool":
                    value = track.forBranchOp.valueB.ToString().ToLower(); break;
                case "string":
                    value = track.forBranchOp.valueS; break;
            }
            return track.forBranchOp.colour[0].ToString() + track.forBranchOp.compOperation.ToString() + value;
        }
        List<string> nodeCode = new List<string>();
        Track[] tracks = FindObjectsOfType<Track>();
        Stop[] stops = FindObjectsOfType<Stop>();
        foreach (Track track in tracks) {
            if (!track.isInitial) {
                switch (track.type) {
                    case "if":
                        
                        nodeCode.Add("i" + CompString(track));
                        break;
                    case "forStart":
                        nodeCode.Add("f" + (track.forRepeats + 1).ToString());
                        break;
                    case "forUntilStart":
                        nodeCode.Add("u" + CompString(track));
                        break;
                    default:
                        break;
                }
            }
        }
        foreach (Stop stop in stops) {
            switch (stop.GetType().Name) {
                case "StopAdd":
                    nodeCode.Add("d" + stop.operation.colour[0] + stop.operation.type[0]);
                    break;
                case "StopOperation":
                    string value = "";
                    switch (stop.operation.type) {
                        case "int":
                            value = stop.operation.valueI.ToString(); break;
                        case "bool":
                            value = stop.operation.valueB.ToString().ToLower(); break;
                        case "string":
                            value = stop.operation.valueS; break;
                    }
                    nodeCode.Add("o" + stop.operation.colour[0].ToString() + stop.operation.operation.ToString() + value);
                    break;
            }
        }
        return nodeCode;
    }

    private Dictionary<char, int> CreateLimitCode() {
        Dictionary<char, int> dict = new Dictionary<char, int>() { { 'i', 0 }, { 'f', 0 }, { 'u', 0 }, { 'd', 0 }, { 'o', 0 } };
        Track[] tracks = FindObjectsOfType<Track>();
        Stop[] stops = FindObjectsOfType<Stop>();
        foreach (Track track in tracks) {
            if (!track.isInitial) {
                switch (track.type) {
                    case "if":
                        dict['i']++;
                        break;
                    case "forStart":
                        dict['f']++;
                        break;
                    case "forUntilStart":
                        dict['u']++;
                        break;
                    default:
                        break;
                }
            }
        }
        foreach (Stop stop in stops) {
            switch (stop.GetType().Name) {
                case "StopAdd":
                    dict['d']++;
                    break;
                case "StopOperation":
                    dict['o']++;
                    break;
            }
        }
        return dict;
    }
}