using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.PlayerLoop;

public class JudgementManager : MonoBehaviour
{
    [SerializeField] private InfoPannel _infoPannel;
    [SerializeField] private ToastText _judgeText;
    [SerializeField] private NoteHitEffectPool _noteHitEffectPool;
    private ScoreManager scoreManager;
    private PatternManager patternManager;
    // 4버튼 기준, 레인별로 노트를 담아둘 큐 배열
    private Queue<NoteObject>[] _laneBuffers;
    private PlayOption _playOption;
    private int[] _judgeCount;
    private int[] _FSList;
    private double _startAudioTime;
    private double _startInputTime;
    private double _offset;
    private float _noteOffset;
    private float _judgeY;
    // 판정 기준 시간 (단위: 초)
    private readonly double _perfectWindow = 0.05001; // ±50.01ms
    private readonly double _greatWindow = 0.08335;   // ±83.35ms
    private readonly double _goodWindow = 0.11669;   // ±116.69ms
    // private readonly double _missWindow = 0.15003;    // ±150.03ms
    private readonly double _missWindow = 0.5d;    //임시
    private readonly double _longPerfectWindow = 0.08335; // ±50.01ms
    private double _FSwindow = 0.05001;
    private bool _isHold;

    private void Awake()
    {
        // 4개의 레인 버퍼 초기화
        _laneBuffers = new Queue<NoteObject>[4];
        for (int i = 0; i < 4; i++)
        {
            _laneBuffers[i] = new Queue<NoteObject>();
        }
        _judgeCount = new int[4]{0,0,0,0};
        _FSList = new int[2]{0,0};
        scoreManager = transform.GetComponent<ScoreManager>();
        patternManager = GameObject.FindGameObjectWithTag("NoteManager").GetComponent<PatternManager>();

        _judgeY = GameObject.FindGameObjectWithTag("Judgement").transform.position.y;


        _infoPannel.SetJudgeCount(_judgeCount);
    }

    // 1. 노트 스포너가 노트를 생성할 때 버퍼에 등록합니다.
    public void RegisterNoteToBuffer(int laneIndex, NoteObject note)
    {
        _laneBuffers[laneIndex].Enqueue(note);
    }

    // 2. 아까 만든 InputManager에서 특정 레인 키가 눌렸을 때 호출됩니다.
    public void OnLaneInputFired(int laneIndex, double inputTime)
    {
        Queue<NoteObject> buffer = _laneBuffers[laneIndex];
        if (PauseManager.IsPaused) 
            return;
        double adjustedInputTime = inputTime - PauseManager.TotalPausedInputTime; // 멈춘 시간만큼 정지
        // 해당 레인에 쳐야 할 노트가 없으면 무시
        if (buffer.Count == 0) return;

        // 큐의 맨 앞(가장 먼저 떨어지는) 노트 확인 (아직 빼지 않음)
        NoteObject targetNote = buffer.Peek();

        // 오차 시간 계산
        double timeDiff = targetNote.GetTargetTime() - adjustedInputTime + _offset;
        
        bool isFast = false;
        if(timeDiff > 0) isFast = true;
        timeDiff = Math.Abs(timeDiff);
        Debug.Log("입력 오차: "+timeDiff);
        // 판정 로직
        if (timeDiff <= _perfectWindow)
        {
            ProcessHit(buffer, targetNote, "Perfect");
        }
        else if (timeDiff <= _greatWindow)
        {
            ProcessHit(buffer, targetNote, "Great");
        }
        else if (timeDiff <= _goodWindow)
        {
            ProcessHit(buffer, targetNote, "Good");
        }
        else if (targetNote.GetTargetTime() > adjustedInputTime - _startInputTime && timeDiff <= _missWindow)
        {
            // 너무 일찍 친 경우 (Fast Miss)
            ProcessHit(buffer, targetNote, "Miss");
        }
        // 허용 범위 밖으로 너무 일찍 눌렀다면 아무 반응도 하지 않고 남겨둡니다 (허공 치기)

        Debug.Log("time: " + inputTime+" target: " + targetNote.GetTargetTime());

        //페슬
        if(timeDiff > _FSwindow)
        {
            ProcessFS(isFast);
        }
    }    
    //키를 땠을때 호출
    public void OnLaneReleaseFired(int laneIndex, double inputTime)
    {
        Queue<NoteObject> buffer = _laneBuffers[laneIndex];
        if (PauseManager.IsPaused)
            return;
        // 해당 레인에 쳐야 할 노트가 없으면 무시
        if (buffer.Count == 0) return;

        // 큐의 맨 앞(가장 먼저 떨어지는) 노트 확인 (아직 빼지 않음)
        NoteObject targetNote = buffer.Peek();

        //노트가 롱노트이면서 판정중이 아니라면
        if(!(targetNote.GetIsLong() && targetNote.GetIsHolding())) return;

        // 멈춘 시간만큼 정지
        double adjustedInputTime = inputTime - PauseManager.TotalPausedInputTime;

        // 오차 시간 계산
        double timeDiff = targetNote.GetReleaseTime() - adjustedInputTime + _offset;
    
        timeDiff = Math.Abs(timeDiff);
        Debug.Log("입력 오차: "+timeDiff);
        // 판정 로직
        if (timeDiff <= _longPerfectWindow)
        {
            ProcessHit(buffer, targetNote, "Perfect");
            ProcessRelease(buffer, targetNote);
        }
        else
        {
            ProcessHit(buffer, targetNote, "Miss");
            // 너무 일찍 친 경우 (Fast Miss)
            ProcessRelease(buffer, targetNote);
        }
        // 허용 범위 밖으로 너무 일찍 눌렀다면 아무 반응도 하지 않고 남겨둡니다 (허공 치기)

        //페슬
        if(timeDiff > _FSwindow)
        {
            ProcessFS(true);
        }
    }


    // 타격 성공 처리
    private void ProcessHit(Queue<NoteObject> buffer, NoteObject note, string judgment)
    {
        Debug.Log($"판정: {judgment}");
        _judgeText.ShowToast(judgment, 0.5f);
        scoreManager.AddJudgment(judgment);
        switch (judgment)
        {
            case "Perfect": _judgeCount[0]++; break;
            case "Great": _judgeCount[1]++; break;
            case "Good": _judgeCount[2]++; break;
            case "Miss": _judgeCount[3]++; break;
        }
        if(!note.GetIsLong()) buffer.Dequeue(); // 롱노트가 아니면 버퍼에서 제거
        note.OnHit(_startAudioTime);     // 타격 이펙트 재생 및 Pool로 반환
        _infoPannel.SetJudgeCount(_judgeCount);

        _noteHitEffectPool.PlayHitEffect(judgment, new Vector3(note.GetLaneIndex() - 1.5f, _judgeY, 0f));

        //이거때문에 note메니저랑 judgement메니저끼리 상호간섭함
        //더 좋은 방안이 없을까
        patternManager.CheckPatternEnd();
    }
    //롱놋 해제 처리
    private void ProcessRelease(Queue<NoteObject> buffer, NoteObject note)
    {
        buffer.Dequeue();
        note.OnRelease();
        patternManager.CheckPatternEnd();
    }
    private void ProcessFS(bool isFast)
    {
        if (isFast)
        {
            Debug.Log("FAST");
            _FSList[0]++;
        }
        else
        {
            Debug.Log("SLOW");
            _FSList[1]++;
        }
    }
    public void SetStartTime(double audioTime, double inputTime)
    {
        _startAudioTime = audioTime;
        _startInputTime = inputTime;
        
        _playOption = GameObject.FindGameObjectWithTag("DataMaster").GetComponent<DataMaster>().GetPlayOption();
        _noteOffset = _playOption.noteOffset;
        _offset = _startInputTime+_noteOffset;
    }

    private void Update()
    {
        if (PauseManager.IsPaused)
            return;
        // 3. 유저가 치지 않고 놓친 노트(Miss) 처리
        // Time.time 대신 반드시 음악의 현재 위치(DSP 타임 등)를 가져와야 합니다.
        double currentTime = AudioSettings.dspTime - PauseManager.TotalPausedDspTime - _startAudioTime;

        //이거 좀 위험해보임
        for (int i = 0; i < _laneBuffers.Length; i++)
        {
            if (_laneBuffers[i].Count > 0)
            {
                NoteObject targetNote = _laneBuffers[i].Peek();
                // 롱노트가 아닌데 늦었으면
                if (currentTime - targetNote.GetTargetTime() > _goodWindow && !targetNote.GetIsLong())
                {
                    Debug.Log("Miss! (놓침)");

                    targetNote.OnMiss(); // Pool로 반환
                    ProcessHit(_laneBuffers[i], targetNote, "Miss");
                }
                // 롱노트인데 늦었으면
                if (currentTime - targetNote.GetReleaseTime() > _longPerfectWindow && targetNote.GetIsLong())
                {
                    Debug.Log("Good, 롱노트 놓침");

                    ProcessHit(_laneBuffers[i], targetNote, "Good");
                    ProcessRelease(_laneBuffers[i], targetNote);
                    ProcessFS(false);
                }
            }
        }
    }
    #region GameEnd
    public PlayData OnPatternEnd()
    {
        int notecount = 0;
        foreach(Queue<NoteObject> buffer in _laneBuffers)
        {
            notecount+=buffer.Count;
        }
        if(notecount != 0) return null;
        PlayData playData = scoreManager.OnPatternEnd();
        playData.fscount = _FSList;
        playData.noteCount = _judgeCount;

        return playData;
    }
    #endregion
}
