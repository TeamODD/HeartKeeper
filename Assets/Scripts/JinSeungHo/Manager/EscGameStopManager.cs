using System.Collections;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscGameStopManager : MonoBehaviour
{
    [Header("UI")]
    public Image fadeBlackImg;
    public GameObject gameEndManager;
    public GameObject clickPanel;
    public GameObject settingPanel;
    public Button resumeButton;
    public Button mainButton;

    // 게임 정지, 게임 종료 여부
    private bool isStop = false;
    private bool isGameOver = false;
    private bool isGameClear = false;

    private void Start()
    {
        resumeButton.onClick.AddListener(GameRelease);
        mainButton.onClick.AddListener(GoToMainScene);
    }

    // Update is called once per frame
    void Update()
    {
        isGameOver = gameEndManager.GetComponent<GameOverManager>().isGameOver;
        isGameClear = gameEndManager.GetComponent <GameClearManager>().isGameClear;

        if (isGameOver || isGameClear) return;
        if (isStop)
        {
            // 시간 정지
            Time.timeScale = 0;
            
            // 화면을 50% 어둡게 함
            Color c = fadeBlackImg.color;
            c.a = 0.5f;
            fadeBlackImg.color = c;

            settingPanel.gameObject.SetActive(true);
            clickPanel.SetActive(false);

            // 그리고 Esc를 누르면 시간이 흐르게 함
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isStop = false;
            }
        }
        else
        {
            // 시간이 다시 흐름
            Time.timeScale = 1;
            // 화면이 다시 밝아짐 (투명도 0%)
            Color c = fadeBlackImg.color;
            c.a = 0f;
            fadeBlackImg.color = c;

            settingPanel.gameObject.SetActive(false);
            clickPanel.SetActive(true);

            // 그리고 Esc를 누르면 시간이 멈추게 함
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isStop = true;
            }

        }
    }

    private void GameRelease()
    {
        isStop = false;
    }

    private void GoToMainScene()
    {
        // 메인 씬으로 이동
        resumeButton.onClick.RemoveAllListeners();
        mainButton.onClick.RemoveAllListeners();
        string mainTitleScene = "heeil_Title";
        SceneManager.LoadScene(mainTitleScene);
    }
}
