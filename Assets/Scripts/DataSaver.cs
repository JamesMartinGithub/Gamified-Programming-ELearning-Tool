using System.IO;
using System.Text;
using UnityEngine;

public class DataSaver : MonoBehaviour
{
    public class Data {
        public string name = "null";
        public string iconCode = "12|4|5|0";
        public int trainDone = 0;
        public int codeDone = 0;
    }
    public Data data = new Data();
    private string path = "Data.txt";

    public void LoadData() {
        try {
            StreamReader sw = new StreamReader(path, Encoding.UTF8);
            data.name = sw.ReadLine();
            data.iconCode = sw.ReadLine();
            data.trainDone = int.Parse(sw.ReadLine());
            data.codeDone = int.Parse(sw.ReadLine());
            sw.Close();
        }
        catch (System.Exception) {
            //Create new savefile
            SaveData(data);
        }
    }

    private void SaveData(Data data) {
        this.data = data;
        StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8);
        sw.WriteLine(data.name);
        sw.WriteLine(data.iconCode);
        sw.WriteLine(data.trainDone.ToString());
        sw.WriteLine(data.codeDone.ToString());
        sw.Close();
    }
    public void SaveName(string name) {
        data.name = name;
        SaveData(data);
    }

    public int[] GetIconCode() {
        string[] split = data.iconCode.Split('|');
        int[] code = new int[4];
        for (int i = 0; i < 4; i++) {
            code[i] = int.Parse(split[i]);
        }
        return code;
    }
    public void SetIconCode(int[] code) {
        data.iconCode = code[0] + "|" + code[1] + "|" + code[2] + "|" + code[3];
        SaveData(data);
    }

    public void CompletedLevel(int id, bool isTrain) {
        if (isTrain) {
            if (id >= data.trainDone) {
                data.trainDone++;
                SaveData(data);
            }
        } else {
            if (id >= data.codeDone) {
                data.codeDone++;
                SaveData(data);
            }
        }
    }
}