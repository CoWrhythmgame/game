using UnityEngine;

public class ResultSceneTestBootstrap : MonoBehaviour
{
    [SerializeField] private bool useTestData = true;
    [SerializeField] private int patternIndex = 0;

    private void Awake()
    {
        if (!useTestData)
            return;

        if (DataMaster.Instance == null)
        {
            GameObject dataMasterObject = new GameObject("DataMaster");
            dataMasterObject.AddComponent<DataMaster>();
        }

        Song testSong = new Song
        {
            songname = "TestSong",
            artist = "TestArtist",
            bpm = 180
        };

        PlayData testPlayData = new PlayData
        {
            score = 950000,
            maxcombo = 850,
            prate = 97.35f,
            noteCount = new int[4] { 900, 80, 15, 0 },
            fscount = new int[2] { 20, 30 }
        };

        DataMaster.Instance.SetPlayResult(testSong, patternIndex, testPlayData);

        Debug.Log("ResultScene 테스트 데이터 생성 완료");
    }
}