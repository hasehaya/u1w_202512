using UnityEngine;

/// <summary>
/// オーディオクリップの情報を保持するScriptableObject
/// BGMやSEのクリップと設定を管理
/// </summary>
[CreateAssetMenu(fileName = "AudioClipData", menuName = "Audio/AudioClipData")]
public class AudioClipData : ScriptableObject
{
    [System.Serializable]
    public class BGMClipInfo
    {
        public BGMType bgmType;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.7f;
        public bool loop = true;
        public float fadeInDuration = 1f;
        public float fadeOutDuration = 1f;
    }

    [System.Serializable]
    public class SeClipInfo
    {
        public SeType seType;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("BGM設定")]
    [SerializeField] private BGMClipInfo[] bgmClips;

    [Header("SE設定")]
    [SerializeField] private SeClipInfo[] seClips;

    [Header("マスター音量")]
    [Range(0f, 1f)] public float masterBGMVolume = 1f;
    [Range(0f, 1f)] public float masterSeVolume = 1f;

    /// <summary>
    /// BGM情報を取得
    /// </summary>
    public BGMClipInfo GetBGMClipInfo(BGMType bgmType)
    {
        foreach (var info in bgmClips)
        {
            if (info.bgmType == bgmType)
            {
                return info;
            }
        }
        Debug.LogWarning($"BGM not found: {bgmType}");
        return null;
    }

    /// <summary>
    /// SE情報を取得
    /// </summary>
    public SeClipInfo GetSeClipInfo(SeType seType)
    {
        foreach (var info in seClips)
        {
            if (info.seType == seType)
            {
                return info;
            }
        }
        Debug.LogWarning($"SE not found: {seType}");
        return null;
    }
}

