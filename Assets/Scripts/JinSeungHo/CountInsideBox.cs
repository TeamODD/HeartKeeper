using UnityEngine;

public class CountInsideBox : MonoBehaviour
{
    public int currentObjCount = 0;
    public GameObject deleteBubbleManager;

    private void Update()
    {
        // 버블 개수를 잘 감지하는 지 확인하는 임시 디버그
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            GetChildCheckBubble();
            // Debug.Log(this.transform.name + "에 있는 버블 개수 : " + currentObjCount);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        GetChildCheckBubble();
        // 버블이 삭제되면 버블이 지워짐을 감지한다는 것을 확인
        // Debug.Log("구역에서 버블이 지워짐/이탈함");
        // 공중에 떠있는 버블도 추가 삭제 + 삭제가 다음 프레임에 되므로 약간 지연?
        Invoke("DelayedDelete", 0.1f);
    }

    private void DelayedDelete()
    {
        deleteBubbleManager.GetComponent<DeleteBubble>().DeleteUnconnectedBubble();
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
