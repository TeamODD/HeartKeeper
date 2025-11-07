using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    // [Header("사운드 속성")]
    // [SerializeField] private GameObject soundManager;

    [Header("색깔 속성")]
    [SerializeField] private Color[] colors =
        { Color.red,Color.yellow,Color.green,Color.blue };
    [SerializeField] private Color currentColor;
    [SerializeField] private Color nextColor;

    [Header("초상화 속성")]
    [SerializeField] private GameObject face;
    private SpriteRenderer faceSpriteRenderer;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Sprite defaultSprite;
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

    void Start()
    {
        faceSpriteRenderer = face.gameObject.GetComponent<SpriteRenderer>();
        faceSpriteRenderer.sprite = defaultSprite;
        
        currentColor = colors[Random.Range(0, colors.Length)];
        nextColor = colors[Random.Range(0, colors.Length)];

        SetBall();
    }
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

        ApplySpriteByColorToFace(nextColor);

        // 2) 색 파이프라인 밀기 (current ← next, next ← random)
        currentColor = nextColor;
        nextColor = colors[Random.Range(0, colors.Length)];

        
    }
    public void ApplySpriteByColorToFace(Color color)
    {
        Sprite sp =
            color == colors[0] ? sprites[0] :
            color == colors[1] ? sprites[1] :
            color == colors[2] ? sprites[2] :
            color == colors[3] ? sprites[3] : defaultSprite;
        faceSpriteRenderer.sprite = sp;
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
