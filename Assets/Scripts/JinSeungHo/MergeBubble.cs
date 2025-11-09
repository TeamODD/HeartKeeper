using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MergeBubble : MonoBehaviour
{
    public LayerMask areaLM;
    public GameObject leftArea;
    public GameObject rightArea;

    private Rigidbody2D rb;
    private Collision2D collidedObj;

    public bool isCollide;
    // private bool canFire = false;
    public float waitTime = 3.1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collidedObj = null;   isCollide = false;

        // 시작한지 waitTime초 뒤에 모든 구슬의 고정을 풀고 머지 버블을 한번 호출함
        StartCoroutine(Wait(waitTime));
    }

    private void FixedUpdate()
    {
        if (isCollide) {
            MergeBubbles(collidedObj);
        }
    }

    private void MergeBubbles(Collision2D c)
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
        rb.bodyType = RigidbodyType2D.Static;

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
        // 충돌 감지
        // Debug.Log("충돌 감지 : " + c.gameObject.name + " 와 충돌함");
        transform.GetComponent<Rigidbody2D>().freezeRotation = true;
        //if (c.gameObject.tag == "Sticky" && canFire)
        //{
        //}
    }

    IEnumerator Wait(float wTime)
    {
        // Debug.Log(wTime + "후에 구슬 부착하도록 함");
        yield return new WaitForSeconds(wTime);

        // 우선 리지드 바디의 고정을 풂
        rb.constraints &= ~(
            RigidbodyConstraints2D.FreezePositionX | 
            RigidbodyConstraints2D.FreezePositionY);

        // 구슬을 고정하는 fj를 비활성화 한다.
        FixedJoint2D fj = this.GetComponent<FixedJoint2D>();
        if(fj != null)
        {
            fj.enabled = false;
        }

        // 리지드 바디의 bodyType을 kinetic에서 dynamic으로 바꾼다.
        rb.bodyType = RigidbodyType2D.Dynamic;
        // canFire = true;
    }
}
