using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("색깔 속성")]
    [SerializeField] private Color[] colors = { Color.yellow, Color.red, Color.blue, Color.green };
    [SerializeField] private Color currentColor;
    [SerializeField] private Color nextColor;
    [SerializeField] private GameObject face;

    [Header("소환 속성")]
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject parent;

    [Header("발사 속성")]
    [SerializeField] private GameObject currentBall;
    [SerializeField] private float maxAimAngle = 120f;
    [SerializeField] private float aimMoveSpeed = 50f;
    [SerializeField] private float launchSpeed = 10;
    [SerializeField] private float intervalTime = 3f;
    float nextSetBallTime = -1f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentColor = colors[Random.Range(0, colors.Length)];
        nextColor = colors[Random.Range(0, colors.Length)];

        SetBall();
    }

    // Update is called once per frame
    void Update()
    {
        // 마우스 클릭 (BallControaller.Fire() 호출)
        bool clicked = Mouse.current != null
                    && Mouse.current.leftButton.wasPressedThisFrame;
        if (clicked) FireBall();
        
        if(nextSetBallTime > 0f && Time.time > nextSetBallTime)
        {
            nextSetBallTime = -1f;
            SetBall();
        }
    }

    public void FireBall()
    {
        currentBall.GetComponent<BallController>().Fire();
        nextSetBallTime = Time.time + intervalTime;
    }
    public void SetBall()
    {
        // 1) 공 소환 + 현재색 적용
        currentBall = Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation, parent.transform);
        InitBall(currentBall);
        ApplyColorToObject(currentBall, currentColor);

        ApplyColorToObject(face, nextColor);

        // 2) 색 파이프라인 밀기 (current ← next, next ← random)
        currentColor = nextColor;
        nextColor = colors[Random.Range(0, colors.Length)];
    }
    
    public void InitBall(GameObject ball)
    {
        if (ball.gameObject.TryGetComponent<BallController>(out var bc))
        {
            bc.maxAimAngle = maxAimAngle;
            bc.aimMoveSpeed = aimMoveSpeed;
            bc.launchSpeed = launchSpeed;
        }
    }
    public void ApplyColorToObject(GameObject gameObject, Color color)
    {
        if (gameObject.TryGetComponent<SpriteRenderer>(out var sr)) sr.color = color;
    }

}
