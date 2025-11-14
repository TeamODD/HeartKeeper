using System.Collections;
// using System.Diagnostics;
using UnityEngine;
using UnityEngine.Analytics;

public class GameClearSFXManager : MonoBehaviour
{
    [Header("게임 클리어 SFX")]
    public AudioClip gameClearSFX;

    [Header("클리어시 바꿀 게임 BGM")]
    public AudioClip gameClearBGM;

    private AudioSource audioSource;

    private bool isSFXPlayable = false;
    private bool isGameClear = false;
    private bool hasCoroutineStarted = false;
    // 페이드 아웃이 되고 난 뒤에 효과음 출력
    // 이 변수는 부모의 UIManager.cs 코드의 fadeoutDuration을 가져옴
    private float fadeoutDuration;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = gameClearSFX;
        audioSource.loop = false;
        audioSource.playOnAwake = false;
        audioSource.volume = 1;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // AudioManager에서 SFX 볼륨을 조절하면 이 소리의 볼륨도 함께 바뀜
        if (AudioManager.I != null && AudioManager.I.SfxGroup != null)
        {
            audioSource.outputAudioMixerGroup = AudioManager.I.SfxGroup;
        }

        fadeoutDuration = GetComponentInParent<UIManager>().fadeoutDuration;
    }

    // Update is called once per frame
    void Update()
    {
        isGameClear = GetComponentInParent<GameClearManager>().isGameClear;
        if(isGameClear)
        {
            if (!hasCoroutineStarted)
            {
                StartCoroutine(Wait(fadeoutDuration));
                hasCoroutineStarted = true;
            }
        }
        if (isSFXPlayable)
        {
            // 클리어 효과음 추가
            audioSource.Play();

            // 클리어 BGM 재생
            // dontdestroyonload에 실려있는 오디오 매니저 찾기
            GameObject audioManager = GameObject.Find("Audio Manager");
            if(audioManager == null)
            {
                Debug.Log("오디오 매니저를 찾지 못했습니다.");
                this.enabled = false;   // 게임 클리어시 사용 X
            }
            // 자식에 딸려있는 오디오 소스의 오디오 리소스 변경하기
            AudioSource bgmSource = audioManager.GetComponentInChildren<AudioSource>();
            // 우선 브금 바꾸기
            bgmSource.Stop();

            bgmSource.clip = gameClearBGM;
            bgmSource.loop = true;
            bgmSource.Play();

            this.enabled = false;   // 게임 클리어시 사용 X
        }
    }

    IEnumerator Wait(float delay)
    {
        // 3초 후에 SFX 재생
        yield return new WaitForSeconds(delay);

        isSFXPlayable = true;
    }
}