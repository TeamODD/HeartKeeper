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
    [SerializeField] private GameObject BackGroundPanel;
    [SerializeField] private GameObject resentPanel;

    [Header("=== 최초 선택 포커스(선택사항) ===")]
    [SerializeField] private Selectable titleFirst;   // 타이틀 화면 기본 선택(예: Start 버튼)
    [Header("=== 배경 확대 타겟 ===")]
    [SerializeField] private RectTransform bg; // 풀스크린 배경 이미지(UI)
     [SerializeField] private Vector2 pivotTL = new(0.35f, 0.7f); // "살짝" 왼쪽 위
    [Header("=== 트윈 설정 ===")]
    [SerializeField] private float targetScale = 1.15f; // 1 → 1.15로 살짝 확대
    [SerializeField] private float zoomDuration = 0.8f;
    [SerializeField] private Ease zoomEase = Ease.InOutQuad;
    [SerializeField] private CanvasGroup fadeGroup; // 선택: 검은 패널(CanvasGroup, 초기 Alpha=0)
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float extraHold = 0.1f;
    // --- Start ---
    void Start()
    {
        sfx = GetComponent<AudioSource>();
        howPanel.SetActive(false);
        settingPanel.SetActive(false);
        BackGroundPanel.SetActive(false);
    }void Awake()
    {
        if (fadeGroup) fadeGroup.alpha = 0f;
        buttonGroup.SetActive(true);
    }

    public void OnClickStart()
    {
        if (string.IsNullOrEmpty(gameSceneName) || bg == null) return;

        buttonGroup.SetActive(false);
        DOTween.Kill(bg);

        var seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

        // 1) 좌상단 기준 확대
        seq.Join(bg.DOScale(targetScale, zoomDuration).SetEase(zoomEase));

        // 2) (선택) 페이드 겹쳐서
        if (fadeGroup)
        {
            float fadeDelay = Mathf.Max(0f, zoomDuration - fadeDuration * 0.8f);
            seq.Insert(fadeDelay, fadeGroup.DOFade(1f, fadeDuration));
        }

        // 3) 씬 로드
        seq.AppendInterval(extraHold)
        //    .OnComplete(() => Debug.Log("start!!!!"));
           .OnComplete(() => SceneManager.LoadScene(gameSceneName));
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

    // ===== 내부 유틸 =====
    // private void OpenPanel(GameObject target, Selectable firstFocus = null)
    // {
    //     if (!target) return;

    //     target.SetActive(true);
    //     BackGroundPanel.SetActive(true);
    // }

    // private void ClosePanel(GameObject target, Selectable focusAfterClose = null)
    // {
    //     if (!target) return;

    //     target.SetActive(false);
    //     BackGroundPanel.SetActive(false);
    // }
    private void PanelOnOff(GameObject target, bool onOff)
    {
        if (!target) return;
        
        target.SetActive(onOff);
        BackGroundPanel.SetActive(onOff);
    }

}
