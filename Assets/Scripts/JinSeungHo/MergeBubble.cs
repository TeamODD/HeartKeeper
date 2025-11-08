using System;
using Unity.VisualScripting;
using UnityEngine;

public class MergeBubble : MonoBehaviour
{
    public LayerMask areaLM;
    public GameObject leftArea;
    public GameObject rightArea;

    private Rigidbody2D rb;
    private Collision2D collidedObj;

    private bool isCollide;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collidedObj = null;   isCollide = false;
    }

    private void FixedUpdate()
    {
        if (isCollide) {
            MergeBeads(collidedObj);
        }
    }

    private void MergeBeads(Collision2D c)
    {
        if (c == null)
            return;

        // 현재 부딪힌 위치 추적
        Vector2 checkPosition = this.transform.position;

        // 구슬의 반지름 크기로 구역 감지
        CircleCollider2D circleCd = GetComponent<CircleCollider2D>();
        float radius = circleCd.radius * transform.lossyScale.x;

        // 현재 위치의 겹치는 areaLM의 레이어를 가지는 구역 검사
        Collider2D[] overlappingAreas = Physics2D.OverlapCircleAll(checkPosition, radius, areaLM);

        // 사각형을 찾지 못했다면 되돌아가기
        if (overlappingAreas.Length <= 0)
            return;

        // 현재 위치 = 사각형의 위치
        this.transform.position = overlappingAreas[0].transform.position;

        // 현재 이 구슬을 사각형의 부모로 편입
        this.transform.SetParent(overlappingAreas[0].transform, true);

        // 현재 위치 사각형의 버블이 붙은 상태임을 체크해준다.
        overlappingAreas[0].GetComponent<CheckBubble>().isBubbleOn = true;

        // 위치 고정
        rb.bodyType = RigidbodyType2D.Kinematic;

        // 위치가 고정되었으므로 각 구역의 버블이 추가되도록 함
        leftArea.GetComponent<CountInsideBox>().GetChildCheckBubble();
        rightArea.GetComponent<CountInsideBox>().GetChildCheckBubble();

        // 이후 구슬 합체는 더이상 진행되지 않으므로, 비활성화
        this.enabled = false;
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        isCollide = true;
        collidedObj = c;
        transform.GetComponent<Rigidbody2D>().freezeRotation = true;
    }
}
