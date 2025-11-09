using UnityEngine;
using DG.Tweening;

public class FairyGrabThrow : MonoBehaviour
{
    [Header("루트(기울일 대상)")]
    [SerializeField] private GameObject fairy;

    private enum FairyState { Grab, Throw }
    [SerializeField] private FairyState state = FairyState.Grab;

    [Header("몸,손")]
    [SerializeField] private Transform body;       
    [SerializeField] private Transform behindHand; 

    [Header("Grab 모션")]
    [SerializeField] private float grabTiltDeg = 8f;
    [SerializeField] private float grabTiltDuration = 0.8f;

    [Header("Throw 모션")]
    [SerializeField] private float throwTiltDeg = -30f;
    [SerializeField] private float throwTiltDuration = 0.8f;

    [Header("옵션")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool timeScaleIndependent = true;

    Transform fairyTf;
    GameObject grabObj, throwObj;

    Quaternion initLocalRot;
    Vector3    initLocalScale;

    FairyState lastState = (FairyState)(-1); // 절대 같지 않게 초기화

    void Awake()
    {
        if (!fairy) fairy = gameObject;
        fairyTf = fairy.transform;

        if (!body) body = transform.GetChild(0);

        if (body.childCount >= 1) grabObj  = body.GetChild(0).gameObject;
        if (body.childCount >= 2) throwObj = body.GetChild(1).gameObject;
        if (!grabObj || !throwObj)
            Debug.LogWarning("[FairyGrabThrow] body 아래 0/1번 자식이 필요합니다.");

        initLocalRot   = fairyTf.localRotation;
        initLocalScale = fairyTf.localScale;
    }

    void Start()
    {
        if (autoStart) ApplyState(force:true);
    }

    void OnValidate()
    {
        grabTiltDuration  = Mathf.Max(0.01f, grabTiltDuration);
        throwTiltDuration = Mathf.Max(0.01f, throwTiltDuration);
    }

    // === 외부 제어용 ===
    public void SetGrab()  { state = FairyState.Grab;  ApplyState(); }
    public void SetThrow() { state = FairyState.Throw; ApplyState(); }

    // ★ 상태가 바뀔 때만 실행 (Update에서 호출하지 않음)
    void ApplyState(bool force = false)
    {
        if (!force && lastState == state) return;

        switch (state)
        {
            case FairyState.Grab:
                if (grabObj)  grabObj.SetActive(true);
                if (throwObj) throwObj.SetActive(false);
                PlayGrabMotion();
                break;

            case FairyState.Throw:
                if (grabObj)  grabObj.SetActive(false);
                if (throwObj) throwObj.SetActive(true);
                PlayThrowMotion();
                break;
        }

        lastState = state;
    }

    void PlayGrabMotion()
    {
        if (!fairyTf) return;
        DOTween.Kill(fairyTf);

        // -deg에서 시작해 +deg까지 요요
        // fairyTf.localRotation = Quaternion.Euler(0f, 0f, -grabTiltDeg);
        fairyTf
            .DOLocalRotate(new Vector3(0f, 0f,  grabTiltDeg), grabTiltDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)          // Grab은 무한 반복
            .SetUpdate(timeScaleIndependent)
            .SetLink(gameObject);
    }

    void PlayThrowMotion()
    {
        if (!fairyTf) return;
        DOTween.Kill(fairyTf);

        var seq = DOTween.Sequence()
            .SetUpdate(timeScaleIndependent)
            .SetLink(gameObject);

        seq.Append(
            fairyTf.DOLocalRotate(new Vector3(0f, 0f, throwTiltDeg), throwTiltDuration)
                   .SetEase(Ease.OutSine)
        );
        seq.Append(
            fairyTf.DOLocalRotate(Vector3.zero, throwTiltDuration * 0.6f)
                   .SetEase(Ease.InOutSine)
        );

        seq.OnComplete(() =>
        {
            state = FairyState.Grab;
            ApplyState(); // 상태 변경 즉시 Grab 모션/표시 갱신
        });
    }

    public void StopMotion(bool restoreInitial = true)
    {
        if (!fairyTf) return;
        DOTween.Kill(fairyTf);
        if (restoreInitial)
        {
            fairyTf.localRotation = initLocalRot;
            fairyTf.localScale    = initLocalScale;
        }
    }

    void OnDisable()
    {
        DOTween.Kill(fairyTf);
    }
}
