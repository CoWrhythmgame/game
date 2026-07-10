using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

//노트의 생성/풀링을 담당

public class NotePoolManager : MonoBehaviour
{
    [SerializeField] private NoteObject _notePrefab;
    [SerializeField] private JudgementManager judgementManager;
    [SerializeField] private int _defaultCapacity = 100; // 곡 시작 전 미리 만들어둘 노트 개수
    [SerializeField] private int _maxSize = 300;         // 최대 허용 노트 개수
    [SerializeField] private float _tripseconds = 2;
    private float spawnY;
    private float judgeY;
    private List<Vector3> noteSpawnPos = new List<Vector3>();

    private IObjectPool<NoteObject> _notePool;

    private void Awake()
    {
        // 풀 초기화
        _notePool = new ObjectPool<NoteObject>(
            createFunc: CreateNote,
            actionOnGet: OnTakeNoteFromPool,
            actionOnRelease: OnReturnNoteToPool,
            actionOnDestroy: OnDestroyNote,
            collectionCheck: false, // 릴리즈할 때 중복 검사 여부 (false가 성능에 더 좋음)
            defaultCapacity: _defaultCapacity,
            maxSize: _maxSize
        );
        SetNoteSpawnTransform(4);
        judgementManager = GameObject.FindGameObjectWithTag("JudgementManager").GetComponent<JudgementManager>();
        // 🚨 실무 핵심 (Pre-warm): 게임 시작(음악 재생) 전에 미리 100개를 생성해둡니다.
        PrewarmPool();

    }

    // 1. 풀이 비어있을 때 새로 생성하는 로직
    private NoteObject CreateNote()
    {
        NoteObject note = Instantiate(_notePrefab, transform);
        return note;
    }

    // 2. 풀에서 꺼낼 때 실행되는 로직
    private void OnTakeNoteFromPool(NoteObject note)
    {
        note.gameObject.SetActive(true);
    }

    // 3. 풀로 반환할 때 실행되는 로직
    private void OnReturnNoteToPool(NoteObject note)
    {
        note.gameObject.SetActive(false);
    }

    // 4. 풀 용량이 초과되어 노트를 파괴해야 할 때
    private void OnDestroyNote(NoteObject note)
    {
        Destroy(note.gameObject);
    }

    // 곡 로딩 화면에서 미리 노트를 만들어두어 렉을 방지하는 함수
    private void PrewarmPool()
    {
        NoteObject[] prewarmedNotes = new NoteObject[_defaultCapacity];
        for (int i = 0; i < _defaultCapacity; i++)
        {
            prewarmedNotes[i] = _notePool.Get(); // 일단 다 꺼내서 생성시킨 뒤
        }
        for (int i = 0; i < _defaultCapacity; i++)
        {
            _notePool.Release(prewarmedNotes[i]); // 다시 전부 집어넣습니다.
        }
    }

    public void SetNoteSpawnTransform(int lanecount)
    {
        noteSpawnPos.Clear();
        spawnY = GameObject.FindGameObjectWithTag("NoteSpawn").transform.position.y;
        judgeY = GameObject.FindGameObjectWithTag("Judgement").transform.position.y;
        for(int i = 0; i < lanecount; i++)
        {
            noteSpawnPos.Add(new Vector3(i-lanecount/2+0.5f, spawnY, 0f));
        }
    }
    // 실제 게임 중 노트를 스폰할 때 외부(패턴 매니저 등)에서 호출하는 함수
    public void SpawnNote(int laneIndex, double targetTime, float scrollSpeed, float noteSpeed = 1)
    {
        // 1. 풀에서 노트를 가져옵니다.
        NoteObject newNote = _notePool.Get();
        
        // 2. 위치 계산 (레인 인덱스에 따라 X좌표 결정)
        Vector3 spawnPos = noteSpawnPos[laneIndex];
        float speed = (spawnY-judgeY)/_tripseconds*scrollSpeed*noteSpeed;//노트 속도 - 기본값: _tripseconds초내에 spawn-judge거리를 이동
        // 3. 노트 초기화
        newNote.Initialize(_notePool, laneIndex, targetTime, spawnPos, speed);

        judgementManager.RegisterNoteToBuffer(laneIndex, newNote);
    }
}
