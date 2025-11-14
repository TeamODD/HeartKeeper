using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class GameClearManager : MonoBehaviour
{
    // 게임이 끝났을 때 요정이 던지는 것을 비활성화 하기 위해 가져옴
    public GameObject gameLogicManager;

    [Header("왼쪽 오른쪽 가운데 버블 측정 박스 구역")]
    public GameObject leftArea;
    public GameObject rightArea;
    public GameObject middleArea;

    // 게임 클리어가 달성되었는지 UIManager.cs에서 참고할 예정
    public bool isGameClear = false;

    // 2초 뒤에 이 조건을 true로 바꾸기
    private bool isGameStart = false;

    private void Start()
    {
        StartCoroutine(Wait(2));
    }

    private void FixedUpdate()
    {
        if (isGameStart)
        {
            int leftBubble = leftArea.GetComponent<CountInsideBox>().currentObjCount;
            int rightBubble = rightArea.GetComponent<CountInsideBox>().currentObjCount;
            int middleBubble = middleArea.GetComponent<CountInsideBox>().currentObjCount;

            if ((leftBubble == 0 && rightBubble == 0 && middleBubble == 0) || isGameClear)
            {
                // 게임 클리어 상태로 변환
                isGameClear = true;
                // UI 호출 및 게임 종료
                GetComponent<UIManager>().setUI();
                // 요정이 버블을 던지는 코드 비활성화
                gameLogicManager.GetComponent<GameManager>().enabled = false;
                // 게임 오버가 되면 모든게 끝나므로 코드 비활성화
                this.enabled = false;
            }
        }
    }

    IEnumerator Wait(float delay)
    {
        // delay 만큼 이 스크립트 멈춤
        yield return new WaitForSeconds(delay);

        // Debug.Log("측정 시작");
        isGameStart = true; // 3초 뒤에 버블 수 측정 시작
    }
}
