// AudioManager.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [SerializeField] private AudioMixer mixer;               
    [SerializeField] private string masterParam = "MasterVol";
    [SerializeField] private string bgmParam    = "BGMVol";
    [SerializeField] private string sfxParam    = "SFXVol";

    public static event Action VolumesApplied;
    public bool IsReady { get; private set; }
    public AudioMixer Mixer => mixer;
    public AudioMixerGroup SfxGroup { get; private set; }

    const string KEY_MASTER = "vol.master";
    const string KEY_BGM    = "vol.bgm";
    const string KEY_SFX    = "vol.sfx";
    const float MIN_DB = -80f, MAX_DB = 0f;

    float master01, bgm01, sfx01;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumes();
        ApplyAll();
        StartCoroutine(LateBoot()); // 1프레임 뒤 한 번 더(초기화 순서 이슈 대비)
        IsReady = true;
    }
    IEnumerator LateBoot()
    {
        yield return null;          // 한 프레임 대기
        ApplyAll();                 // 2차 적용
        VolumesApplied?.Invoke();
    }
    // 외부에서 mixer 주입하고 싶을 때(부트스트랩퍼)
    public void SetMixer(AudioMixer m)
    {
        mixer = m;
        var groups = mixer.FindMatchingGroups("SFX");
        if (groups != null && groups.Length > 0) SfxGroup = groups[0];
        ApplyAll();
    }

    public void SetMaster01(float v){ master01 = Mathf.Clamp01(v); Apply(masterParam, master01); PlayerPrefs.SetFloat(KEY_MASTER, master01); }
    public void SetBGM01   (float v){ bgm01    = Mathf.Clamp01(v); Apply(bgmParam,    bgm01);    PlayerPrefs.SetFloat(KEY_BGM,    bgm01);    }
    public void SetSFX01   (float v){ sfx01    = Mathf.Clamp01(v); Apply(sfxParam,    sfx01);    PlayerPrefs.SetFloat(KEY_SFX,    sfx01);    }

    public float GetMaster01()=> master01;
    public float GetBGM01()   => bgm01;
    public float GetSFX01()   => sfx01;

    void LoadVolumes()
    {
        master01 = PlayerPrefs.GetFloat(KEY_MASTER, 1f);
        bgm01    = PlayerPrefs.GetFloat(KEY_BGM,    1f);
        sfx01    = PlayerPrefs.GetFloat(KEY_SFX,    1f);
    }

    void ApplyAll()
    {
        if (mixer == null) return;
        Apply(masterParam, master01);
        Apply(bgmParam,    bgm01);
        Apply(sfxParam,    sfx01);
    }

    void Apply(string param, float v01)
    {
        if (mixer == null) return;
        float db = (v01 <= 0.0001f) ? MIN_DB : Mathf.Log10(v01) * 20f;
        mixer.SetFloat(param, Mathf.Clamp(db, MIN_DB, MAX_DB));
    }
}
