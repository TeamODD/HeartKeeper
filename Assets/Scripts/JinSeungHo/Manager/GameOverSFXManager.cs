using System.Collections;
using UnityEngine;

public class GameOverSFXManager : MonoBehaviour
{
    [Header("게임 오버 SFX")]
    public AudioClip gameOverSFX;

    private AudioSource audioSource;

    // 게임 오버 판정
    private bool hasCoroutineStarted = false;
    private bool isGameOver = false;
    private bool isSFXPlayable = false;
    // 페이드 아웃이 되고 난 뒤에 효과음 출력
    // 이 변수는 부모의 UIManager.cs 코드의 fadeoutDuration을 가져옴
    private float fadeoutDuration;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = gameOverSFX;
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

    private void Update()
    {
        isGameOver = GetComponentInParent<GameOverManager>().isGameOver;
        if (isGameOver)
        {
            if (!hasCoroutineStarted)
            {
                StartCoroutine(Wait(fadeoutDuration));
                hasCoroutineStarted = true;
            }
        }
        if (isSFXPlayable)
        {
            audioSource.Play();
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
