using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 노트 히트 이펙트 전담 오브젝트 풀.
/// 스프라이트 시트 기반 Animation Clip을 Animator로 재생하며,
/// 판정(Perfect/Great/Good/Miss)별로 다른 프리팹을 등록해 관리한다.
/// </summary>
public class NoteHitEffectPool : MonoBehaviour
{
    public static NoteHitEffectPool Instance { get; private set; }

    [SerializeField] private SEManager _seManager;

    [System.Serializable]
    public class EffectEntry
    {
        public string judgement;
        public Animator effectPrefab;
        [Tooltip("초기 생성해둘 풀 개수")]
        public int poolSize = 5;
        [Tooltip("Animator Controller 안의 State 이름. 비워두면 judgement 이름을 그대로 사용")]
        public string stateNameOverride;
    }

    [SerializeField] private List<EffectEntry> _effectEntries;

    private readonly Dictionary<string, Queue<Animator>> _pools = new();
    private readonly Dictionary<string, Animator> _prefabLookup = new();
    private readonly Dictionary<string, string> _stateNameLookup = new();
    private readonly Dictionary<string, float> _clipLengthLookup = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (var entry in _effectEntries)
        {
            if (entry.effectPrefab == null)
            {
                Debug.LogWarning($"[NoteHitEffectPool] {entry.judgement}에 대한 프리팹이 비어있습니다.");
                continue;
            }

            _prefabLookup[entry.judgement] = entry.effectPrefab;

            string stateName = string.IsNullOrEmpty(entry.stateNameOverride)
                ? entry.judgement.ToString()
                : entry.stateNameOverride;
            _stateNameLookup[entry.judgement] = stateName;

            _clipLengthLookup[entry.judgement] = GetClipLength(entry.effectPrefab, stateName);

            var queue = new Queue<Animator>();
            for (int i = 0; i < entry.poolSize; i++)
                queue.Enqueue(CreateInstance(entry.effectPrefab));
            _pools[entry.judgement] = queue;
        }
    }

    /// <summary>
    /// 지정한 판정의 히트 이펙트를 worldPos 위치에 재생한다.
    /// </summary>
    public void PlayHitEffect(string judgement, Vector3 worldPos)
    {
        if(judgement == "Miss")
        {
            // Miss 이펙트는 재생하지 않음
            return;
        }
        if (!_pools.TryGetValue(judgement, out var pool))
        {
            Debug.LogWarning($"[NoteHitEffectPool] {judgement}에 대한 이펙트가 등록되지 않았습니다.");
            return;
        }

        Animator instance = pool.Count > 0
            ? pool.Dequeue()
            : CreateInstance(_prefabLookup[judgement]); // 풀 고갈 시 즉석 생성

        instance.transform.position = worldPos;
        instance.gameObject.SetActive(true);
        instance.Play(_stateNameLookup[judgement], 0, 0f); // normalizedTime 0으로 강제 초기화
        _seManager.PlayHitSound();
        StartCoroutine(ReturnToPoolAfterPlay(instance, judgement));
    }

    private IEnumerator ReturnToPoolAfterPlay(Animator instance, string judgement)
    {
        yield return new WaitForSeconds(_clipLengthLookup[judgement]);

        instance.gameObject.SetActive(false);
        _pools[judgement].Enqueue(instance);
    }

    private Animator CreateInstance(Animator prefab)
    {
        var instance = Instantiate(prefab, transform);
        instance.gameObject.SetActive(false);
        return instance;
    }

    private float GetClipLength(Animator prefab, string stateName)
    {
        var clips = prefab.runtimeAnimatorController.animationClips;
        foreach (var clip in clips)
        {
            if (clip.name == stateName)
                return clip.length;
        }

        Debug.LogWarning($"[NoteHitEffectPool] '{stateName}' 이름의 Animation Clip을 찾지 못했습니다. 기본값 0.5초 사용.");
        return 0.5f;
    }
}