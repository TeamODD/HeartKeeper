using UnityEngine;

public class CountInsideBox : MonoBehaviour
{
    public int currentObjCount = 0;

    private void Update()
    {
        // 버블 개수를 잘 감지하는 지 확인하는 임시 디버그
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            GetChildCheckBubble();
            Debug.Log(this.transform.name + "에 있는 버블 개수 : " + currentObjCount);
        }
    }

    // 버블 카운트 로직 변경
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    currentObjCount++;
    //}

    private void OnTriggerExit2D(Collider2D collision)
    {
        GetChildCheckBubble();
    }

    public void GetChildCheckBubble()
    {
        CheckBubble[] allChildFloor = GetComponentsInChildren<CheckBubble>();
        int bubbleCount = 0;

        foreach(CheckBubble b in allChildFloor)
        {
            if (b.isBubbleOn)
            {
                bubbleCount++;
            }
        }

        currentObjCount = bubbleCount;
    }
}
