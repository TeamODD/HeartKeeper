using System;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{

    [Header("공 속성")]
    [SerializeField] private string color;
    [SerializeField] private Vector2 upWard;
    [Header("효광음 속성")]
    public SoundManager soundManager;
    [Header("발사 속성")]
    public bool launched=false;
    public float launchSpeed;
    public float maxAimAngle=120;
    public float aimMoveSpeed=40;

    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Awake()
    {
        if (soundManager == null)
            soundManager = FindAnyObjectByType<SoundManager>();
    }

    void Update()
    {
        //한번 발사된 공은 비활성화
        if (launched == true)
        {
            var t = transform.Find("Arrow"); // 자식 이름이 정확히 "arrow"
            if (t) t.gameObject.SetActive(false);

            return;
        }
        // 와이퍼 움직임
        transform.rotation = Quaternion.Euler(0f, 0f, GetWiperAngle());
    }
    
    // 와이퍼처럼 -maxAimAngle ↔ +maxAimAngle 를 왕복하며 각도를 반환
    // aimMoveSpeed: 초당 각도 변화량
    float GetWiperAngle()
    {
        float half = maxAimAngle / 2;
        if (maxAimAngle <= 0f || aimMoveSpeed <= 0f) return 0f;

        // 0→1→0으로 왕복하는 선형 파형 (속도 = aimMoveSpeed 유지)
        float phase01 = Mathf.PingPong(Time.time * (aimMoveSpeed / (2f * half)), 1f);

        // -max ↔ +max 로 매핑
        return Mathf.Lerp(-half, half, phase01);
    }

    
    public void Fire()
    {
        if (launched == true) return;
        
        soundManager.Play_shot();
        rb.AddForce(transform.up * launchSpeed, ForceMode2D.Impulse);
        launched = true;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        soundManager.Play_attached();
        if (!col.collider.TryGetComponent<BallController>(out var other)) return;
        // 색이 같을 때
        if (other.color == color)
        {
        }
        // 다를 때
        else
        {
        }
    }


}
