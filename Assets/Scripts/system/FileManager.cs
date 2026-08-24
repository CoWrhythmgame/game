using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

// * 곡 정보 종류가 추가되면 반드시 enum에도 추가할것
public enum SongDataType
{
    SongInfo,
    PatternInfo,
    Pattern,
    Record,
    song,
    jaket
}

public class FileManager
{
    private static readonly string[] _filenames = new string[4]{"1-Easy","2-Normal","3-Hard","4-Extreme"};
    private static readonly string _info = "0-Info";

    #region 외부 접근 함수
    public static List<Song> LoadSong(bool isBuiltin)
    {
        Debug.Log("LoadSong");
        List<Song> songs = new List<Song>();
        string Path = isBuiltin ? GetStreamingAssetsPath() : GetLocalPath();


        string[] songnames = GetSongNames(isBuiltin);
        for(int i = 0; i < songnames.Length; i++)
        {
            string songJson;
            List<string> patternInfosJson;
            List<string> recordsJson;
            
            songJson = GetDataJson(SongDataType.SongInfo, songnames[i], isBuiltin)[0];
            patternInfosJson = GetDataJson(SongDataType.PatternInfo, songnames[i], isBuiltin);
            recordsJson = GetDataJson(SongDataType.Record, songnames[i], isBuiltin);
            songs.Add(GetSongFromString(songJson, patternInfosJson, recordsJson));
        }
        return songs;
    }
    public static Pattern LoadPattern(Song songData, int difficultyIndex, bool isBuiltin)
    {
        Pattern pattern;
        string patternJson;
        Debug.Log($"Load Pattern - difficultyIndex:{difficultyIndex}");
        patternJson = GetDataJson(SongDataType.Pattern, songData.songname, isBuiltin)[difficultyIndex];
        pattern = GetClassFromString<Pattern>(patternJson);
        return pattern;
    }
    public static void Editor_SavePattern(Song songInfo , PatternInfo patternInfo, Pattern pattern, int patternIndex)
    {
        string songname = songInfo.songname;
        SetFileFromJson(SongDataType.SongInfo, songname, GetStringFromClass(songInfo), false);
        SetFileFromJson(SongDataType.PatternInfo, songname, GetStringFromClass(patternInfo), false, patternIndex);
        SetFileFromJson(SongDataType.Pattern, songname, GetStringFromClass(pattern), false, patternIndex);
    }
    /// <summary>
    /// 곡 파일을 로드합니다. 곡 파일은 Song/{song.songname}에 있어야 합니다.
    /// </summary>
    /// <param name="song">곡 정보</param>
    /// <param name="isBuiltin">내장 곡인지 여부</param>
    /// <param name="externalToken">외부 토큰</param>
    /// <returns>곡 audioclip</returns>
    /// <exception cref="FileNotFoundException">곡을 찾지 못함</exception>
    /// <exception cref="Exception">기타 예외</exception>
    public static async Awaitable<AudioClip> LoadMusic(Song song, bool isBuiltin, CancellationToken? externalToken = null)
    {
        string filePath;
        string dir;

        dir = GetDirPathByType(SongDataType.song, song.songname, isBuiltin);
        filePath = Directory.GetFiles(dir)
            .FirstOrDefault(file => !file.EndsWith(".meta") && Path.GetFileName(file).Contains(song.songname));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"곡 파일을 찾을 수 없습니다: {filePath}");
        Debug.Log($"곡 파일 로드: {filePath}");
        AudioType audioType = GetAudioType(filePath);
        string url = "file://" + filePath;

        using UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
        await req.SendWebRequest(); //! unity 6 기능임 다른 API와 호환이 안될 수 있음.

        if (req.result != UnityWebRequest.Result.Success)
            throw new Exception($"오디오 로드 실패 [{filePath}]: {req.error}");

        AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
        clip.name = Path.GetFileNameWithoutExtension(filePath);

        return clip;
    }

    #endregion

    #region 기본 파일 경로 관련
    private static string GetStreamingAssetsPath()
    {
        return Application.streamingAssetsPath+"/";
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
                folderPath += "RythmGame/";
        #endif
        return folderPath;
    }
    private static string[] GetSongNames(bool isBuiltin)
    {
        string dir = GetDirPathByType(SongDataType.SongInfo, "", isBuiltin);
        string[] songnames = Directory.GetDirectories(dir)  
        .Where(file => !file.EndsWith(".meta")).ToArray();
        for(int i = 0; i < songnames.Length; i++)
        {
            songnames[i] = songnames[i].Replace(dir, "").Replace("/", "");
        }
        return songnames;
    }
    private static AudioType GetAudioType(string path)
    {
        return Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".mp3" => AudioType.MPEG,
            ".wav" => AudioType.WAV,
            ".ogg" => AudioType.OGGVORBIS,
            ".aiff" or ".aif" => AudioType.AIFF,
            _ => AudioType.UNKNOWN
        };
    }
    private static List<string> GetFilePathByType(SongDataType dataType, string songname, bool isBuiltin)
    {
        List<string> paths = new List<string>();
        string path = "";

        if(isBuiltin && dataType != SongDataType.Record)
        {
            path = GetStreamingAssetsPath();
        }
        else
        {
            path = GetLocalPath();
            if(dataType == SongDataType.Record)
            {
                path += "Record/";
            }
            else
            {
                path += "Songs/";
            }
        }

        path += songname + "/";
        switch (dataType)
        {
            case SongDataType.SongInfo:
                path += _info+".json";
                paths.Add(path);
                break;
            case SongDataType.song:
                path += songname;
                paths.Add(path);
                break;
            case SongDataType.Record:
                paths = DifficultyFilePathAdder(path);
                break;
            case SongDataType.PatternInfo:
                paths = DifficultyFilePathAdder(path);
                break;
            case SongDataType.Pattern:
                paths = DifficultyFilePathAdder(path+"Pattern/");
                break;
            default:
                throw new ArgumentException("Invalid SongDataType");
        }
        return paths;
    }
    private static string GetDirPathByType(SongDataType dataType, string songname, bool isBuiltin)
    {
        string path = "";

        if(isBuiltin && dataType != SongDataType.Record)
        {
            path = GetStreamingAssetsPath();
        }
        else
        {
            path = GetLocalPath();
            if(dataType == SongDataType.Record)
            {
                path += "Record/";
            }
            else
            {
                path += "Songs/";
            }
        }
        path += songname + "/";
        
        if(dataType == SongDataType.Pattern)
        {
            path += "Pattern/";
        }
        return path;
    }
    private static List<string> DifficultyFilePathAdder(string path)
    {
        List<string> paths = new List<string>();
        for(int i = 0; i < _filenames.Length; i++)
        {
            paths.Add(path + _filenames[i] + ".json");
        }
        return paths;
    }
    public static bool ChackBuiltIn(string path)
    {
        string streamingAssetsPath = GetStreamingAssetsPath();
        return path.StartsWith(streamingAssetsPath);
    }
    #endregion

    #region string to class 변환
    private static T GetClassFromString<T>(string json) where T : class
    {
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogError($"[FileManager] 빈 JSON 문자열, 타입: {typeof(T).Name}");
            return null;
        }

        try
        {
            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[FileManager] 파싱 실패, 타입: {typeof(T).Name}, 에러: {e.Message}");
            return null;
        }
    }
    //json을 song으로 변환
    private static Song GetSongFromString(string songJson, List<string> patternInfosJson, List<string> recordsJson)
    {
        Song song = GetClassFromString<Song>(songJson);
        List<PatternInfo> patternInfos = new List<PatternInfo>();
        List<Record> records = new List<Record>();

        //json 변환
        foreach(string pattern in patternInfosJson)
        {
            if(string.IsNullOrEmpty(pattern))
            {
                Debug.LogWarning($"[FileManager] 빈 PatternInfo JSON 문자열, 곡: {song.songname}");
                continue;
            }
            if(pattern == "{}")
            {
                Debug.LogWarning($"[FileManager] 존재하지 않는 PatternInfo JSON 문자열, 곡: {song.songname}");
                continue;
            }
            
            PatternInfo temp = GetClassFromString<PatternInfo>(pattern);
            patternInfos.Add(temp);
        }
        foreach(string record in recordsJson)
        {
            if(string.IsNullOrEmpty(record))
            {   
                Debug.LogWarning($"[FileManager] 빈 Record JSON 문자열, 곡: {song.songname}");
                records.Add(new Record()
                {
                    comboResult = ComboResult.none,
                    maxcombo = 0,
                    prate = 0,
                    score = 0
                });
                continue;
            }
            if(record == "{}")
            {
                Debug.LogWarning($"[FileManager] 존재하지 않는 Record JSON 문자열, 곡: {song.songname}");
                records.Add(new Record()
                {
                    comboResult = ComboResult.none,
                    maxcombo = 0,
                    prate = 0,
                    score = 0
                });
                continue;
            }

            Record temp = GetClassFromString<Record>(record);
            records.Add(temp);
        }

        //예외처리용 기록 채우기 < 리펙토링 이후로 할필요 없어짐
        // for(int i = 0; i < 4; i++)
        // {
        //     records.Add(new Record()
        //     {
        //         comboResult = ComboResult.none,
        //         maxcombo = 0,
        //         prate = 0,
        //         score = 0
        //     });
        // }
        song.patternInfo = patternInfos;
        song.record = records;
        return song;
    }
    #endregion
    
    #region json file to string

    private static List<string> GetDataJson(SongDataType dataType, string songname,bool isBuiltin)
    {
        List<string> jsonDatas = new List<string>();
        List<string> paths;
        paths = GetFilePathByType(dataType, songname, isBuiltin);
        Debug.Log($"[FileManager] GetDataJson - dataType: {dataType}, songname: {songname}, isBuiltin: {isBuiltin}");
        foreach(string filePath in paths)
        {
            if(File.Exists(filePath) == false)
            {
                Debug.LogWarning($"[filemanager] 파일을 찾을 수 없습니다: {filePath}");
                jsonDatas.Add("{}");
                continue;
            }
            jsonDatas.Add(File.ReadAllText(filePath));
        }

        return jsonDatas;
    }

    //! 리펙토링된 함수, GetDataJson 사용 바람.
    // private static List<string> GetPatternInfoJson(string songname)
    // {
    //     List<string> patternsJson = new List<string>();
    //     string AssetPath = GetStreamingAssetsPath();

    //     string[] songpaths = Directory.GetFiles(AssetPath+"/SongInfo"+songname)
    //     .Where(file => file.EndsWith(".json")).ToArray();
        
    //     foreach(string path in songpaths)
    //     {
    //         if(path.Contains("Info.json")) continue;
    //         Debug.Log(File.ReadAllText(path));
    //         patternsJson.Add(File.ReadAllText(path));
    //     }

    //     return patternsJson;
    // }
    // private static List<string> GetRecordJson(string songname, string LocalPath)
    // {

    //     if(!File.Exists(LocalPath+"/Record"+songname)) return new List<string>();
        
    //     List<string> recordsJson = new List<string>();

    //     string[] recordpaths = Directory.GetFiles(LocalPath+"/Record"+songname)
    //     .Where(file => file.EndsWith(".json")).ToArray();

    //     foreach(string path in recordpaths)
    //     {
    //         recordsJson.Add(File.ReadAllText(path));
    //     }
    //     return recordsJson;
    // }
    // private static string GetPatternJson(string songname, int difficultyIndex)
    // {
    //     string patternPath = GetStreamingAssetsPath() + "/Pattern/" + songname + "/" + _filenames[difficultyIndex] + ".json";
    //     if (!File.Exists(patternPath))
    //     {
    //         Debug.LogWarning("파일 탐지 못함");
    //         return "";
    //     }
    //     string patternJson;

    //     patternJson = File.ReadAllText(patternPath);
    //     return patternJson;
    // }
    #endregion
    #region class to String
    private static string GetStringFromClass(object obj)
    {
        return JsonUtility.ToJson(obj, true);
    }
    #endregion
    #region String to json file

    private static void SetFileFromJson(SongDataType dataType, string songname, string json, bool isBuiltin, int patternIndex = -1)
    {
        List<string> paths = GetFilePathByType(dataType, songname, isBuiltin);
        string path;
        if(paths.Count > 1)
        {
            path = paths[patternIndex];
        }
        else
        {
            path = paths[0];
        }
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        if (File.Exists(path))
        {
            Debug.LogWarning("파일을 교체합니다.");
        }
        File.WriteAllText(path, json);
    }

    //리펙토링됨. SetFileFromJson 사용 바람.
    // private static void SetFileFromPattern(string songName, int patternIndex, string PatternJson)
    // {
    //     string path = GetStreamingAssetsPath()+"/Pattern/"+songName;
    //     if (!Directory.Exists(path))
    //     {
    //         Directory.CreateDirectory(path);
    //     }
    //     path += "/" + _filenames[patternIndex]+".json";
    //     if (File.Exists(path))
    //     {
    //         Debug.LogWarning("파일을 교체합니다.");
    //     }
    //     File.WriteAllText(path, PatternJson);
        

    // }
    // private static void SetFileFromSong(string songName, string SongJson)
    // {
    //     string path = GetStreamingAssetsPath()+"/SongInfo/"+songName;
    //     if (!Directory.Exists(path))
    //     {
    //         Directory.CreateDirectory(path);
    //     }
    //     path += "/0-Info"+".json";
    //     if (File.Exists(path))
    //     {
    //         Debug.LogWarning("파일을 교체합니다.");
    //     }
    //     File.WriteAllText(path, SongJson);
    // }
    // private static void SetFileFromPatternInfo(string songName,int patternIndex , string patternInfoJson)
    // {
    //     string path = GetStreamingAssetsPath()+"/SongInfo/"+songName;
    //     if (!Directory.Exists(path))
    //     {
    //         Directory.CreateDirectory(path);
    //     }
    //     path += "/"+_filenames[patternIndex]+".json";
    //     if (File.Exists(path))
    //     {
    //         Debug.LogWarning("파일을 교체합니다.");
    //     }
    //     File.WriteAllText(path, patternInfoJson);
    // }
    #endregion
    #region 테스트용 함수
    /// <summary>
    /// 패턴파일 경로는 pattern/TestSong/1-Easy.json 입니다.
    /// </summary>
    // public static Pattern TestPatternLoad()
    // {
    //     Pattern pattern;
    //     pattern = GetPatternFromString(GetPatternJson(GetStreamingAssetsPath() + "/Pattern/TestSong/" + _filenames[0] + ".json"));
    //     return pattern;
    // }
    #endregion
    // 추가한 코드 
    public static Record UpdateRecord(string songName, int patternIndex, PlayData playData)
    {
        Record newRecord = ConvertPlayDataToRecord(playData);
        //Record oldRecord = LoadRecord(songName, patternIndex, false);
        Record oldRecord = GetClassFromString<Record>(GetDataJson(SongDataType.Record, songName, false)[patternIndex]);
        
        if (IsBetterRecord(newRecord, oldRecord))
        {
            // SaveRecord(songName, patternIndex, newRecord);
            SetFileFromJson(SongDataType.Record, songName, GetStringFromClass(newRecord), false, patternIndex);
            return newRecord;
        }

        return oldRecord;
    }
    
    // public static Record LoadRecord(string songName, int patternIndex, bool isBuiltin = false)
    // {
    //     string filePath = GetFilePathByType(SongDataType.Record, songName, isBuiltin)[patternIndex];
    //     // string filePath = GetRecordFilePath(songName, patternIndex);

    //     if (!File.Exists(filePath))
    //     {
    //         return new Record();
    //     }

    //     string json = File.ReadAllText(filePath);
    //     return JsonUtility.FromJson<Record>(json);
    // }

    // public static void SaveRecord(string songName, int patternIndex, Record record, bool isBuiltin = false)
    // {
    //     string directoryPath = GetDirPathByType(SongDataType.Record, songName, isBuiltin);

    //     if (!Directory.Exists(directoryPath))
    //     {
    //         Directory.CreateDirectory(directoryPath);
    //     }

    //     string filePath = GetFilePathByType(SongDataType.Record, songName, isBuiltin)[patternIndex];
    //     // string filePath = GetRecordFilePath(songName, patternIndex);
    //     string json = JsonUtility.ToJson(record, true);

    //     File.WriteAllText(filePath, json);

    //     Debug.Log("Record saved: " + filePath);
    // }

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

    // private static string GetRecordDirectoryPath(string songName)
    // {
    //     string localPath = GetLocalPath();
    //     string safeSongName = SanitizeFileName(songName);

    //     return Path.Combine(localPath, "Record", safeSongName);
    // }

    // private static string GetRecordFilePath(string songName, int patternIndex)
    // {
    //     string directoryPath = GetRecordDirectoryPath(songName);
    //     string fileName = GetRecordFileName(patternIndex);

    //     return Path.Combine(directoryPath, fileName);
    // }

    // private static string GetRecordFileName(int patternIndex)
    // {
    //     switch (patternIndex)
    //     {
    //         case 0:
    //             return "1-Easy.json";
    //         case 1:
    //             return "2-Normal.json";
    //         case 2:
    //             return "3-Hard.json";
    //         case 3:
    //             return "4-Extreme.json";
    //         default:
    //             return (patternIndex + 1) + "-Unknown.json";
    //     }
    // }

    // private static string SanitizeFileName(string fileName)
    // {
    //     if (string.IsNullOrEmpty(fileName))
    //     {
    //         return "UnknownSong";
    //     }

    //     foreach (char invalidChar in Path.GetInvalidFileNameChars())
    //     {
    //         fileName = fileName.Replace(invalidChar, '_');
    //     }

    //     return fileName;
    // }

    public static bool Editor_TryLoadSongInfo(string songName, bool isbuiltin, out Song song) // json 파일 읽어오기
    {
        song = null;

        string path = GetFilePathByType(SongDataType.SongInfo, songName, isbuiltin)[0];

        if (!File.Exists(path))
        {
            Debug.LogWarning("Song info file not found: " + path);
            return false;
        }

        string json = File.ReadAllText(path);
        song = JsonUtility.FromJson<Song>(json);

        return song != null;
    }

    public static bool Editor_TryLoadPatternInfo(string songName, int patternIndex, bool isbuiltin, out PatternInfo patternInfo) // 난이도 정보 읽어오기
    {
        patternInfo = null;

        string[] filenames = { "1-Easy", "2-Normal", "3-Hard", "4-Extreme" };

        if (patternIndex < 0 || patternIndex >= filenames.Length)
            return false;

        string path = GetFilePathByType(SongDataType.PatternInfo, songName, isbuiltin)[patternIndex];

        if (!File.Exists(path))
            return false;

        string json = File.ReadAllText(path);
        patternInfo = JsonUtility.FromJson<PatternInfo>(json);

        return patternInfo != null;
    }

    public static bool Editor_TryLoadPattern(string songName, int patternIndex, bool isbuiltin, out Pattern pattern) // 패턴 읽어오기
    {
        pattern = null;

        string[] filenames = { "1-Easy", "2-Normal", "3-Hard", "4-Extreme" };

        if (patternIndex < 0 || patternIndex >= filenames.Length)
            return false;

        string path = GetFilePathByType(SongDataType.Pattern, songName, isbuiltin)[patternIndex];

        if (!File.Exists(path))
            return false;

        string json = File.ReadAllText(path);
        pattern = JsonUtility.FromJson<Pattern>(json);

        return pattern != null;
    }
}
