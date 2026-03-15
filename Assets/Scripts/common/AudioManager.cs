using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 声音管理器，负责游戏中的所有音频播放和控制
/// </summary>
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();
                if (_instance == null)
                {
                    GameObject audioManagerObj = new GameObject("AudioManager");
                    _instance = audioManagerObj.AddComponent<AudioManager>();
                }
            }
            return _instance;
        }
    }

    [Header("音频设置")]
    [SerializeField] private bool musicEnabled = true;
    [SerializeField] private bool soundEnabled = true;
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float soundVolume = 1f;

    // 音频源
    private AudioSource musicSource;
    private List<AudioSource> soundSources = new List<AudioSource>();
    private const int MAX_SOUND_SOURCES = 10;

    // 当前播放的背景音乐
    private string currentMusicName = string.Empty;

    /// <summary>
    /// 背景音乐是否开启
    /// </summary>
    public bool MusicEnabled
    {
        get { return musicEnabled; }
        set
        {
            musicEnabled = value;
            UpdateMusicVolume();
        }
    }

    /// <summary>
    /// 音效是否开启
    /// </summary>
    public bool SoundEnabled
    {
        get { return soundEnabled; }
        set { soundEnabled = value; }
    }

    /// <summary>
    /// 背景音乐音量
    /// </summary>
    public float MusicVolume
    {
        get { return musicVolume; }
        set
        {
            musicVolume = Mathf.Clamp01(value);
            UpdateMusicVolume();
        }
    }

    /// <summary>
    /// 音效音量
    /// </summary>
    public float SoundVolume
    {
        get { return soundVolume; }
        set { soundVolume = Mathf.Clamp01(value); }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 创建背景音乐音频源
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;

        // 预创建音效音频源
        for (int i = 0; i < MAX_SOUND_SOURCES; i++)
        {
            AudioSource soundSource = gameObject.AddComponent<AudioSource>();
            soundSource.loop = false;
            soundSource.playOnAwake = false;
            soundSources.Add(soundSource);
        }

        Debug.Log("AudioManager实例已创建");
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="audioName">音频名称</param>
    /// <param name="fadeInTime">淡入时间（秒）</param>
    public void PlayMusic(string audioName, float fadeInTime = 0f)
    {
        if (string.IsNullOrEmpty(audioName))
        {
            Debug.LogWarning("音频名称为空");
            return;
        }

        // 如果正在播放相同的音乐，不重复播放
        if (currentMusicName == audioName && musicSource.isPlaying)
        {
            return;
        }

        AudioClip clip = Game.Instance.LoadAudio(audioName);
        if (clip == null)
        {
            Debug.LogWarning($"未找到背景音乐: {audioName}");
            return;
        }

        StopMusic();

        musicSource.clip = clip;
        currentMusicName = audioName;

        if (musicEnabled)
        {
            if (fadeInTime > 0)
            {
                StartCoroutine(MusicFadeIn(fadeInTime));
            }
            else
            {
                musicSource.volume = musicVolume;
                musicSource.Play();
            }
        }
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    /// <param name="fadeOutTime">淡出时间（秒）</param>
    public void StopMusic(float fadeOutTime = 0f)
    {
        if (!musicSource.isPlaying)
        {
            return;
        }

        if (fadeOutTime > 0)
        {
            StartCoroutine(MusicFadeOut(fadeOutTime));
        }
        else
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// 背景音乐淡入
    /// </summary>
    private IEnumerator MusicFadeIn(float duration)
    {
        float startVolume = 0f;
        float targetVolume = musicVolume;
        float elapsedTime = 0f;

        musicSource.volume = startVolume;
        musicSource.Play();

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsedTime / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    /// <summary>
    /// 背景音乐淡出
    /// </summary>
    private IEnumerator MusicFadeOut(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = musicVolume;
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="audioName">音频名称</param>
    /// <param name="volumeScale">音量缩放（0-1）</param>
    public void PlaySound(string audioName, float volumeScale = 1f)
    {
        if (!soundEnabled || string.IsNullOrEmpty(audioName))
        {
            return;
        }

        AudioClip clip = Game.Instance.LoadAudio(audioName);
        if (clip == null)
        {
            Debug.LogWarning($"未找到音效: {audioName}");
            return;
        }

        AudioSource soundSource = GetAvailableSoundSource();
        if (soundSource != null)
        {
            soundSource.clip = clip;
            soundSource.volume = soundVolume * volumeScale;
            soundSource.Play();
        }
    }

    /// <summary>
    /// 播放音效（3D音效）
    /// </summary>
    /// <param name="audioName">音频名称</param>
    /// <param name="position">播放位置</param>
    /// <param name="volumeScale">音量缩放（0-1）</param>
    public void PlaySound3D(string audioName, Vector3 position, float volumeScale = 1f)
    {
        if (!soundEnabled || string.IsNullOrEmpty(audioName))
        {
            return;
        }

        AudioClip clip = Game.Instance.LoadAudio(audioName);
        if (clip == null)
        {
            Debug.LogWarning($"未找到音效: {audioName}");
            return;
        }

        AudioSource soundSource = GetAvailableSoundSource();
        if (soundSource != null)
        {
            soundSource.clip = clip;
            soundSource.volume = soundVolume * volumeScale;
            soundSource.spatialBlend = 1f;
            soundSource.transform.position = position;
            soundSource.Play();
        }
    }

    /// <summary>
    /// 获取可用的音效音频源
    /// </summary>
    private AudioSource GetAvailableSoundSource()
    {
        // 优先查找未播放的音频源
        foreach (var source in soundSources)
        {
            if (!source.isPlaying)
            {
                source.spatialBlend = 0f;
                return source;
            }
        }

        // 如果所有音频源都在播放，使用最早的那个
        return soundSources[0];
    }

    /// <summary>
    /// 更新背景音乐音量
    /// </summary>
    private void UpdateMusicVolume()
    {
        if (musicEnabled)
        {
            musicSource.volume = musicVolume;
        }
        else
        {
            musicSource.volume = 0f;
        }
    }

    /// <summary>
    /// 切换背景音乐开关
    /// </summary>
    public void ToggleMusic()
    {
        MusicEnabled = !MusicEnabled;
        Debug.Log($"背景音乐已{(MusicEnabled ? "开启" : "关闭")}");
    }

    /// <summary>
    /// 切换音效开关
    /// </summary>
    public void ToggleSound()
    {
        SoundEnabled = !SoundEnabled;
        Debug.Log($"音效已{(SoundEnabled ? "开启" : "关闭")}");
    }

    /// <summary>
    /// 暂停所有音频
    /// </summary>
    public void PauseAll()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
        }

        foreach (var source in soundSources)
        {
            if (source.isPlaying)
            {
                source.Pause();
            }
        }
    }

    /// <summary>
    /// 恢复所有音频
    /// </summary>
    public void ResumeAll()
    {
        musicSource.UnPause();

        foreach (var source in soundSources)
        {
            source.UnPause();
        }
    }

    /// <summary>
    /// 停止所有音效
    /// </summary>
    public void StopAllSounds()
    {
        foreach (var source in soundSources)
        {
            source.Stop();
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
