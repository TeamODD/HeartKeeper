using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string masterParam = "MasterVol";
    [SerializeField] private string bgmParam    = "BGMVol";
    [SerializeField] private string sfxParam    = "SFXVol";

    [Header("Audio Sources (Mixer 라우팅)")]
    [SerializeField] private AudioSource bgmSource;   // Mixer의 BGM 그룹에 연결
    [SerializeField] private AudioSource sfxSource;   // Mixer의 SFX 그룹에 연결

    const string KEY_MASTER="vol_master", KEY_BGM="vol_bgm", KEY_SFX="vol_sfx";

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        LoadVolumes();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy()
    {
        if (I == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로드 시: 이 씬의 BGMSource를 찾아 즉시 재생/정지
    void OnSceneLoaded(Scene _, LoadSceneMode __)
    {
        var tag = FindAnyObjectByType<BGMSource>();

        if (tag) PlayBGM(tag.clip, tag.loop, tag.volume01);
        else     StopBGM();
    }

    // ===== 볼륨(슬라이더) =====
    public void SetMaster(float v) => SetDb(masterParam, v, KEY_MASTER);
    public void SetBGM   (float v) => SetDb(bgmParam,    v, KEY_BGM);
    public void SetSFX   (float v) => SetDb(sfxParam,    v, KEY_SFX);

    void SetDb(string param, float linear01, string key)
    {
        float v = Mathf.Clamp01(linear01);
        float dB = (v <= 0.0001f) ? -80f : Mathf.Log10(v) * 20f;
        mixer.SetFloat(param, dB);
        PlayerPrefs.SetFloat(key, v);
        PlayerPrefs.Save();
    }

    void LoadVolumes()
    {
        SetMaster(PlayerPrefs.GetFloat(KEY_MASTER, 1f));
        SetBGM   (PlayerPrefs.GetFloat(KEY_BGM,    1f));
        SetSFX   (PlayerPrefs.GetFloat(KEY_SFX,    1f));
    }

    // ===== 재생 API (즉시 전환) =====
    public void PlayBGM(AudioClip clip, bool loop = true, float volume01 = 1f)
    {
        if (!bgmSource) return;

        // 같은 클립이 이미 재생 중이면 무시
        if (bgmSource.isPlaying && bgmSource.clip == clip) return;

        bgmSource.Stop();
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = Mathf.Clamp01(volume01); // Mixer 전/후 원하는 쪽 기준으로 사용
        if (clip) bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource) bgmSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float volume01 = 1f)
    {
        if (!sfxSource || !clip) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume01));
    }

    // 슬라이더 초기 표시용 Getter
    public float GetMaster01() => PlayerPrefs.GetFloat(KEY_MASTER, 1f);
    public float GetBGM01()    => PlayerPrefs.GetFloat(KEY_BGM,    1f);
    public float GetSFX01()    => PlayerPrefs.GetFloat(KEY_SFX,    1f);
}
