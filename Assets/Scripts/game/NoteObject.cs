using UnityEngine;
using UnityEngine.Pool;

public class NoteObject : MonoBehaviour
{
    [SerializeField]Sprite[] _noteSprite = new Sprite[2];
    [SerializeField]float _travelheight = 34;
    [SerializeField]float _traveldistance;
    // 자신이 속한 풀을 기억해두었다가 스스로 돌아갈 때 사용합니다.
    private IObjectPool<NoteObject> _managedPool;
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
    public void Warming()
    {
        _transform = GetComponent<Transform>();
        GetComponent<SpriteRenderer>().sortingOrder = 1;
        _judgePos = GameObject.FindGameObjectWithTag("Judgement").transform.position;
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
            _isLong = false;
        }
        // 🚨 매우 중요: 이전 상태 초기화
        // 풀에서 재사용된 노트이므로, 이전 색상이나 투명도, 이펙트 등을 반드시 초기 상태로 돌려놔야 합니다.
        GetComponent<SpriteRenderer>().color = Color.white; 
    }

    private void Update()
    {
        if (PauseManager.IsPaused)
            return;
        if (_isHolding)
        {
            double currentTime = AudioSettings.dspTime - PauseManager.TotalPausedDspTime - _startTime;
            _tempVector.x = 1;
            _tempVector.y = Mathf.Clamp((float)(_targetReleaseTime-currentTime)*_noteSpeed*_travelheight/_traveldistance,0,float.MaxValue);
            _transform.localScale = _tempVector;
        }else{
        transform.Translate(Vector3.down * _noteSpeed * Time.deltaTime);
        }
    }

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
    public void OnMiss()
    {
        ReleaseToPool();
    }

    // 풀로 반환하는 함수
    private void ReleaseToPool()
    {
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
}
