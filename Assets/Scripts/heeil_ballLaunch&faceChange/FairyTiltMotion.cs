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
    [SerializeField] private float throwHandTiltDeg = -30f;
    [SerializeField] private float throwHandDuration = 0.4f;
    // 클래스 상단 어딘가에 추가
    [SerializeField] private float handStartZ = -17f; // 시작 Z 각도(요구: -3)

    // // === 추가 필드(원하면 인스펙터에서 조정) ===
    // [SerializeField] private float handStartZ = -3f;   // 던질 때 시작 각도(Z)
    // [SerializeField] private float handEndZ   = -17f;  // 던질 때 도착 각도(Z)


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
        if (!behindHand) behindHand = transform.GetChild(1);

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
        throwHandDuration = Mathf.Max(0.01f, throwHandDuration);
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
        behindHand.localRotation = Quaternion.Euler(0f, 0f, handStartZ);

        // -deg에서 시작해 +deg까지 요요
        // fairyTf.localRotation = Quaternion.Euler(0f, 0f, -grabTiltDeg);
        fairyTf
            .DOLocalRotate(new Vector3(0f, 0f, grabTiltDeg), grabTiltDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)          // Grab은 무한 반복
            .SetUpdate(timeScaleIndependent)
            .SetLink(gameObject);
    }

    // 
    void PlayThrowMotion()
    {
        if (!fairyTf) return;
        DOTween.Kill(fairyTf);
        DOTween.Kill(behindHand);

        var seq = DOTween.Sequence()
            .SetUpdate(timeScaleIndependent)
            .SetLink(gameObject);

        // 1) 던지기 전진 구간
        seq.Append(
            fairyTf.DOLocalRotate(new Vector3(0f, 0f, throwTiltDeg), throwTiltDuration)
                .SetEase(Ease.OutSine)
        );

        seq.Join(
            behindHand.DOLocalRotate(new Vector3(0, 0, throwHandTiltDeg),throwHandDuration)
                .SetEase(Ease.OutSine)
        );

        // 2) 복귀 구간
        seq.Append(
            fairyTf.DOLocalRotate(Vector3.zero, throwTiltDuration * 0.6f)
                .SetEase(Ease.InOutSine)
        );
        behindHand.localRotation = Quaternion.Euler(0f, 0f, handStartZ);
        // 끝나면 상태만 Grab으로
        seq.OnComplete(() =>
        {
            state = FairyState.Grab;
            ApplyState(); // 필요 없으면 이 줄 지워도 됨(정말 상태만 바꾸고 싶다면)
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
