using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class TitleManager : MonoBehaviour
{
    [Header("=== 설정 ===")]
    [Tooltip("Start 버튼이 로드할 씬 이름")]
    [SerializeField] private string gameSceneName = "Game";

    [Tooltip("패널을 열 때 다른 패널은 자동으로 닫기")]
    [SerializeField] private bool closeOthersWhenOpen = true;

    [Header("=== 패널 참조 ===")]
    [SerializeField] private GameObject howPanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject BackGroundPanel;
    [SerializeField] private GameObject resentPanel;

    [Header("=== 최초 선택 포커스(선택사항) ===")]
    [SerializeField] private Selectable titleFirst;   // 타이틀 화면 기본 선택(예: Start 버튼)

    // --- Start ---
    public void OnClickStart()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogWarning("[TitleManager] gameSceneName이 비어있습니다.");
            return;
        }
        SceneManager.LoadScene(gameSceneName);
    }

    // --- How ---
    public void OnClickHow()
    {
        PanelOnOff(howPanel, true);
        resentPanel = howPanel;
    }

    // --- Setting ---
    public void OnClickSetting()
    {
        PanelOnOff(settingPanel, true);
        resentPanel = settingPanel;
    }
    public void OnClickBackGround()
    {
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
