using System;
using Unity.VisualScripting;
using UnityEngine;

public class MergeBead : MonoBehaviour
{
    public LayerMask areaLM;
    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D c)
    {
        // 현재 부딪힌 위치 추적
        Vector2 checkPosition = this.transform.position;

        // 구슬의 반지름 크기로 구역 감지
        CircleCollider2D circleCd = GetComponent<CircleCollider2D>();
        float radius = circleCd.radius * transform.lossyScale.x;

        // 현재 위치의 겹치는 areaLM의 레이어를 가지는 구역 검사
        Collider2D[] overlappingAreas = Physics2D.OverlapCircleAll(checkPosition, radius, areaLM);

        // 현재 위치 = 사각형의 위치
        try
        {
            this.transform.position = overlappingAreas[0].transform.position;
            this.transform.SetParent(overlappingAreas[0].transform);
        } catch(IndexOutOfRangeException e)
        {
            // 충돌시 감지된 사각형이 없으면 에러 발생, 이 경우 다시 호출
            OnCollisionEnter2D(c);
            return;
        }
        

        // 현재 이 구슬을 사각형의 부모로 편입
        this.transform.SetParent(overlappingAreas[0].transform, true);

        // 위치 고정
        rb.bodyType = RigidbodyType2D.Kinematic;
    }
}
