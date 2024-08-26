using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;
using System.Linq;
using SFB;

public class MenuController : MonoBehaviour
{
    public GameObject levelSelectPanel;
    public GameObject profilePanel;
    public Transform profileIcon;
    public Transform[] iconPos;
    public GameObject exitCross;
    public ProfileIconBuilder profileBuilder;
    public TextMeshProUGUI nameText;
    public TMP_InputField nameInputText;
    public TextMeshProUGUI trainCountText;
    public TextMeshProUGUI codeCountText;
    public Image radialProgress;
    public GameObject[] blockers;
    public GameObject resourceButton;
    public GameObject resources;

    private void Start() {
        levelSelectPanel.SetActive(false);
        profilePanel.SetActive(false);
        profileIcon.position = iconPos[0].position;
        exitCross.SetActive(true);
        //Load profile
        DataSaver data = GameObject.Find("Level Information").GetComponent<DataSaver>();
        data.LoadData();
        profileBuilder.SetIcon(data.GetIconCode());
        if (data.data.name == "null") {
            nameText.text = "";
            nameInputText.text = "";
        } else {
            nameText.text = data.data.name;
            nameInputText.text = data.data.name;
        }
        trainCountText.text = data.data.trainDone.ToString();
        codeCountText.text = data.data.codeDone.ToString();
        foreach (GameObject blocker in blockers) {
            blocker.SetActive(true);
        }
        for (int i = 0; i < (Mathf.Min(data.data.trainDone, 3)); i++) {
            blockers[i].SetActive(false);
        }
        for (int i = 3; i < (Mathf.Min(data.data.codeDone + 3, 6)); i++) {
            blockers[i].SetActive(false);
        }
        radialProgress.fillAmount = (data.data.trainDone + data.data.codeDone) / 8.0f;
    }

    public void LoadLevel(LevelInfo.Level level) {
        GameObject.Find("Level Information").GetComponent<LevelInfo>().level = level;
        string scene;
        if (level.isTrain) {
            scene = "TrainLevel";
        } else {
            scene = "PseudocodeLevel";
        }
        SceneManager.LoadScene(scene);
    }

    public void LoadCustom() {
        var extensions = new[] {
            new ExtensionFilter("Level Files", "lvl" ),
        };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
        if (paths.Length != 0) {
            (bool valid, LevelInfo.Level level) = ParseCustom(paths[0]);
            if (valid) {
                LoadLevel(level);
            }
        }
    }

    private (bool valid, LevelInfo.Level level) ParseCustom(string path) {
        LevelInfo.Level level = new LevelInfo.Level();
        try {
            StreamReader sw = new StreamReader(path, Encoding.UTF8);
            level.isSandbox = false;
            level.isTrain = false;
            level.showTutorial = false;
            level.task = sw.ReadLine();
            level.taskcode = sw.ReadLine().Split('&').ToList();
            level.constraints = sw.ReadLine();
            level.musthavecode = sw.ReadLine();
            level.limitscode = sw.ReadLine();
            level.hint = sw.ReadLine();
            sw.Close();
            return (true, level);
        }
        catch (System.Exception) {
            //Custom level file not valid
            return (false, null);
        }
    }

    public void Profile() {
        profilePanel.SetActive(true);
        profileIcon.position = iconPos[1].position;
        exitCross.SetActive(false);
        nameText.gameObject.SetActive(false);
        resourceButton.SetActive(false);
        resources.SetActive(false);
    }
    public void ProfileBack() {
        profilePanel.SetActive(false);
        profileIcon.position = iconPos[0].position;
        exitCross.SetActive(true);
        nameText.gameObject.SetActive(true);
        resourceButton.SetActive(true);
    }

    public void LevelSelect() {
        levelSelectPanel.SetActive(true);
        profileIcon.gameObject.SetActive(false);
        resourceButton.SetActive(false);
        resources.SetActive(false);
    }
    public void LevelSelectBack() {
        levelSelectPanel.SetActive(false);
        profileIcon.gameObject.SetActive(true);
        resourceButton.SetActive(true);
    }

    public void CodeSandbox() {
        LevelInfo.Level level = new LevelInfo.Level();
        level.isSandbox = true;
        level.isTrain = false;
        LoadLevel(level);
    }

    public void SetName() {
        nameText.text = nameInputText.text;
        GameObject.Find("Level Information").GetComponent<DataSaver>().SaveName(nameInputText.text);
    }

    public void LearningResources() {
        if (resources.activeSelf) {
            resources.SetActive(false);
        } else {
            resources.SetActive(true);
        }
    }

    public void Link1() {
        Application.OpenURL("https://www.khanacademy.org/computing/ap-computer-science-principles/programming-101");
    }
    public void Link2() {
        Application.OpenURL("https://www.geeksforgeeks.org/programming/?ref=lbp");
    }
    public void Link3() {
        Application.OpenURL("https://www.edx.org/learn/python/the-university-of-michigan-programming-for-everybody-getting-started-with-python?index=product&objectID=course-911175d0-6724-4276-a058-c7b052773dd1&webview=false&campaign=Programming+for+Everybody+%28Getting+Started+with+Python%29&source=edX&product_category=course&placement_url=https%3A%2F%2Fwww.edx.org%2Flearn%2Fcomputer-programming");
    }

    public void Exit() {
        Application.Quit();
    }
}