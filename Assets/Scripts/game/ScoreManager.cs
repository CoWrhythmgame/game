using UnityEngine;

public class ScoreManager : MonoBehaviour
{

    [SerializeField] private InfoPannel _infoPannel;

    // 판정별 가중치를 무조건 '정수'로 둡니다 (부동소수점 오차 원천 차단)
    private const int WEIGHT_PERFECT = 100;
    private const int WEIGHT_GREAT = 80;
    private const int WEIGHT_GOOD = 50;
    private const int WEIGHT_MISS = 0;

    private const long MAX_SCORE = 1000000; // 100만점
    
    private int _noteCount;


    private long _maxPossibleWeight; // 이번 곡에서 얻을 수 있는 이론상 최대 가중치
    private long _currentAccumulatedWeight; // 유저가 현재까지 획득한 가중치 총합
    private int _combo;
    private int _maxCombo;

    // 곡이 시작될 때 (노트 스포너가 패턴을 다 읽고 난 후) 호출
    public void Initialize(int totalNoteCount, int holdNoteCount)
    {
        // 모든 노트를 Perfect로 쳤을 때의 가중치
        _maxPossibleWeight = (long)(totalNoteCount+holdNoteCount) * WEIGHT_PERFECT; 
        _currentAccumulatedWeight = 0;
        _combo = 0;
        _maxCombo = 0;
        _noteCount = 0;
        _infoPannel.SetScore(0,0);
    }

    // 노트 판정이 발생할 때마다 호출
    public void AddJudgment(string judgment)
    {
        // 1. 판정에 따른 정수 가중치 누적
        switch (judgment)
        {
            case "Perfect": _currentAccumulatedWeight += WEIGHT_PERFECT; break;
            case "Great": _currentAccumulatedWeight += WEIGHT_GREAT; break;
            case "Good": _currentAccumulatedWeight += WEIGHT_GOOD; break;
            case "Miss": _currentAccumulatedWeight += WEIGHT_MISS; break;
        }
        if(judgment == "Miss")
        {
            _combo = 0;
        }
        else
        {
            _combo++;
            if(_combo > _maxCombo) _maxCombo = _combo;
        }

        // 2. 🚨 핵심: 누적된 점수에 값을 더하는 게 아니라, 100만점 대비 비율로 '현재 점수'를 새로 도출합니다.
        // 분자가 먼저 곱해져야 정수 나눗셈에서 소실이 발생하지 않습니다.
        long currentScore = (MAX_SCORE * _currentAccumulatedWeight) / _maxPossibleWeight;

        Debug.Log($"현재 점수: {currentScore} / {MAX_SCORE}");
        
        _noteCount++;
        
        double prate = (100f * _currentAccumulatedWeight) / (WEIGHT_PERFECT*_noteCount);

        // 이 currentScore를 UI Canvas로 보내서 표시해줍니다.
        _infoPannel.SetScore(currentScore, prate);

    }
    public PlayData OnPatternEnd()
    {
        PlayData playData;
        playData = new PlayData
        {
            score = (MAX_SCORE * _currentAccumulatedWeight) / _maxPossibleWeight,
            maxcombo = _maxCombo,
            prate = (100f * _currentAccumulatedWeight) / _maxPossibleWeight
        };
        return playData;
    }
}
