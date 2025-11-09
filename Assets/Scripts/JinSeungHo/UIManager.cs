using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    // UI 관리(끄고 키기)를 위한 변수
    public GameObject canvasObj;

    // 직접적으로 UI를 관리
    public Image gameOverTextImg;
    public Image fadeBlack;
    public Button mainButton;
    public Button restartButton;
    // resume 버튼은 EscGameStopManager에서 SetActive 여부를 결정함, 여기서는 false로 초기화
    public Button resumeButton;

    // 게임 오버시에 얼마나 검게 페이드할 것인지 정하는 변수, (0 ~ 1)
    public float fadeoutAmount = 0.5f;
    // 게임 오버시 몇초동안 페이드 될지 정하는 변수
    public float fadeoutDuration = 3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //// 게임 시작시 UI 숨김
        //canvasObj.SetActive(false);
        // fadeBlack 이미지를 제외하고 전부 비활성화
        gameOverTextImg.gameObject.SetActive(false);
        mainButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        resumeButton.gameObject.SetActive(false);
    }

    // 호출시 UI를 전부 보여주고, 활성화함
    public void setUI()
    {
        // 페이드 아웃 코루틴 시작
        StartCoroutine(GameOverUIAppear());
    }

    // 게임을 재시작하는 버튼
    public void TaskRestartButtonOnClick()
    {
        mainButton.onClick.RemoveAllListeners();
        restartButton.onClick.RemoveAllListeners();
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    
    // 메인 화면으로 돌아가는 버튼
    public void TaskMainButtonOnClick()
    {
        mainButton.onClick.RemoveAllListeners();
        restartButton.onClick.RemoveAllListeners();
        string mainTitleScene = "heeil_Title";
        SceneManager.LoadScene(mainTitleScene);
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float timer = 0;

        Color c = fadeBlack.color;
        c.a = startAlpha;
        fadeBlack.color = c;

        // 페이드 시간동안 반복
        while(timer < fadeoutDuration)
        {
            // 타이머에 시간 경과
            timer += Time.deltaTime;

            // 페이드 보간 비율 => (경과 시간 / 총 시간)
            float t = timer / fadeoutDuration;

            // Mathf.Lerp를 사용, 현재 프레임의 투명도 값 계산
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, t);

            // 색의 투명도 = 새롭게 계산된 투명도
            c.a = newAlpha;
            fadeBlack.color = c;

            // 다음 프레임까지 기다린 후 코루틴 재개
            yield return null;
        }

        // 코루틴이 끝나면 확실한 페이드 아웃을 위해 최종 값을 대입
        c.a = endAlpha;
        fadeBlack.color = c;
    }

    IEnumerator GameOverUIAppear()
    {
        // 원래부터 UI가 활성화 된 상태로 하고, fadeBlack을 제외한 나머지 것들이 비활성화 되도록 함
        //// 그 이후에 UI를 활성화
        //// UI 활성화
        //canvasObj.SetActive(true);

        // 페이드 아웃 실행
        yield return StartCoroutine(Fade(0, fadeoutAmount));

        // 페이드 아웃이 끝나면 다시 전부 활성화
        gameOverTextImg.gameObject.SetActive(true);
        mainButton.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);

        // 두 버튼의 리스너를 추가
        mainButton.onClick.AddListener(TaskMainButtonOnClick);
        restartButton.onClick.AddListener(TaskRestartButtonOnClick);
    }
}
