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
    [SerializeField] private AudioClip shotClip;    // 발사음
    [SerializeField] private AudioClip attachedClip;// 충돌음
    private AudioSource sfx;

    [Header("발사 속성")]
    [SerializeField] private GameObject arrow;
    public bool launched=false;
    public float launchSpeed;
    public float maxAimAngle=120;
    public float aimMoveSpeed=40;

    private Rigidbody2D rb;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sfx = GetComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.loop = false;
        sfx.spatialBlend = 0f;
        sfx.dopplerLevel = 0f;
    }

    void Update()
    {
        //한번 발사된 공은 비활성화
        if (launched == true)
        {
            arrow.SetActive(false);

            return;
        }
        else
        {
            arrow.SetActive(true);
            // 와이퍼 움직임
            transform.rotation = Quaternion.Euler(0f, 0f, GetWiperAngle());
        }
        
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
        
        if (shotClip != null)
            sfx.PlayOneShot(shotClip); 

        rb.AddForce(transform.up * launchSpeed, ForceMode2D.Impulse);
        launched = true;
    }
    public event Action<string, Collider2D> OnHitZone;
    public event Action<string, Collider2D> OnCollisionBall;
    // public bool hitHandled = false;
    // 트리거 방식(권장)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("OutZone")) //Destroy(gameObject);
            gameObject.SetActive(false); // 성능이슈

        // hitHandled = true;
        OnHitZone?.Invoke(other.name, other);
    }

    // 충돌 방식 쓰려면 이것도 가능(Zone이 isTrigger=Off일 때)
    // void OnCollisionEnter2D(Collision2D col)
    // {
    //     Debug.Log("충돌체");
    //     // hitHandled = true;
    //     OnHitZone?.Invoke(col.collider.name, col.collider);
    // }


}
