using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
public class TitleManager : MonoBehaviour
{
    [Header("=== 설정 ===")]
    [Tooltip("Start 버튼이 로드할 씬 이름")]
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioSource sfx;

    [Header("=== 버튼 참조 ===")]
    [SerializeField] private GameObject buttonGroup;

    [Header("=== 패널 참조 ===")]
    [SerializeField] private GameObject howPanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject MenuBackPanel;
    [SerializeField] private GameObject resentPanel;

    [Header("=== 최초 선택 포커스(선택사항) ===")]
    [SerializeField] private Selectable titleFirst;   // 타이틀 화면 기본 선택(예: Start 버튼)
    [Header("=== 배경 확대 타겟 ===")]
    [SerializeField] private RectTransform bg; // 풀스크린 배경 이미지(UI)
    [Header("=== 트윈 설정 ===")]
    [SerializeField] private float startScale = 2f;
    [SerializeField] private float endScale   = 1f;
    [SerializeField] private float zoomDuration = 0.8f;
    [SerializeField] private Ease zoomEase = Ease.InOutQuad;
    [SerializeField] private CanvasGroup fadeGroup; // 선택: 검은 패널(CanvasGroup, 초기 Alpha=1)
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float extraHold = 0.1f;

    [Header("Position (수치로 지정)")]
    [Tooltip("시작시 위로 밀어올릴 픽셀 오프셋(+Y가 위)")]
    [SerializeField] float startOffsetY = 80f;
    [Tooltip("필요하면 X도 수치로 조정(좌/우)")]
    [SerializeField] float startOffsetX = 0f;

    [Tooltip("끝나는 위치(보통 0,0 = 중심)")]
    [SerializeField] Vector2 endPan = Vector2.zero;

    // --- Start ---
    void Start()
    {
        sfx = GetComponent<AudioSource>();
        howPanel.SetActive(false);
        settingPanel.SetActive(false);
        MenuBackPanel.SetActive(false);
    }void Awake()
    {
        // 시작: ‘살짝 위’에서 확대된 상태로 대기
        bg.localScale = Vector3.one * startScale;
        bg.anchoredPosition = endPan + new Vector2(startOffsetX, startOffsetY);
        buttonGroup.SetActive(true);
    }
    void Update()
    {
        // bg.anchoredPosition = endPan + new Vector2(startOffsetX, startOffsetY);
    }

    public void OnClickStart()
    {
        if (string.IsNullOrEmpty(gameSceneName) || bg == null) return;

        // if (buttonGroup) buttonGroup.SetActive(false);
        AllButtonDisable();
        DOTween.Kill(bg);
        DOTween.Kill(fadeGroup);

        var seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

        // 로고 페이드아웃 먼저
        seq.Append(fadeGroup.DOFade(0f, fadeDuration).SetEase(Ease.OutQuad));

        // 그 다음 줌아웃(스케일 + 포지션 동시)
        seq.Append(bg.DOScale(endScale, zoomDuration).SetEase(zoomEase));
        seq.Join(bg.DOAnchorPos(endPan, zoomDuration, true).SetEase(zoomEase));

        // 마무리
        seq.AppendInterval(extraHold)
           .OnComplete(() =>
           {
               bg.localScale = Vector3.one * endScale;
               bg.anchoredPosition = endPan;
               SceneManager.LoadScene(gameSceneName);
           });
    }
    // --- How ---
    public void OnClickHow()
    {
        sfx.PlayOneShot(clickClip);
        
        PanelOnOff(howPanel, true);
        resentPanel = howPanel;
    }

    // --- Setting ---
    public void OnClickSetting()
    {
        sfx.PlayOneShot(clickClip);
        PanelOnOff(settingPanel, true);
        resentPanel = settingPanel;
    }
    public void OnClickBackGround()
    {
        sfx.PlayOneShot(clickClip);
        PanelOnOff(resentPanel, false);
    }

    // --- Quit ---
    public void OnClickQuit()
    {
        // 에디터/빌드 환경 모두 종료 지원
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
    void AllButtonDisable()
    {
        if (buttonGroup == null) return;

        Button[] buttons = buttonGroup.GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons) btn.enabled = false;
    }

    private void PanelOnOff(GameObject target, bool onOff)
    {
        if (!target) return;
        
        target.SetActive(onOff);
        MenuBackPanel.SetActive(onOff);
    }

}
