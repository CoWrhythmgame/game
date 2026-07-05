using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class FileManager
{
    public static string GetStreamingAssetsPath()
    {
        return Application.streamingAssetsPath;
    }
    //local 경로 확인
    public static string GetLocalPath()
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
    //json을 song으로 변환
    public static Song GetSongFromJson(string songJson, List<string> patternInfosJson, List<string> recordsJson)
    {
        Song song = JsonUtility.FromJson<Song>(songJson);
        List<Pattern> patterns = new List<Pattern>();
        List<Record> records = new List<Record>();
        foreach(string pattern in patternInfosJson)
        {
            Pattern temp = JsonUtility.FromJson<Pattern>(pattern);
            patterns.Add(temp);
        }
        foreach(string record in recordsJson)
        {
            Record temp = JsonUtility.FromJson<Record>(record);
            records.Add(temp);
        }
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
        song.pattern = patterns;
        song.record = records;
        return song;
    }
    public static List<string> GetPatternInfoJson(string songname)
    {
        List<string> patternsJson = new List<string>();
        string AssetPath = GetStreamingAssetsPath();

        string[] patternpaths = Directory.GetFiles(AssetPath+"/SongInfo"+songname)
        .Where(file => file.EndsWith(".json")).ToArray();
        Debug.Log("patternpaths: " + patternpaths[3]);
        foreach(string path in patternpaths)
        {
            if(path.Contains("Info.json")) continue;
            Debug.Log(File.ReadAllText(path));
            patternsJson.Add(File.ReadAllText(path));
        }
        return patternsJson;
    }
    public static List<string> GetRecordJson(string songname, string LocalPath)
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
    public static List<Song> LoadSong()
    {
        Debug.Log("LoadSong");
        List<Song> songs = new List<Song>();
        string AssetPath = GetStreamingAssetsPath();
        string LocalPath = GetLocalPath();

        string[] songnames = Directory.GetDirectories(AssetPath+"/SongInfo")
        .Where(file => !file.EndsWith(".meta")).ToArray();
        for(int i = 0; i < songnames.Length; i++)
        {
            songnames[i] = songnames[i].Replace(AssetPath+"/SongInfo", "");
        }
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
}
