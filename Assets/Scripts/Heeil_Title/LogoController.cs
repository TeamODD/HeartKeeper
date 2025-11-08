using UnityEngine;
using DG.Tweening;

/// <summary>
/// 로고컨트롤러: RawImage(혹은 임의 UI)의 RectTransform을
/// 좌↔우 기울이며(scale 펌핑 포함) 무한 반복 애니메이션.
/// </summary>
public class LogoController : MonoBehaviour
{
    [Header("대상")]
    [SerializeField] private RectTransform logo; // 로고가 붙은 오브젝트(RectTransform)

    [Header("회전(좌↔우 기울기)")]
    [SerializeField] private float tiltDeg = 8f;        // 좌/우 기울기 각도
    [SerializeField] private float tiltDuration = 0.8f; // 한쪽으로 기울어지는 시간

    [Header("스케일(펌핑)")]
    [SerializeField] private float scaleMin = 0.95f;
    [SerializeField] private float scaleMax = 1.05f;
    [SerializeField] private float scaleDuration = 0.9f;

    [Header("옵션")]
    [SerializeField] private bool autoStart = true;         // OnEnable 시 자동 시작
    [SerializeField] private bool timeScaleIndependent = true; // 타임스케일 무시(일시정지에도 재생)

    private Quaternion _initRot;
    private Vector3 _initScale;

    void Awake()
    {
        if (!logo) logo = GetComponent<RectTransform>();
        if (!logo)
        {
            Debug.LogWarning("[LogoController] RectTransform(logo)이 없습니다.");
            enabled = false;
            return;
        }

        _initRot   = logo.localRotation;
        _initScale = logo.localScale;
    }

    void OnEnable()
    {
        if (autoStart) StartIdle();
    }

    void OnDisable()
    {
        StopIdle();
    }

    /// <summary>무한 반복 아이들 애니메이션 시작</summary>
    public void StartIdle()
    {
        if (!logo) return;

        // 시작값 살짝 왼쪽 기울기 & 최소 스케일
        logo.localRotation = Quaternion.Euler(0f, 0f, -tiltDeg);
        logo.localScale    = Vector3.one * scaleMin;

        // 회전 트윈: -tilt → +tilt 왕복
        logo.DOKill();
        logo.DOLocalRotate(new Vector3(0f, 0f,  tiltDeg), tiltDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(timeScaleIndependent)
            .SetLink(gameObject);

        // 스케일 트윈: min ↔ max 왕복(동시)
        logo.DOScale(scaleMax, scaleDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(timeScaleIndependent)
            .SetLink(gameObject);
    }

    /// <summary>애니메이션 정지 및 초기값 복귀(원하면 주석 처리)</summary>
    public void StopIdle(bool restoreInitial = true)
    {
        if (!logo) return;
        DOTween.Kill(logo);

        if (restoreInitial)
        {
            logo.localRotation = _initRot;
            logo.localScale    = _initScale;
        }
    }

    // 인스펙터에서 값 바꿀 때 안전장치
    void OnValidate()
    {
        tiltDuration  = Mathf.Max(0.01f, tiltDuration);
        scaleDuration = Mathf.Max(0.01f, scaleDuration);
        scaleMin = Mathf.Max(0.01f, scaleMin);
        scaleMax = Mathf.Max(scaleMin + 0.001f, scaleMax);
    }
}
