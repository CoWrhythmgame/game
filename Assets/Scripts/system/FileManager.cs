using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class FileManager
{
    private static readonly string[] _filenames = new string[4]{"1-Easy","2-Normal","3-Hard","4-Extreme"};

    #region 실제 사용 함수
    public static List<Song> LoadSong()
    {
        Debug.Log("LoadSong");
        List<Song> songs = new List<Song>();
        string AssetPath = GetStreamingAssetsPath();
        string LocalPath = GetLocalPath();


        string[] songnames = GetSongNames(AssetPath);
        for(int i = 0; i < songnames.Length; i++)
        {
            string songJson;
            List<string> patternInfosJson;
            List<string> recordsJson;
            
            songJson = File.ReadAllText(AssetPath+"/SongInfo"+songnames[i]+"/0-Info.json");
            patternInfosJson = GetPatternInfoJson(songnames[i]);
            recordsJson = GetRecordJson(songnames[i], LocalPath);
            Debug.Log(patternInfosJson[0]);
            songs.Add(GetSongFromJson(songJson, patternInfosJson, recordsJson));
        }
        return songs;
    }
    public static Pattern LoadPattern(Song songData, int difficultyIndex)
    {
        Pattern pattern;
        string patternJson;
        patternJson = GetPatternJson(songData.patternInfo[difficultyIndex].patternPath);
        pattern = GetPatternFromJson(patternJson);
        return pattern;
    }
    #endregion

    #region 기본 파일 경로 관련
    private static string GetStreamingAssetsPath()
    {
        return Application.streamingAssetsPath;
    }
    //local 경로 확인
    private static string GetLocalPath()
    {
        //window이면 mygames에 저장, 아니면 persistentDataPath에 저장
        string folderPath = "";
        #if UNITY_STANDALONE_WIN
            folderPath =
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("\\", "/");
            folderPath += "/My Games/RythmGame/";
        #else
                folderPath = Application.persistentDataPath + "/";
        #endif
        return folderPath;
    }
    private static string[] GetSongNames( string AssetPath)
    {
        string[] songnames = Directory.GetDirectories(AssetPath+"/SongInfo")
        .Where(file => !file.EndsWith(".meta")).ToArray();
        for(int i = 0; i < songnames.Length; i++)
        {
            songnames[i] = songnames[i].Replace(AssetPath+"/SongInfo", "");
        }
        return songnames;
    }
    #endregion

    #region json to class 변환
    //json을 song으로 변환
    private static Song GetSongFromJson(string songJson, List<string> patternInfosJson, List<string> recordsJson)
    {
        Song song = JsonUtility.FromJson<Song>(songJson);
        List<PatternInfo> patternInfos = new List<PatternInfo>();
        List<Record> records = new List<Record>();
        string assetPath = GetStreamingAssetsPath();

        //json 변환
        foreach(string pattern in patternInfosJson)
        {
            PatternInfo temp = JsonUtility.FromJson<PatternInfo>(pattern);
            patternInfos.Add(temp);
        }
        foreach(string record in recordsJson)
        {
            Record temp = JsonUtility.FromJson<Record>(record);
            records.Add(temp);
        }

        //예외처리용 기록 채우기
        for(int i = 0; i < 4; i++)
        {
            records.Add(new Record()
            {
                comboResult = ComboResult.none,
                maxcombo = 0,
                prate = 0,
                score = 0
            });
        }
        for(int i = 0; i < patternInfos.Count; i++)
        {
            patternInfos[i].patternPath = assetPath + "/Pattern/" + song.songname + "/" + _filenames[i] + ".json";
        }
        song.patternInfo = patternInfos;
        song.record = records;
        return song;
    }
    private static Pattern GetPatternFromJson(string patternJson)
    {
        Pattern pattern = JsonUtility.FromJson<Pattern>(patternJson);
        return pattern;
    }
    #endregion
    
    #region json file to string
    private static List<string> GetPatternInfoJson(string songname)
    {
        List<string> patternsJson = new List<string>();
        string AssetPath = GetStreamingAssetsPath();

        string[] songpaths = Directory.GetFiles(AssetPath+"/SongInfo"+songname)
        .Where(file => file.EndsWith(".json")).ToArray();
        
        Debug.Log("patternpaths: " + songpaths[3]);
        foreach(string path in songpaths)
        {
            if(path.Contains("Info.json")) continue;
            Debug.Log(File.ReadAllText(path));
            patternsJson.Add(File.ReadAllText(path));
        }

        return patternsJson;
    }
    private static List<string> GetRecordJson(string songname, string LocalPath)
    {

        if(!File.Exists(LocalPath+"/Record"+songname)) return new List<string>();
        
        List<string> recordsJson = new List<string>();

        string[] recordpaths = Directory.GetFiles(LocalPath+"/Record"+songname)
        .Where(file => file.EndsWith(".json")).ToArray();

        foreach(string path in recordpaths)
        {
            recordsJson.Add(File.ReadAllText(path));
        }
        return recordsJson;
    }
    private static string GetPatternJson(string patternPath)
    {
        if (!File.Exists(patternPath))
        {
            Debug.LogWarning("파일 탐지 못함");
            return "";
        }
        string patternJson;

        patternJson = File.ReadAllText(patternPath);
        return patternJson;
    }
    #endregion
    #region 테스트용 함수
    /// <summary>
    /// 패턴파일 경로는 pattern/TestSong/1-Easy.json 입니다.
    /// </summary>
    public static Pattern TestPatternLoad()
    {
        Pattern pattern;
        pattern = GetPatternFromJson(GetPatternJson(GetStreamingAssetsPath() + "/Pattern/TestSong/" + _filenames[0] + ".json"));
        return pattern;
    }
    #endregion
    // 추가한 코드 
    public static Record UpdateRecord(string songName, int patternIndex, PlayData playData)
    {
        Record newRecord = ConvertPlayDataToRecord(playData);
        Record oldRecord = LoadRecord(songName, patternIndex);
        
        if (IsBetterRecord(newRecord, oldRecord))
        {
            SaveRecord(songName, patternIndex, newRecord);
            return newRecord;
        }

        return oldRecord;
    }
    
    public static Record LoadRecord(string songName, int patternIndex)
    {
        string filePath = GetRecordFilePath(songName, patternIndex);

        if (!File.Exists(filePath))
        {
            return new Record();
        }

        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<Record>(json);
    }

    public static void SaveRecord(string songName, int patternIndex, Record record)
    {
        string directoryPath = GetRecordDirectoryPath(songName);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string filePath = GetRecordFilePath(songName, patternIndex);
        string json = JsonUtility.ToJson(record, true);

        File.WriteAllText(filePath, json);

        Debug.Log("Record saved: " + filePath);
    }

    private static Record ConvertPlayDataToRecord(PlayData playData)
    {
        if (playData == null)
        {
            return new Record();
        }

        return new Record
        {
            score = playData.score,
            maxcombo = playData.maxcombo,
            prate = playData.prate,
            comboResult = CalculateComboResult(playData)
        };
    }

    private static ComboResult CalculateComboResult(PlayData playData)
    {
        if (playData == null || playData.noteCount == null || playData.noteCount.Length < 4)
        {
            return ComboResult.none;
        }

        int great = playData.noteCount[1];
        int good = playData.noteCount[2];
        int miss = playData.noteCount[3];

        if (miss == 0 && great == 0 && good == 0)
        {
            return ComboResult.allperfact;
        }

        if (miss == 0)
        {
            return ComboResult.fullcombo;
        }

        return ComboResult.none;
    }

    private static bool IsBetterRecord(Record newRecord, Record oldRecord)
    {
        if (oldRecord == null)
        {
            return true;
        }

        if (newRecord.score > oldRecord.score)
        {
            return true;
        }

        if (Mathf.Approximately(newRecord.score, oldRecord.score) &&
            newRecord.prate > oldRecord.prate)
        {
            return true;
        }

        if (Mathf.Approximately(newRecord.score, oldRecord.score) &&
            Mathf.Approximately(newRecord.prate, oldRecord.prate) &&
            newRecord.maxcombo > oldRecord.maxcombo)
        {
            return true;
        }

        if (newRecord.comboResult > oldRecord.comboResult)
        {
            return true;
        }

        return false;
    }

    private static string GetRecordDirectoryPath(string songName)
    {
        string localPath = GetLocalPath();
        string safeSongName = SanitizeFileName(songName);

        return Path.Combine(localPath, "Record", safeSongName);
    }

    private static string GetRecordFilePath(string songName, int patternIndex)
    {
        string directoryPath = GetRecordDirectoryPath(songName);
        string fileName = GetRecordFileName(patternIndex);

        return Path.Combine(directoryPath, fileName);
    }

    private static string GetRecordFileName(int patternIndex)
    {
        switch (patternIndex)
        {
            case 0:
                return "1-Easy.json";
            case 1:
                return "2-Normal.json";
            case 2:
                return "3-Hard.json";
            case 3:
                return "4-Extreme.json";
            default:
                return (patternIndex + 1) + "-Unknown.json";
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return "UnknownSong";
        }

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidChar, '_');
        }

        return fileName;
    }
}
