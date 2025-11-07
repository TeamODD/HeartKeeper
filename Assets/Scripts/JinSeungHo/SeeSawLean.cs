using Unity.VisualScripting;
using UnityEngine;

public class SeesawLean : MonoBehaviour
{
    public GameObject leftBox;
    public GameObject rightBox;

    // 기울기 제어 변수 (이전 코드 유지)
    public float anglePerDiff = 5;
    public float maxLeanAngle = 20;
    public float leanSpeed = 1;

    private CountInsideBox leftCounting;
    private CountInsideBox rightCounting;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftCounting = leftBox.GetComponent<CountInsideBox>();
        rightCounting = rightBox.GetComponent<CountInsideBox>();

        transform.rotation = Quaternion.Euler(0, 0, 180);
    }

    // Update is called once per frame
    void Update()
    {
        // 구슬 개수 카운트
        int leftCount = leftCounting.currentObjCount;
        int rightCount = rightCounting.currentObjCount;

        // 왼쪽 오른쪽 구슬 차이 계산
        int diff = leftCount - rightCount;

        // 차이가 나는 만큼 기울기 증가/감소
        float calculateAngle = 180 + (diff * anglePerDiff);

        // 기울어야 되는 각도 계산
        float targetAngle = Mathf.Clamp(calculateAngle, 180 - maxLeanAngle, 180 + maxLeanAngle);
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        // 목표 각도만큼 기울임
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * leanSpeed);

        // 기울어지는 조건 + 기울어지는 방향 확인
        //if (leftCount > rightCount)
        //{
        //    Debug.Log("왼쪽에 구슬이 " + (leftCount - rightCount).ToString() + "개 더 많음, 왼쪽으로 기욺");
        //}
        //else if (leftCount == rightCount)
        //{
        //    Debug.Log("현재 평형 상태");
        //}
        //else
        //{
        //    Debug.Log("오른쪽에 구슬이 " + (rightCount - leftCount).ToString() + "개 더 많음, 오른쪽으로 기욺");
        //}
    }

    // 이걸 구슬에 옮겨야 할듯?
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 부딫히는 물체가 구슬밖에 없다고 가정
        collision.rigidbody.freezeRotation = true;
    }
}