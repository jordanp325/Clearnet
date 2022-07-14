using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveData{ //TODO: add encryption so save files can't be tampered with
    public static SaveData Instance{
        get{
            return instance;
        }
        set{
            instance = value;
            Save();
        }
    }
    private static SaveData instance = new SaveData();
    public const string FileName = "SaveData.save";
    public static void Save(){
        string data = JsonUtility.ToJson(Instance);
        // File.WriteAllText(Application.dataPath + "/Resources/" + FileName, data);
        File.WriteAllText(FileName, data);
    }
    public static void Load(){
        string data;
        try{
            // data = File.ReadAllText(Application.dataPath + "/Resources/" + FileName);
            data = File.ReadAllText(FileName);
        } catch(FileNotFoundException e){
            Debug.Log(e);
            return;
        }
        if(data != "") Instance = JsonUtility.FromJson<SaveData>(data);
    }


    //Gamestage key:
    //  0 - plays the startup screen, then immediately switches to gamestage 1
    //  1 - looking around on the DOS for files to run
    //  2 - after re-running the AI
    //  3 - after killing the AI (same as gamestage 0 but with admin)
    //  4 - after running Repair_OS, which fixes the GUI of the OS
    //  ? - and more to come...
    public int GameStage{
        get{
            return gameStage;
        }
        set{
            gameStage = value;
            if(this == Instance) Save();
        }
    }
    [SerializeField]
    int gameStage = 0;
    public string[] RemovedFiles{
        get{
            return removedFiles;
        }
        set{
            removedFiles = value;
            if(this == Instance) Save();
        }
    }
    [SerializeField]
    string[] removedFiles = new string[0];
    public bool CompletedTutorial{
        get{
            return completedTutorial;
        }
        set{
            completedTutorial = value;
            if(this == Instance) Save();
        }
    }
    [SerializeField]
    bool completedTutorial;
    
    public void AddRemovedFiles(params string[] files){
        List<string> rf = new List<string>(RemovedFiles);
        rf.AddRange(files);
        RemovedFiles = rf.ToArray();
    }
    public bool ContainsRemovedFile(string file){
        return new List<string>(removedFiles).FindIndex(a => a == file) != -1;
    }
}