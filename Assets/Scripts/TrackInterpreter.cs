using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackInterpreter : MonoBehaviour
{
    private class TypeErrorData {
        public bool noError;
        public string errorString;
    }

    public TrainVariables variables = new TrainVariables();
    public List<string> activeVars = new List<string>(); // red || green || blue
    public Track initialTrack;
    public TrainVariableDisplay varDisplay;
    private int runId = 0;
    private Dictionary<string, int> forRepeats = new Dictionary<string, int>();
    private TrainLevelController controller;

    private void Start() {
        controller = GameObject.Find("Controller").GetComponent<TrainLevelController>();
    }

    public void Interpret() {
        Clear();
        InterpretNode(initialTrack);
    }

    private void InterpretNode(Track track, bool jumping = false, bool joining = false) {
        float time;
        switch (track.type) {
            case "straight":
                time = track.PlayAnimation();
                if (track.stop != null) {
                    //Stop
                    TrackOperation opStop = track.stop.operation;
                    if (track.stop.GetType().Name == "StopAdd") {
                        if (activeVars.Contains(opStop.colour)) {
                            Failure("'" + opStop.colour + "' variable already declared, cannot be declared again.");
                        } else {
                            //Add variable
                            activeVars.Add(opStop.colour);
                            variables.AddVariable(opStop.colour, opStop.type);
                            varDisplay.RefreshAfter(time / 2);
                            if (track.GetNextTrack()[0] == null) {
                                StartCoroutine(WaitForSecs(time, track, runId, track, last: true));
                            } else {
                                StartCoroutine(WaitForSecs(time, track.GetNextTrack()[0], runId, track));
                            }
                        }
                    } else if (track.stop.GetType().Name == "StopOperation") {
                        if (activeVars.Contains(opStop.colour) && (variables.GetInitialised(opStop.colour) || opStop.operation == '=')) {
                            if (TypeCheck(opStop).noError) {
                                //Run operation
                                switch (opStop.type) {
                                    case "int":
                                        switch (opStop.operation) {
                                            case '+':
                                                variables.SetValue(opStop.colour, valueI: (opStop.valueI + int.Parse(variables.GetValue(opStop.colour))));
                                                break;
                                            case '-':
                                                variables.SetValue(opStop.colour, valueI: (opStop.valueI - int.Parse(variables.GetValue(opStop.colour))));
                                                break;
                                            case '*':
                                                variables.SetValue(opStop.colour, valueI: (opStop.valueI * int.Parse(variables.GetValue(opStop.colour))));
                                                break;
                                            case '=':
                                                variables.SetValue(opStop.colour, valueI: (opStop.valueI));
                                                break;
                                        }
                                        break;
                                    case "bool":
                                        variables.SetValue(opStop.colour, valueB: (opStop.valueB));
                                        break;
                                    case "string":
                                        switch (opStop.operation) {
                                            case '+':
                                                variables.SetValue(opStop.colour, valueS: (opStop.valueS + variables.GetValue(opStop.colour)));
                                                break;
                                            case '=':
                                                variables.SetValue(opStop.colour, valueS: (opStop.valueS));
                                                break;
                                        }
                                        break;
                                }
                                varDisplay.RefreshAfter(time / 2);
                                if (track.GetNextTrack()[0] == null) {
                                    StartCoroutine(WaitForSecs(time, track, runId, track, last: true));
                                } else {
                                    StartCoroutine(WaitForSecs(time, track.GetNextTrack()[0], runId, track));
                                }
                            } else {
                                Failure(TypeCheck(opStop).errorString);
                            }
                        } else {
                            Failure("'" + opStop.colour + "' variable not declared or initialised, it cannot be used before it is created.");
                        }
                    }
                } else {
                    if (track.GetNextTrack()[0] == null) {
                        StartCoroutine(WaitForSecs(time, track, runId, track, last: true));
                    } else {
                        StartCoroutine(WaitForSecs(time, track.GetNextTrack()[0], runId, track));
                    }
                }
                break;
            case "curve":
                time = track.PlayAnimation();
                if (track.GetNextTrack()[0] == null) {
                    StartCoroutine(WaitForSecs(time, track, runId, track, last: true));
                } else {
                    StartCoroutine(WaitForSecs(time, track.GetNextTrack()[0], runId, track));
                }
                break;
            case "if":
                {
                    TrackOperation op = track.forBranchOp;
                    if (TypeCheck(op).noError) {
                        (dynamic value, dynamic value2) = GetComparisonValues(op);
                        if (RunComparison(value, value2, op.compOperation)) {
                            time = track.PlayAnimation(branch: false);
                            if (track.GetNextTrack()[0] == null) {
                                StartCoroutine(WaitForSecs(time, track, runId, track, last: true));
                            } else {
                                StartCoroutine(WaitForSecs(time, track.GetNextTrack()[0], runId, track));
                            }
                        } else {
                            time = track.PlayAnimation(branch: true);
                            if (track.GetNextTrack()[1] == null) {
                                StartCoroutine(WaitForSecs(time, track, runId, track, last: true));
                            } else {
                                StartCoroutine(WaitForSecs(time, track.GetNextTrack()[1], runId, track));
                            }
                        }
                    } else {
                        Failure(TypeCheck(op).errorString);
                    }
                    break;
                }
            case "ifJoin": {
                    if (joining) {
                        time = track.PlayAnimation(branch: true);
                    } else {
                        time = track.PlayAnimation(branch: false);
                    }
                    if (track.GetNextTrack()[0] == null) {
                        StartCoroutine(WaitForSecs(time, track, runId, track, last: true));
                    } else {
                        StartCoroutine(WaitForSecs(time, track.GetNextTrack()[0], runId, track));
                    }
                    break;
                }
            case "forStart":
            case "forUntilStart":
                if (jumping) {
                    time = track.PlayAnimation(forState: "curve");
                    StartCoroutine(WaitForSecs(time, track.GetNextTrack()[0], runId, track));
                } else {
                    time = track.PlayAnimation(forState: "straight");
                    if (track.GetNextTrack()[0] == null) {
                        StartCoroutine(WaitForSecs(time, track, runId, track, last: true));
                    } else {
                        StartCoroutine(WaitForSecs(time, track.GetNextTrack()[0], runId, track));
                    }
                }
                break;
            case "forEnd":
                if (!forRepeats.ContainsKey(track.name)) {
                    forRepeats.Add(track.name, track.forStart.forRepeats);
                }
                if (forRepeats[track.name] <= 0) {
                    forRepeats.Remove(track.name);
                    //Don't repeat
                    time = track.PlayAnimation(forState: "straight");
                    if (track.GetNextTrack()[0] == null) {
                        StartCoroutine(WaitForSecs(time, track, runId, track, last: true));
                    } else {
                        StartCoroutine(WaitForSecs(time, track.GetNextTrack()[0], runId, track));
                    }
                } else {
                    forRepeats[track.name] -= 1;
                    //Repeat
                    time = track.PlayAnimation(forState: "curve");
                    StartCoroutine(WaitForSecs(time, track.forStart, runId, track, jumping: true));
                }
                break;
            case "forUntilEnd": {
                    TrackOperation op = track.forStart.forBranchOp;
                    if (TypeCheck(op).noError) {
                        (dynamic value, dynamic value2) = GetComparisonValues(op);
                        if (RunComparison(value, value2, op.compOperation)) {
                            //Don't repeat
                            time = track.PlayAnimation(forState: "straight");
                            if (track.GetNextTrack()[0] == null) {
                                StartCoroutine(WaitForSecs(time, track, runId, track, last: true));
                            } else {
                                StartCoroutine(WaitForSecs(time, track.GetNextTrack()[0], runId, track));
                            }
                        } else {
                            //Repeat
                            time = track.PlayAnimation(forState: "curve");
                            StartCoroutine(WaitForSecs(time, track.forStart, runId, track, jumping: true));
                        }
                    } else {
                        Failure(TypeCheck(op).errorString);
                    }
                    break;
                }
        }
    }

    private (dynamic, dynamic) GetComparisonValues(TrackOperation op) {
        dynamic value = null;
        dynamic value2 = null;
        switch (op.type) {
            case "int":
                value = op.valueI;
                value2 = int.Parse(variables.GetValue(op.colour));
                break;
            case "bool":
                value = op.valueB;
                value2 = bool.Parse(variables.GetValue(op.colour));
                break;
            case "string":
                value = op.valueS;
                value2 = variables.GetValue(op.colour);
                break;
        }
        return (value2, value);
    }

    private IEnumerator WaitForSecs(float time, Track nextTrack, int id, Track currentTrack, bool last = false, bool jumping = false) {
        yield return new WaitForSeconds(time);
        if (id == runId) {
            if (last) {
                Successful();
            } else {
                if (jumping) {
                    InterpretNode(nextTrack, jumping: true);
                } else {
                    if (nextTrack.type == "ifJoin") {
                        int x = Mathf.RoundToInt(currentTrack.transform.position.x);
                        int z = -Mathf.RoundToInt(currentTrack.transform.position.z);
                        int x2 = Mathf.RoundToInt(nextTrack.secondPos.position.x);
                        int z2 = -Mathf.RoundToInt(nextTrack.secondPos.position.z);
                        if (x == x2 || z == z2) {
                            InterpretNode(nextTrack, joining: true);
                        } else {
                            InterpretNode(nextTrack, joining: false);
                        }
                    } else {
                        InterpretNode(nextTrack);
                    }
                }
            }
        }
    }

    private void Successful() {
        //Send to controller
        controller.InterpreterSuccesful(variables);
    }

    private void Failure(string errorString) {
        //Send to error script and reset
        Clear();
        print(errorString);
        controller.ShowError(errorString);
    }

    public void Clear() {
        runId += 1;
        if (runId > 100) {
            runId = 0;
        }
        activeVars.Clear();
        variables = new TrainVariables();
        varDisplay.Refresh();
        forRepeats.Clear();
        foreach (Track track in FindObjectsOfType<Track>()) {
            track.gameObject.GetComponent<Animation>().Stop();
        }
        foreach (GameObject target in GameObject.FindGameObjectsWithTag("TrainTarget")) {
            target.SetActive(false);
        }
        FindObjectOfType<TrainMove>().ResetPos();
    }

    private TypeErrorData TypeCheck(TrackOperation op) {
        TypeErrorData data = new TypeErrorData();
        if (op.compOperation != '\0') {
            //ifUI, forUntilUI
            switch (op.type) {
                case "int":
                    goto variablesCheck;
                case "bool":
                    if (op.compOperation == '=') {
                        goto variablesCheck;
                    } else {
                        data.noError = false;
                        data.errorString = "Comparative operator '" + op.compOperation + "' is not compatible with type 'bool', only '=' is compatible.";
                        return data;
                    }
                case "string":
                    if (op.compOperation == '=') {
                        goto variablesCheck;
                    } else {
                        data.noError = false;
                        data.errorString = "Comparative operator '" + op.compOperation + "' is not compatible with type 'string', only '=' is compatible.";
                        return data;
                    }
            }
        } else {
            //stopOperationUI
            switch (op.type) {
                case "int":
                    goto variablesCheck;
                case "bool":
                    if (op.operation == '=') {
                        goto variablesCheck;
                    } else {
                        data.noError = false;
                        data.errorString = "Operator '" + op.operation + "' is not compatible with type 'bool', only '=' is compatible.";
                        return data;
                    }
                case "string":
                    if (op.operation == '=' || op.operation == '+') {
                        goto variablesCheck;
                    } else {
                        data.noError = false;
                        data.errorString = "Operator '" + op.operation + "' is not compatible with type 'string', only '=' and '+' are compatible.";
                        return data;
                    }
            }
        }
        data.noError = true;
        return data;
    variablesCheck:
        if (activeVars.Contains(op.colour) && (variables.GetInitialised(op.colour) || op.operation == '=')) {
            if (variables.GetType(op.colour) == op.type) {
                data.noError = true;
                return data;
            } else {
                data.noError = false;
                data.errorString = "'" + op.colour + "' variable of type '" + variables.GetType(op.colour) + "' does not match provided type: '" + op.type + "'.";
                return data;
            }
        } else {
            data.noError = false;
            data.errorString = "'" + op.colour + "' variable has not been initialised, aka no value has been set so it cannot be used.";
            return data;
        }
    }

    private bool RunComparison(int i1, int i2, char compOp) {
        switch (compOp) {
            case '=':
                if (i1 == i2) {
                    return true;
                } else {
                    return false;
                }
            case '>':
                if (i1 > i2) {
                    return true;
                } else {
                    return false;
                }
            case '<':
                if (i1 < i2) {
                    return true;
                } else {
                    return false;
                }
        }
        return false;
    }

    private bool RunComparison(bool b1, bool b2, char compOp) {
        if (b1 == b2) {
            return true;
        } else {
            return false;
        }
    }

    private bool RunComparison(string s1, string s2, char compOp) {
        if (s1 == s2) {
            return true;
        } else {
            return false;
        }
    }
}