using UnityEngine;

public class WarningSoundManager : MonoBehaviour
{
    [Header("재생할 경고음")]
    public AudioClip warningSFX;

    private AudioSource audioSource;

    // 현재 볼륨 강도
    private float currVol;
    private float targetVol;

    private bool is8thHasBubble = false;

    private void Awake()
    {
        // 오디오 소스 가져오기
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.clip = warningSFX;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 볼륨 초기화
        currVol = 0;
        targetVol = 0;

        // AudioManager에서 SFX 볼륨을 조절하면 이 소리의 볼륨도 함께 바뀜
        if (AudioManager.I != null && AudioManager.I.SfxGroup != null)
        {
            audioSource.outputAudioMixerGroup = AudioManager.I.SfxGroup;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 볼륨 강도 = 버블 변화에 따른 비네트 효과 강도 퍼센티지(VignetteManager)와 일치
        int leftBubble = GetComponentInParent<VignetteManager>().leftArea.GetComponent<CountInsideBox>().currentObjCount;
        int rightBubble = GetComponentInParent<VignetteManager>().rightArea.GetComponent<CountInsideBox>().currentObjCount;

        // 현재 차이 값
        float currDiff = Mathf.Abs(leftBubble - rightBubble);

        // 최대 차이 값 = 최대 버블량 - 최소 버블량
        float maxDiff = GetComponentInParent<VignetteManager>().maxDiffBubble;
        float minDiff = GetComponentInParent<VignetteManager>().minDiffBubble;

        // 최대 최소 차이 범위
        float range = maxDiff - minDiff;
        // 타겟 볼륨 초기화
        targetVol = 0;

        // 8층에 버블 존재 유무 = VignetteManager에서 처리
        is8thHasBubble = GetComponentInParent<VignetteManager>().is8thHasBubble;
        // 만약 8층에 버블이 존재한다면 최대치로 바로 조정
        if (is8thHasBubble)
            targetVol = 0.99f;  // 100%
        else if (Mathf.Abs(leftBubble - rightBubble) < GetComponentInParent<VignetteManager>().minDiffBubble)
        {
            // 왼쪽 오른쪽 차이값이 최소 값보다 작다면 볼륨 강도는 0
            targetVol = 0;
        }
        else    // 아니라면 볼륨 강도는 (현재 차이 값) / (최대 - 최소)
        {
            targetVol = (currDiff - minDiff) / range;
            
            // 볼륨이 1.0을 넘지 않도록 함
            targetVol = Mathf.Clamp01(targetVol);
        }

        // 목표 볼륨까지 부드럽게 전환
        currVol = Mathf.Lerp(currVol, targetVol, Time.deltaTime);

        // 볼륨 강도가 0보다 크다면
        if (currVol > 0.001f)
        {
            audioSource.volume = currVol;

            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {       // 볼륨 강도가 0이 되거나 게임이 멈추면
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.volume = 0;
            currVol = 0;
        }

        if(Time.timeScale == 0)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
}
