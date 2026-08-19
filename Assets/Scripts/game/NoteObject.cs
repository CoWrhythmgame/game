using UnityEngine;
using UnityEngine.Pool;

public class NoteObject : MonoBehaviour
{
    [SerializeField]Sprite[] _noteSprite = new Sprite[2];
    [SerializeField]float _travelheight = 32;
    [SerializeField]float _traveldistance;
    // 자신이 속한 풀을 기억해두었다가 스스로 돌아갈 때 사용합니다.
    private IObjectPool<NoteObject> _managedPool;
    private NotePosManager _notePosManager;
    private Transform _transform;
    private Vector3 _judgePos;
    private Vector3 _tempVector = new Vector3(1,1,1);
    private int _laneIndex;
    private double _targetHitTime;
    private double _targetReleaseTime;
    private double _startTime;
    private float _noteSpeed;
    private bool _isHolding;
    private bool _isLong;
    
    // 풀에서 생성될 때 호출(초기화함수)
    public void Warming(NotePosManager notePosManager)
    {
        _transform = GetComponent<Transform>();
        GetComponent<SpriteRenderer>().sortingOrder = 1;
        _judgePos = GameObject.FindGameObjectWithTag("Judgement").transform.position;
        _notePosManager = notePosManager;
    }   

    // 풀에서 노트를 꺼낼 때 호출할 초기화 함수
    public void Initialize(IObjectPool<NoteObject> pool, Note note, Vector3 spawnPosition, float noteSpeed, float traveldistance)
    {
        _managedPool = pool;
        _laneIndex = note.lane;
        _targetHitTime = note.time;
        _noteSpeed = noteSpeed;
        _transform.position = spawnPosition;
        _isHolding = false;
        _tempVector = Vector3.one;
        _traveldistance = traveldistance; //이건 warming에서 find써서 정의 가능한 변수임. 나중에 최적화할때 확인바람.
        if(note.noteType == NoteType.hold)
        {
            _targetReleaseTime = note.releaseTime;
            _tempVector.y = (float)(note.releaseTime-note.time)*_noteSpeed*_travelheight/_traveldistance;
            _transform.localScale = _tempVector;
            GetComponent<SpriteRenderer>().sprite = _noteSprite[1];
            _isLong = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = _noteSprite[0];
            _transform.localScale = new Vector3(1, 1, 1);
            _isLong = false;
        }
        // 🚨 매우 중요: 이전 상태 초기화
        // 풀에서 재사용된 노트이므로, 이전 색상이나 투명도, 이펙트 등을 반드시 초기 상태로 돌려놔야 합니다.
        GetComponent<SpriteRenderer>().color = Color.white; 

        _notePosManager.AddNote(_transform);
    }

    // ! NotePosManager가 노트 위치를 다루고 있음.
    // private void Update()
    // {
    //     if (_isHolding)
    //     {
    //         double currentTime = AudioSettings.dspTime-_startTime;
    //         _tempVector.x = 1;
    //         _tempVector.y = Mathf.Clamp((float)(_targetReleaseTime-currentTime)*_noteSpeed*_travelheight/_traveldistance,0,float.MaxValue);
    //         _transform.localScale = _tempVector;
    //     }else{
    //     transform.Translate(Vector3.down * _noteSpeed * Time.deltaTime);
    //     }
    // }

    // 유저가 키를 눌러서 노트를 쳤을 때 (Hit 처리)
    public void OnHit(double startTime)
    {
        if(!_isLong) ReleaseToPool();
        _tempVector = _judgePos;
        _tempVector.x = _laneIndex - 1.5f;
        _transform.position = _tempVector;
        _startTime = startTime;
        _isHolding = true;
    }
    public void OnRelease()
    {
        ReleaseToPool();
    }
    /// <summary>
    /// 노트를 놓쳐서 화면에 보이지 않게 되면 풀로 반환하기 위해 호출되는 함수
    /// </summary>
    /// <param name="startTime">곡 시작 시간</param>
    /// HACK: 총 정지 시간을 확인할 필요가 있음
    public void OnMiss(double startTime)
    {
        double currentTime = AudioSettings.dspTime-startTime;
        Invoke("ReleaseToPool", (float)(_targetReleaseTime - currentTime+3f));

    }

    // 풀로 반환하는 함수
    private void ReleaseToPool()
    {
        _notePosManager.RemoveNote(_transform);
        // 이미 풀에 반환되었는지 체크 (중복 반환 방지)
        if (gameObject.activeSelf)
        {
            _managedPool.Release(this);
        }
    }
    public double GetTargetTime()
    {
        return _targetHitTime;
    }
    public double GetReleaseTime()
    {
        return _targetReleaseTime;
    }
    public bool GetIsLong()
    {
        return _isLong;
    }
    public bool GetIsHolding()
    {
        return _isHolding;
    }
    public int GetLaneIndex()
    {
        return _laneIndex;
    }
    public float GetNoteSpeed()
    {
        return _noteSpeed;
    }
}
