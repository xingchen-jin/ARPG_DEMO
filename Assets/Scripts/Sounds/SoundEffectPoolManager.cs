using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SoundEffectPoolManager : Singleton<SoundEffectPoolManager>
{

    [System.Serializable]
    public class SoundEffectPoolConfig
    {
        public SoundEffectType soundEffectType;
        public AudioClip[] clips;   //多个音频
        public int maxSize = 20;    
        public int defaultCapacity = 5;
    }
    public AudioSource audioSourcePrefab;//音频源预制体
    public List<SoundEffectPoolConfig> soundEffectPoolConfigs;
    private Dictionary<SoundEffectType, AudioClip[]> soundEffectClips = new Dictionary<SoundEffectType, AudioClip[]>();
    private ObjectPoolManager<SoundEffectType, AudioSource> soundEffectPoolManager = new ObjectPoolManager<SoundEffectType, AudioSource>();
    protected override void Awake()
    {
        base.Awake();
        foreach (var config in soundEffectPoolConfigs)
        {
            if (config.clips == null || config.clips.Length == 0)
            {
                Debug.LogWarning($"音效类型{config.soundEffectType}没有配置音频文件");
                continue;
            }
            soundEffectClips[config.soundEffectType] = config.clips;

            Transform parent = new GameObject($"SoundEffectPool_{config.soundEffectType}").transform;
            parent.SetParent(transform);

            soundEffectPoolManager.RegisterPool(config.soundEffectType, 
            createFunc: () =>
            {
                AudioSource audioSource = Instantiate(audioSourcePrefab, parent);
                return audioSource;
            }, onGet: (audioSource) =>
            {
                audioSource.gameObject.SetActive(true);
            }, onRelease: (audioSource) =>
            {
                audioSource.gameObject.SetActive(false);
            }, onDestroy: (audioSource) =>
            {
                Destroy(audioSource.gameObject);
            }, collectionCheck: false,
            defaultCapacity: config.defaultCapacity, maxSize: config.maxSize);
        }
    }
    
    public void OnPlayerSound(SoundEffectType type,Vector3 position,float volume = 0.3f,float pitch = 1f)
    {
        if (!soundEffectClips.ContainsKey(type))
        {
            Debug.LogWarning($"音效类型{type}没有配置音频文件");
            return;
        }
        AudioSource audioSource = soundEffectPoolManager.Get(type);
        if (audioSource == null)
        {
            Debug.LogWarning($"音效类型{type}的音频源获取失败");
            return;
        }
        AudioClip[] clips = soundEffectClips[type];
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        //TODO: 设置音频源的音量和音调
        audioSource.transform.position = position;
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();

        StartCoroutine(ReleaseAudioSourceAfterPlay(type, audioSource, clip.length/Mathf.Abs(pitch)));
    }

    private IEnumerator ReleaseAudioSourceAfterPlay(SoundEffectType type, AudioSource audioSource,float delay)
    {
        yield return new WaitForSeconds(delay);
        soundEffectPoolManager.Release(type, audioSource);
    }
}
