using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BGMSoundData;

public class AudioManager : MonoBehaviour
{
    [SerializeField] 
    private AudioSource _bgmAudioSource;
    [SerializeField] 
    private AudioSource _seAudioSource;

    [SerializeField]
    private List<BGMSoundData> _bgmSoundDatas;
    [SerializeField]
    private List<SESoundData> _seSoundDatas;

    public float _masterVolume = 1;
    public float _bgmMasterVolume = 1;
    public float _seMasterVolume = 1;

    public static AudioManager _instance { get; private set; } 
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(BGMSoundData.BGM bgm)
    {
        BGMSoundData data = _bgmSoundDatas.Find(data => data.bgm == bgm);
        _bgmAudioSource.clip = data.audioClip;
        _bgmAudioSource.volume = data.volume * _bgmMasterVolume * _masterVolume;
        _bgmAudioSource.Play();
    }

    public void PlaySE(SESoundData.SE se)
    {
        SESoundData data = _seSoundDatas.Find(data => data.se == se);
        _seAudioSource.volume = data.volume * _seMasterVolume * _masterVolume;
        _seAudioSource.PlayOneShot(data.audioClip);
    }
}

[System.Serializable]
public class BGMSoundData
{
    public enum BGM
    {
        Title,
    }
    public BGM bgm;
    public AudioClip audioClip;
    [Range(0, 1)]
    public float volume = 1;
}

[System.Serializable]
public class SESoundData
{
    public enum SE
    {
        Attack,
        Damage,
    }

    public SE se;
    public AudioClip audioClip;
    [Range(0, 1)]
    public float volume = 1;
}


