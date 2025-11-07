using UnityEngine;

public class CheckBubble : MonoBehaviour
{
    public bool isBubbleOn = false;

    // true가 되는 조건 : bubble이 이 사각형 범위의 isBubbleOn을 True로 바꿔줄 때
    private void OnTriggerExit2D(Collider2D collision)
    {
        isBubbleOn = false;
    }
}
