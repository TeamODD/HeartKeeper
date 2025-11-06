using System;
using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{

    [Header("공 속성")]
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private float speed;
    [SerializeField] private string color;
    [SerializeField] private Vector2 upWard;
    [Header("발사 속성")]
    [SerializeField] private bool launched=false;
    [SerializeField] private float maxAimAngle;
    [SerializeField] private float aimMoveSpeed;

    private Rigidbody2D rb;
    private Camera cam;
    private SpriteRenderer sr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        cam = Camera.main;

        Init();
    }

    // Update is called once per frame
    void Update()
    {
        //한번 발사된 공은 비활성화
        if (launched == true) return;

        // //마우스 방향이 위를 바라보게 회전
        // transform.up = mouseDir();
        // upWard = mouseDir().normalized;
        
        // 와이퍼 움직임
        Wiper();
        //마우스 클릭
        bool clicked = Mouse.current != null 
                    && Mouse.current.leftButton.wasPressedThisFrame;
        if (clicked && !launched) Fire();
    }

    void Init()
    {
        string[] colors = { "yellow", "red", "blue", "green" };
        color = colors[UnityEngine.Random.Range(0, colors.Length)];

        Color c;
        switch (color)
        {
            case "yellow": c = Color.yellow; break;
            case "red": c = Color.red; break;
            case "blue": c = Color.blue; break;
            case "green": c = Color.green; break;
            default: c = Color.white; break; // 예외시 화이트
        }

        sr.color = c;
    }
    
    public

    void Wiper()
    {
        // float t;
        // t += Time.deltaTime;

        // // -1..+1로 진동하는 값 (사인파)
        // float s = Mathf.Sin(t * Mathf.PI * 2f * Mathf.Max(0f, cyclesPerSecond));
        // float angle = centerAngleDeg + s * Mathf.Abs(maxAngleDeg);

        // if (useLocalRotation)
        //     transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        // else
        //     transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    private Vector2 mouseDir()
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos  = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;

        return (mouseWorldPos - transform.position).normalized;
    }
    
    void Fire()
    {
        rb.AddForce(transform.up * speed, ForceMode2D.Impulse);
        launched = true;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.TryGetComponent<BallController>(out var other)) return;
        // 색이 같을 때
        if (other.color == color)
        {
            Debug.Log("같은 색");
        }
        // 다를 때
        else
        {
            Debug.Log("다른 색");
        }
    }


}
