using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public GameObject leftBoxArea;
    public GameObject rightBoxArea;
    public float maxDiff = 10;
    public bool isGameOver = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int leftBubble = leftBoxArea.GetComponent<CountInsideBox>().currentObjCount;
        int rightBubble = rightBoxArea.GetComponent<CountInsideBox>().currentObjCount;

        int diff = Mathf.Abs(leftBubble - rightBubble);

        // 게임 오버 조건 충족시 게임 오버
        // 여기에 게임 오버시 실행할 것들 작성
        if(diff >= maxDiff)
        {
            isGameOver = true;
            Debug.Log("게임 오버");
        }
    }
}
