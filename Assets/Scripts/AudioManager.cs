using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// BGMとSEの再生を管理するシングルトンマネージャー
/// 責務: オーディオの再生、停止、フェード、音量管理
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("設定")]
    [SerializeField] private AudioClipData audioClipData;

    private AudioSource bgmSource;
    private AudioSource seSource;

    // BGM管理
    private BGMType _currentBGMType = BGMType.None;
    private Coroutine _bgmFadeCoroutine;

    // SE用のプール
    private List<AudioSource> _seSourcePool = new List<AudioSource>();
    private const int SeSourcePoolSize = 5;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        // BGM用AudioSourceが設定されていない場合は作成
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }

        // SE用AudioSourceが設定されていない場合は作成
        if (seSource == null)
        {
            seSource = gameObject.AddComponent<AudioSource>();
            seSource.playOnAwake = false;
            seSource.loop = false;
        }

        // SE用AudioSourceのプールを作成
        for (int i = 0; i < SeSourcePoolSize; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            _seSourcePool.Add(source);
        }
    }

    #region BGM制御

    /// <summary>
    /// BGMを再生（フェードイン付き）
    /// </summary>
    public void PlayBGM(BGMType bgmType, bool forceRestart = false)
    {
        if (audioClipData == null)
        {
            Debug.LogWarning("AudioClipData is not assigned!");
            return;
        }

        // 同じBGMが再生中で、強制再起動でない場合は何もしない
        if (_currentBGMType == bgmType && bgmSource.isPlaying && !forceRestart)
        {
            return;
        }

        var bgmInfo = audioClipData.GetBGMClipInfo(bgmType);
        if (bgmInfo == null || bgmInfo.clip == null)
        {
            Debug.LogWarning($"BGM clip not found: {bgmType}");
            return;
        }

        // 既存のフェード処理を停止
        if (_bgmFadeCoroutine != null)
        {
            StopCoroutine(_bgmFadeCoroutine);
        }

        _currentBGMType = bgmType;
        bgmSource.clip = bgmInfo.clip;
        bgmSource.loop = bgmInfo.loop;

        // フェードインで再生
        _bgmFadeCoroutine = StartCoroutine(FadeInBGM(bgmInfo.volume * audioClipData.masterBGMVolume, bgmInfo.fadeInDuration));
    }

    /// <summary>
    /// BGMを停止（フェードアウト付き）
    /// </summary>
    public void StopBGM()
    {
        if (_bgmFadeCoroutine != null)
        {
            StopCoroutine(_bgmFadeCoroutine);
        }
        
        float fadeOutDuration = audioClipData.GetBGMClipInfo(_currentBGMType)?.fadeOutDuration ?? 0f;

        _bgmFadeCoroutine = StartCoroutine(FadeOutBGM(fadeOutDuration));
    }

    /// <summary>
    /// BGMを一時停止
    /// </summary>
    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    /// <summary>
    /// BGMを再開
    /// </summary>
    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    /// <summary>
    /// BGMの音量を設定
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume) * audioClipData.masterBGMVolume;
    }

    /// <summary>
    /// BGMフェードイン
    /// </summary>
    private IEnumerator FadeInBGM(float targetVolume, float duration)
    {
        bgmSource.volume = 0f;
        bgmSource.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
        _bgmFadeCoroutine = null;
    }

    /// <summary>
    /// BGMフェードアウト
    /// </summary>
    private IEnumerator FadeOutBGM(float duration)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        bgmSource.volume = 0f;
        bgmSource.Stop();
        _currentBGMType = BGMType.None;
        _bgmFadeCoroutine = null;
    }

    #endregion

    #region SE制御

    /// <summary>
    /// SEを再生
    /// </summary>
    public void PlaySe(SeType seType)
    {
        if (audioClipData == null)
        {
            Debug.LogWarning("AudioClipData is not assigned!");
            return;
        }

        var seInfo = audioClipData.GetSeClipInfo(seType);
        if (seInfo == null || seInfo.clip == null)
        {
            Debug.LogWarning($"SE clip not found: {seType}");
            return;
        }

        // プールから空いているAudioSourceを探す
        AudioSource availableSource = GetAvailableSeSource();
        if (availableSource != null)
        {
            availableSource.volume = seInfo.volume * audioClipData.masterSeVolume;
            availableSource.PlayOneShot(seInfo.clip);
        }
        else
        {
            // プールが全て使用中の場合は、デフォルトのseSourceを使用
            seSource.volume = seInfo.volume * audioClipData.masterSeVolume;
            seSource.PlayOneShot(seInfo.clip);
        }
    }

    /// <summary>
    /// プールから使用可能なAudioSourceを取得
    /// </summary>
    private AudioSource GetAvailableSeSource()
    {
        foreach (var source in _seSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        return null;
    }

    /// <summary>
    /// 全てのSEを停止
    /// </summary>
    public void StopAllSe()
    {
        seSource.Stop();
        foreach (var source in _seSourcePool)
        {
            source.Stop();
        }
    }

    /// <summary>
    /// SEの音量を設定
    /// </summary>
    public void SetSeVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp01(volume);
        seSource.volume = clampedVolume * audioClipData.masterSeVolume;
        
        foreach (var source in _seSourcePool)
        {
            source.volume = clampedVolume * audioClipData.masterSeVolume;
        }
    }

    #endregion

    #region マスター音量制御

    /// <summary>
    /// マスターBGM音量を設定
    /// </summary>
    public void SetMasterBGMVolume(float volume)
    {
        if (audioClipData != null)
        {
            audioClipData.masterBGMVolume = Mathf.Clamp01(volume);
            
            // 現在再生中のBGMの音量を更新
            if (_currentBGMType != BGMType.None)
            {
                var bgmInfo = audioClipData.GetBGMClipInfo(_currentBGMType);
                if (bgmInfo != null)
                {
                    bgmSource.volume = bgmInfo.volume * audioClipData.masterBGMVolume;
                }
            }
        }
    }

    /// <summary>
    /// マスターSE音量を設定
    /// </summary>
    public void SetMasterSeVolume(float volume)
    {
        if (audioClipData != null)
        {
            audioClipData.masterSeVolume = Mathf.Clamp01(volume);
        }
    }

    #endregion

    #region ユーティリティ

    /// <summary>
    /// 現在再生中のBGMタイプを取得
    /// </summary>
    public BGMType GetCurrentBGMType()
    {
        return _currentBGMType;
    }

    /// <summary>
    /// BGMが再生中かどうか
    /// </summary>
    public bool IsBGMPlaying()
    {
        return bgmSource.isPlaying;
    }

    #endregion
}

