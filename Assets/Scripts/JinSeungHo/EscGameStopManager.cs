using System.Collections;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.UI;

public class EscGameStopManager : MonoBehaviour
{
    public Image fadeBlackImg;
    public Button resumeButton;
    public GameObject gameOverManager;
    public GameObject panel;
    private bool isStop = false;
    private bool isGameOver = false;

    private void Start()
    {
        resumeButton.onClick.AddListener(GameRelease);
    }

    // Update is called once per frame
    void Update()
    {
        bool isGameOver = gameOverManager.GetComponent<GameOverManager>().isGameOver;
        if (isGameOver) return;
        if (isStop)
        {
            // 시간 정지
            Time.timeScale = 0;
            
            // 화면을 50% 어둡게 함
            Color c = fadeBlackImg.color;
            c.a = 0.5f;
            fadeBlackImg.color = c;

            resumeButton.gameObject.SetActive(true);
            panel.SetActive(false);

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

            resumeButton.gameObject.SetActive(false);
            panel.SetActive(true);

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
}
