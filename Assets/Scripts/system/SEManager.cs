using UnityEngine;
using UnityEngine.Audio;

public class SEManager : MonoBehaviour
{
    // 재생할 오디오 클립 (인스펙터에서 할당)
    [SerializeField] private AudioClip _hitSoundClip;
    [SerializeField] private AudioMixerGroup _mixerGroup;
    
    // 미리 만들어둘 스피커(AudioSource)의 개수
    [SerializeField] private int _poolSize = 20;
    
    private AudioSource[] _audioSources;
    private int _currentIndex = 0; // 현재 사용할 스피커 번호

    private void Awake()
    {
        // 1. 게임 시작 시, 미리 스피커 20개를 만들어서 장착해둔다.
        _audioSources = new AudioSource[_poolSize];
        for (int i = 0; i < _poolSize; i++)
        {
            _audioSources[i] = gameObject.AddComponent<AudioSource>();
            _audioSources[i].outputAudioMixerGroup = _mixerGroup;
            _audioSources[i].playOnAwake = false;
        }
    }

    // 노트가 판정선에 닿아서 소리를 내야 할 때 이 함수를 호출
    public void PlayHitSound()
    {
        // 2. 현재 차례의 스피커를 가져옴
        AudioSource source = _audioSources[_currentIndex];

        // 3. 소리 장전 및 재생 (이미 재생 중이면 끊고 처음부터 다시 재생)
        source.clip = _hitSoundClip;
        source.Play();

        // 4. 다음 스피커로 순서 넘기기 (20번까지 가면 다시 0번으로 돌아옴)
        _currentIndex = (_currentIndex + 1) % _poolSize;
    }
}