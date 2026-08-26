using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    [SerializeField] private MeasureList _measureList;
    [SerializeField] private EditorSongFileLoader _editorSongFileLoader;
    [SerializeField] private EditorSongInfoUI _editorSongInfoUI;
    [SerializeField] private EditorDifficultyUI editorDifficultyUI;

    public void New_SavePattern()
    {
        SavePattern();
    }

    public async Awaitable<bool> TrySavePattern()
    {
        DataMaster dataMaster = GameObject.FindGameObjectWithTag("DataMaster").GetComponent<DataMaster>();

        try
        {
            if (_editorSongFileLoader == null)
            {
                Debug.LogWarning("EditorSongFileLoader가 연결되지 않았습니다.");
                return false;
            }

            if (_measureList == null)
            {
                Debug.LogWarning("MeasureList가 연결되지 않았습니다.");
                return false;
            }

            if (editorDifficultyUI == null)
            {
                Debug.LogWarning("EditorDifficultyUI가 연결되지 않았습니다.");
                return false;
            }

            EditorLoadedSongData editorLoadedSongData = _editorSongFileLoader.GetCurrentSongData();

            if (editorLoadedSongData == null)
            {
                Debug.LogWarning("로드된 곡 정보가 없습니다.");
                return false;
            }

            if (!editorDifficultyUI.CanSaveCurrentDifficulty())
            {
                Debug.LogWarning("현재 난이도는 테스트 플레이할 수 없습니다. 체크 여부와 난이도 값을 확인하세요.");
                return false;
            }

            editorDifficultyUI.SaveCurrentPatternToMemory();

            int difficultyIndex = editorDifficultyUI.CurrentDifficultyIndex;
            float difficultyLevel = editorDifficultyUI.GetDifficultyLevel(difficultyIndex);

            Pattern pattern = _measureList.GetPattern();

            Song song = new Song
            {
                songname = editorLoadedSongData.songName,
                bpm = editorLoadedSongData.bpm,
                artist = editorLoadedSongData.artistName
            };

            PatternInfo patternInfo = new PatternInfo
            {
                difficulty = Mathf.RoundToInt(difficultyLevel),
                totalNoteCount = pattern.notes != null ? pattern.notes.Count : 0
            };
            FileManager.Editor_SavePattern(song, patternInfo, pattern, _editorSongInfoUI.GetDifficultyIndex());
            byte[] jaketdata = _editorSongFileLoader.GetJacketImageData();
            if(jaketdata != null)
            {
                FileManager.SaveJacket(song.songname, false, jaketdata);
            }
            else
            {
                Debug.LogWarning("자켓 이미지 데이터가 없습니다. 자켓 이미지를 저장하지 않습니다.");
            }

            song = FileManager.LoadSong(false).Where(x => x.songname == song.songname).ToArray()[0];
            AudioClip musicClip = await FileManager.LoadMusic(song, false);
            dataMaster.SetSongData(song, editorLoadedSongData.selectedDifficultyIndex);


            dataMaster.SetSongData(song, difficultyIndex);
            dataMaster.SetMusic(musicClip);
            dataMaster.SetIsTestPlay(true);

            EditorReturnContext.Set(
                editorLoadedSongData.songName,
                difficultyIndex
            );

            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning("노래 정보에 문제가 생겼습니다!");
            Debug.LogException(e);
            return false;
        }
    }

    public async void TestPlay()
    {
        try
        {
            if (await TrySavePattern())
            {
                SceneManager.LoadScene("InGameScene");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("노래 정보에 문제가 생겼습니다!");
            Debug.LogException(e);
        }
    }

    public bool SavePattern()
    {
        if (_editorSongFileLoader == null)
        {
            Debug.LogWarning("EditorSongFileLoader가 연결되지 않았습니다.");
            return false;
        }

        if (editorDifficultyUI == null)
        {
            Debug.LogWarning("EditorDifficultyUI가 연결되지 않았습니다.");
            return false;
        }

        EditorLoadedSongData editorLoadedSongData = _editorSongFileLoader.GetCurrentSongData();

        if (editorLoadedSongData == null)
        {
            Debug.LogWarning("로드된 곡 정보가 없습니다.");
            return false;
        }

        editorDifficultyUI.SaveCurrentPatternToMemory();

        Song song = new Song
        {
            songname = editorLoadedSongData.songName,
            bpm = editorLoadedSongData.bpm,
            artist = editorLoadedSongData.artistName
        };

        bool savedAnyDifficulty = false;

        for (int i = 0; i < editorDifficultyUI.DifficultyCount; i++)
        {
            if (!editorDifficultyUI.IsDifficultyEnabled(i))
                continue;

            if (!editorDifficultyUI.CanSaveDifficulty(i))
            {
                Debug.LogWarning("저장할 수 없는 난이도입니다: " + editorDifficultyUI.GetDifficultyName(i));
                return false;
            }

            Pattern pattern = editorDifficultyUI.GetPatternForSave(i);

            if (pattern == null)
                continue;

            float difficultyLevel = editorDifficultyUI.GetDifficultyLevel(i);

            PatternInfo patternInfo = new PatternInfo
            {
                difficulty = Mathf.RoundToInt(difficultyLevel),
                totalNoteCount = pattern.notes != null ? pattern.notes.Count : 0
            };

            FileManager.Editor_SavePattern(song, patternInfo, pattern, i);
            byte[] jaketdata = _editorSongFileLoader.GetJacketImageData();
            if(jaketdata != null)
            {
                FileManager.SaveJacket(song.songname, false, jaketdata);
            }
            else
            {
                Debug.LogWarning("자켓 이미지 데이터가 없습니다. 자켓 이미지를 저장하지 않습니다.");
            }

            Debug.Log("Pattern saved. Difficulty: " + editorDifficultyUI.GetDifficultyName(i));

            savedAnyDifficulty = true;
        }

        if (!savedAnyDifficulty)
        {
            Debug.LogWarning("저장할 활성화된 난이도가 없습니다.");
            return false;
        }


        return true;
    }

    public void SavePatternFromButton()
    {
        SavePattern();
    }
}