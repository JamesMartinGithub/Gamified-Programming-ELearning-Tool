using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;
using System;

public class PseudocodeLevelController : MonoBehaviour
{
    public PseudocodeInterpreter interpreter;
    public InfoUI infoUI;
    public TMP_InputField codeInput;
    public TextMeshProUGUI errorUnderlineText;
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
    private string mustHaveCode = "";
    private string limitsCode = "";

    private void Start() {
        //Setup task
        LevelInfo.Level level = GameObject.Find("Level Information").GetComponent<LevelInfo>().level;
        if (level.isSandbox) {
            task.text = "None";
            constraint.text = "None";
            hint.text = "No hint available in sandbox mode";
            id = -1;
        } else {
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
            if (mustHaveCode != "") {
                foreach (string code in mustHaveCode.Split('&')) {
                    if (code[0] == '$') {
                        codeInput.text = code.Substring(1);
                    }
                }
            }
        }
        victoryPanel.SetActive(false);
    }

    public void Play() {
        interpreter.Interpret(codeInput.text);
    }

    public void ShowOutput(List<string> messages) {
        //foreach (var message in messages) { print(message); }
        infoUI.ResetError();
        infoUI.ShowOutput(messages);
        errorUnderlineText.text = "";
        if (id != -1) {
            //Compare variable list to task list
            bool matches = true;
            if (!taskList.SequenceEqual(messages)) {
                matches = false;
            }
            string[] toRemove = new string[3] { "(", ")", " " };
            string cleanedCode = codeInput.text;
            foreach (string s in toRemove) {
                cleanedCode = cleanedCode.Replace(s, "");
            }
            //Compare code to musthavelist
            if (mustHaveCode != "") {
                foreach (string code in mustHaveCode.Replace(" ", "").Split('&')) {
                    if (code[0] == '$') {
                        if (!cleanedCode.StartsWith(code.Substring(1))) {
                            matches = false;
                        }
                    } else {
                        if (!cleanedCode.Contains(code)) {
                            matches = false;
                        }
                    }
                }
            }
            //Compare code to limitlist
            if (limitsCode != "") {
                foreach (string code in limitsCode.Split('&')) {
                    string cutcode = code.Substring(0, code.Length - 1);
                    var match = from word in cleanedCode
                                     where word.Equals(cutcode)
                                     select word;
                    if (match.Count() > int.Parse(code[code.Length - 1].ToString())) {
                        matches = false;
                    }
                }
            }
            if (matches) {
                //Task complete
                //Play sound
                victorySound.Play();
                //Show victory screen
                victoryPanel.SetActive(true);
                //Save data
                if (id != -1) {
                    GameObject.Find("Level Information").GetComponent<DataSaver>().CompletedLevel(id, false);
                }
            }
        }
    }

    public void ShowError(List<PseudocodeParser.Error> errors) {
        //Play sound
        errorSound.Play();
        //Fill underline text with spaces to be replaced
        char[] chars = new char[codeInput.text.Length];
        for (int i = 0; i < codeInput.text.Length; i++) {
            if (codeInput.text[i] == '\t') {
                chars[i] = '\t';
            } else if (codeInput.text[i] == '\n') {
                chars[i] = '\n';
            } else {
                chars[i] = ' ';
            }
        }
        void ErrorUnderline(PseudocodeParser.Error error) {
            for (int i = 0; i < codeInput.text.Length; i++) {
                if (i >= error.startPosition.Index && i <= error.endPosition.Index) {
                    chars[i] = '_';
                }
            }
        }
        errorUnderlineText.text = "";
        List<string> errorStrings = new List<string>();
        //foreach (var error in errors) { print(error.errorText); }
        foreach (var error in errors) {
            if (error.startPosition == null || error.endPosition == null) {
                errorStrings.Add(error.errorText);
            } else {
                errorStrings.Add(error.errorText + " (" + error.startPosition.Line + "," + error.startPosition.Column + ")");
                ErrorUnderline(error);
            }
        }
        infoUI.ShowError(errorStrings);
        errorUnderlineText.text = new string(chars);
    }

    public void Exit() {
        SceneManager.LoadScene("Menu");
    }
}