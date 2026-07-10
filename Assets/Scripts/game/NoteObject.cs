using UnityEngine;
using UnityEngine.Pool;

public class NoteObject : MonoBehaviour
{
    // 자신이 속한 풀을 기억해두었다가 스스로 돌아갈 때 사용합니다.
    private IObjectPool<NoteObject> _managedPool;
    
    private int _laneIndex;
    private double _targetHitTime;
    private float _noteSpeed;
    
    // 풀에서 노트를 꺼낼 때 호출할 초기화 함수
    public void Initialize(IObjectPool<NoteObject> pool, int laneIndex, double targetHitTime, Vector3 spawnPosition, float noteSpeed)
    {
        _managedPool = pool;
        _laneIndex = laneIndex;
        _targetHitTime = targetHitTime;
        _noteSpeed = noteSpeed;

        transform.position = spawnPosition;
        
        // 🚨 매우 중요: 이전 상태 초기화
        // 풀에서 재사용된 노트이므로, 이전 색상이나 투명도, 이펙트 등을 반드시 초기 상태로 돌려놔야 합니다.
        GetComponent<SpriteRenderer>().color = Color.white; 
    }

    private void Update()
    {
        // (예시) 아래로 떨어지는 로직
        transform.Translate(Vector3.down * _noteSpeed * Time.deltaTime);

        // 노트가 판정선을 한참 지나쳐서 화면 밖으로 나갔다면 (Miss 처리)
        // if (transform.position.y < -10f)
        // {
        //     ReleaseToPool();
        // }
    }

    // 유저가 키를 눌러서 노트를 쳤을 때 (Hit 처리)
    public void OnHit()
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
}
