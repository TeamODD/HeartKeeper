using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeleteBubble : MonoBehaviour
{
    public int floorNumber;
    public GameObject[][] bubbleFloor;

    // 연결된 구슬을 저장할 리스트
    private List<GameObject> connectedBubbles;
    // 이미 방문한 구슬을 체크하는 리스트
    private HashSet<GameObject> visitedBubbles;

    private void Awake()
    {
        // 버블 층 초기화
        bubbleFloor = new GameObject[floorNumber][];

        floorNumber = GetComponent<SaveFloorNumber>().floorNumber;

        for (int i = 0; i < floorNumber; i++)
        {
            string floorNumStr = "Floor" + i.ToString();
            // 층 초기화
            bubbleFloor[i] = GameObject.FindGameObjectsWithTag(floorNumStr);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // log로 확인
        for (int i = 0; i < floorNumber; i++)
        {
            Debug.Log(i + "층의 개수는 : " + bubbleFloor[i].Length);
        }
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        DeleteUnconnectedBubble();
    //        Debug.Log("스페이스바가 눌렸습니다. 버블을 지웁니다.");
    //    }
    //}

    public void DeleteUnconnectedBubble()
    {
        // 천장에 연결된 버블들을 모은다.
        List<GameObject> searchTopBubble = FindConnectedToCeiling();
        // 연결된 구역에 HashSet을 만듦
        HashSet<GameObject> connectedSet = new HashSet<GameObject>(searchTopBubble);

        // 현재 씬에 있는 모든 구역 사각형 감지 후 배열에 담음
        List<GameObject> allAreaObj = new List<GameObject>();
        for (int i = 0; i < floorNumber; i++)
        {
            if (bubbleFloor[i] != null)
            {
                allAreaObj.AddRange(bubbleFloor[i]);
            }
        }

        // 연결되지 않은 구역과 그 안에 붙어있는 구슬을 식별해 삭제
        for(int i = 0; i < allAreaObj.Count; i++) {
            // 모든 사각형 배열을 임시로 담음    
            GameObject areaObj = allAreaObj[i];

            // 이미 연결된 구역이면 건너 뜀
            if (areaObj == null || connectedSet.Contains(areaObj)) {
                continue;
            }

            // 현 구역의 버블이 있는지 확인하는 변수를 가진 스크립트 가져옴
            CheckBubble checkflag = areaObj.GetComponent<CheckBubble>();

            // 버블이 존재한다면
            if (checkflag != null && checkflag.isBubbleOn) {
                // 현재 사각형 구역의 자식으로 들어간 bubble을 삭제
                Transform bubble = null;
                if (areaObj.transform.childCount > 0)
                {
                    bubble = areaObj.transform.GetChild(0);
                }
               
                if(bubble != null)
                {
                    // 버블을 가지고 있는지 체크를 해제
                    checkflag.isBubbleOn = false;
                    
                    // 파괴된 버블이 위치하던 구역 사각형을 null로 처리해줌
                    for(int j = 0; j < floorNumber; j++)
                    {
                        for(int k = 0; k < bubbleFloor[j].Length; k++)
                        {
                            // 파괴할 버블이 위치하던 구역사각형을 찾아내면
                            if(bubbleFloor[j][k] == areaObj)
                            {
                                // null 처리후 for문을 빠져나감
                                bubbleFloor[j][k] = null;
                                goto EndSearch;     // 처리후 for문 빠져나감
                            }
                        }
                    }
                    EndSearch:;
                    // null 처리가 되었다면 bubble 삭제
                    Destroy(bubble.gameObject);
                }
            }
        }
    }

    private List<GameObject> FindConnectedToCeiling()
    {
        connectedBubbles = new List<GameObject>();
        visitedBubbles = new HashSet<GameObject>();

        // 최상위 층 (Floor 0)의 모든 구슬이 곧 시작점
        if (floorNumber > 0 && bubbleFloor[0] != null)
        {
            foreach (GameObject bubbleArea in bubbleFloor[0])
            {
                // 비어있는 구간이 있는 경우 스킵
                if(bubbleArea == null || bubbleArea.gameObject == null) continue;

                if(!visitedBubbles.Contains(bubbleArea))
                {
                    DFS(bubbleArea);
                }
            }
        }

        return connectedBubbles;
    }

    // DFS를 사용하여 현재 구슬과 연결된 모든 구슬을 찾음
    private void DFS(GameObject currentArea)
    {
        // 방울이 존재하지 않거나 이미 방문한 방울이라면 return
        if (currentArea == null || visitedBubbles.Contains(currentArea))
        {
            return;
        }   

        CheckBubble checkflag = currentArea.GetComponent<CheckBubble>();

        if(checkflag == null || !checkflag.isBubbleOn)
        {
            return;
        }

        // 현재 버블을 연결 목록, 방문 목록에 추가
        connectedBubbles.Add(currentArea);
        visitedBubbles.Add(currentArea);

        Vector2 currentPos = currentArea.transform.position;

        // 고정된 반지름을 이용해 서치
        float bubbleRadius = 1f;

        Collider2D[] neighbors = Physics2D.OverlapCircleAll(
            currentPos,
            bubbleRadius,
            LayerMask.GetMask("BoxArea")
        );

        foreach (Collider2D neighborCd in neighbors)
        {
            GameObject neighborBubble = neighborCd.gameObject;

            if (neighborBubble != currentArea &&
               !visitedBubbles.Contains(neighborBubble))
            {
                DFS(neighborBubble);
            }
        }

    }
}
