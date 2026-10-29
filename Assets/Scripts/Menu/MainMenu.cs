using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] List<TextMeshProUGUI> _names;
    [SerializeField] List<TextMeshProUGUI> _times;


    string _savePath;
    string _legacySavePath;

    // Player score class
    [System.Serializable]
    public class SavePlayer
    {
        public string playerName;
        public PlayTime playerTime;
    }

    // leaderboard class
    [System.Serializable]
    public class LeaderboardSaveObject
    {
        public List<SavePlayer> playerList;
    }

    // time class use to convert sting -> class or class -> string
    [System.Serializable]
    public class PlayTime
    {
        public int minute;
        public int seconde;
    }

    // the leaderboard variable
    LeaderboardSaveObject _leaderboard;


    // init all variable
    void Awake()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "leaderboard.sav");
        _legacySavePath = Path.Combine(Application.dataPath, "Saves", "leaderboard.sav");
    }

    void Start()
    {
        InitUI();
        LoadData();
        UpdateLeaderboardDisplay();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


    public void StartGame()
    {
        TimerGame.StartTimer();
        SceneManager.LoadScene("RoomGeneration");
    }



    void InitUI()
    {
        foreach (TextMeshProUGUI name in _names) name.text = "";
        foreach (TextMeshProUGUI time in _times) time.text = "";
    }


    void UpdateLeaderboardDisplay()
    {
        int i = 0;
        foreach ( SavePlayer cur in _leaderboard.playerList )
        {
            if ( i >= _names.Count ) break;

            _names[i].text = cur.playerName;
            _times[i].text = TimeToString(cur.playerTime);

            i++;
        }
    }


    void SaveData()
    {
        string json = ImportToJson(_leaderboard);

        Debug.Log($"Save Data ({_savePath}):\n{json}");

        Directory.CreateDirectory(Path.GetDirectoryName(_savePath));
        
        File.WriteAllText(_savePath, json);
    }


    void LoadData()
    {
        string loadPath = File.Exists(_savePath) ? _savePath : _legacySavePath;

        if (!File.Exists(loadPath))
        {
            CreateEmptyLeaderboard();
            return;
        }

        string json = File.ReadAllText(loadPath);

        if (string.IsNullOrWhiteSpace(json))
        {
            CreateEmptyLeaderboard();
            return;
        }

        _leaderboard = ExctractFromJson(json);

        if (_leaderboard == null || _leaderboard.playerList == null)
        {
            CreateEmptyLeaderboard();
            return;
        }

        if (loadPath != _savePath)
            SaveData();
    }

    void CreateEmptyLeaderboard()
    {
        _leaderboard = new LeaderboardSaveObject()
        {
            playerList = new List<SavePlayer>()
        };
    }

    // return json text form leaderboard object
    string ImportToJson(LeaderboardSaveObject save)
    {
        return JsonUtility.ToJson(save);
    }

    // return leaderboard object form json text
    LeaderboardSaveObject ExctractFromJson(string json)
    {
        return JsonUtility.FromJson<LeaderboardSaveObject>(json);
    }


    // [ContextMenu("test")]
    // void test()
    // {
    //     Debug.Log(TimeToString(new PlayTime(){ minute = 14, seconde = 46 }));
    // }


    // convert time (mm:ss) into playtime object
    PlayTime stringToTime(string t)
    {
        string mString = "";
        string sString = "";

        int i;

        for (i = 0; i < t.Length; i++)
        {
            if ( t[i] == ':' )
                break;
            mString = mString + t[i];
        }

        for (i = i + 1; i < t.Length; i++)
        {
            sString = sString + t[i];
        }

        int m = int.Parse(mString);
        int s = int.Parse(sString);

        PlayTime obj = new PlayTime(){ minute = m, seconde = s};
        return obj;
    }

    // convert playtime object into string (mm:ss)
    string TimeToString(PlayTime t)
    {
        string m = "";
        string s = "";

        if ( t.minute < 10 )
            m = "0";
        
        if ( t.seconde < 10 )
            s = "0";

        return $"{m}{t.minute}:{s}{t.seconde}";
    }
}
