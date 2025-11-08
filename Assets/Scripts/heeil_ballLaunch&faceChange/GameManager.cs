using System.Collections;
using TMPro;
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
    [SerializeField] private Sprite curentSprite;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private float faceAppearDuration=1f;

    [Header("소환 속성")]
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject parent;
    [SerializeField] private float ballAppearDuration=0.5f;

    [Header("발사 속성")]
    [SerializeField] private GameObject clickPanel;
    [SerializeField] private GameObject currentBall;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private float maxAimAngle = 120f;
    [SerializeField] private float aimMoveSpeed = 50f;
    [SerializeField] private float launchSpeed = 10;
    [SerializeField] private float settingIntervalTime = 3f;
    [SerializeField] private float fireLimitTime = 5f;
    [SerializeField] private int countDown = 3;
    private float SetBallTime = 0.4f;
    private float nextFireBallTime = 0f;
    [SerializeField]private bool canFire = false;
    [SerializeField]private bool canCountDown = false;




    void Start()
    {
        faceSpriteRenderer = face.gameObject.GetComponent<SpriteRenderer>();
        faceSpriteRenderer.sprite = defaultSprite;
        countText.text = "";

        currentColor = colors[Random.Range(0, colors.Length)];
        nextColor = colors[Random.Range(0, colors.Length)];

        SetBall();
    }
    public void OnClickBackGround()
    {
        FireBall();
    }
    void Update()
    {
        nextFireBallTime += Time.deltaTime;

        // 마우스 클릭 (BallControaller.Fire() 호출)
        // bool clicked = Mouse.current != null
        //             && Mouse.current.leftButton.wasPressedThisFrame
        //             && canFire;
        // if (clicked) FireBall();

        // 시간이 지났는데 아직도 안눌렀어?
        if (nextFireBallTime > fireLimitTime && canFire == true)
        {
            oneTurn = false;
            

            if (canFire == false)
            {
                return;
            }


            // 공 충돌 구독해제
            FireBall();
            // Invoke("SetBall", settingIntervalTime);
        }
        // 응 시간이 지났어
        

        // 끝나기 ( )초 전이야
        if (nextFireBallTime > fireLimitTime - countDown && canCountDown == true)
        {
            StartCoroutine(CountDownExact(3));
        }
    }

    public bool oneTurn = false;
    public IEnumerator CountDownExact(int seconds, bool unscaled = false)
    {
        canCountDown = false;

        for (int s = seconds; s > 0; s--) // 초당 1회 반응
        {
            Debug.Log(s);
            countText.text = s.ToString();
            // StartCoroutine(PopEffect(rect));

            float t = 1f;
            while (t > 0f) // 여기서 1초를 쪼개서 기다림
            {
                if (oneTurn == false) // 탈출 조건
                {
                    countText.text = "";
                    yield break;   // 코루틴 자체 종료
                }
                // 시간 감소 (스케일드/언스케일드 선택)
                t -= unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;   // 다음 프레임까지 기다림
            }
        }
        oneTurn = false;
        countText.text = "";
        // Invoke("SetBall", settingIntervalTime);
    }

    void realizeHandleBallHitZone()=>currentBall.gameObject.GetComponent<BallController>().OnHitZone -= HandleBallHitZone;
    void HandleBallHitZone(string zoneName, Collider2D zoneCol)
    {
        // 중복 방지: 이미 발사 불가 상태면(=세팅 대기) 무시할 수도 있음


        if (zoneCol.CompareTag("ReturnZone"))
        {
            if (oneTurn == false)
            {
                Invoke("SetBall", settingIntervalTime);
                realizeHandleBallHitZone();
                Debug.Log("구독 해제2");
                return;
            }
            Debug.Log("ReturnZone1");
            ReturnBall(currentBall);
            return;
        }
        
        // 여기선 바로 새 공 세팅
        // 한 턴이 조기종료 된 경우 (다른 공에 맞아서?)
        // else if (zoneCol.CompareTag("Ball"))
        // {
        //     oneTurn = false;
        //     Debug.Log("공맞음");
        //     SetBall();
        //     return;
        // }
        
    }
    public void ReturnBall(GameObject ball)
    {
        
        if (ball == null || ballSpawnPoint == null) return;
        ball.transform.position = ballSpawnPoint.position;
        var rb = ball.GetComponent<Rigidbody2D>();
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }
        // 다시 쏠 수 있게
        canFire = true;
        ball.gameObject.GetComponent<BallController>().launched = false;
    
    }

    public void FireBall()
    {
        currentBall.GetComponent<BallController>().Fire();
        canFire = false;
    }
    
    public void SetBall()
    {
        // 하나의 판(턴)이 시작
        oneTurn = true;

        nextFireBallTime = 0;
        canFire = true;
        canCountDown = true;
        // 이전 공 이벤트 구독 해제
        if(currentBall != null)
        {
            realizeHandleBallHitZone();
            Debug.Log("구독 해제 ");
        }
            
        // 1) 공 소환 + 현재색 적용
        currentBall = Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation, parent.transform);
        InitBall(currentBall);
        ApplyColorToObject(currentBall, currentColor);
        currentBall.gameObject.GetComponent<BallController>().OnHitZone += HandleBallHitZone;
        ApplySpriteByColorToFace(nextColor);

        // 2) 색 파이프라인 밀기 (current ← next, next ← random)
        currentColor = nextColor;
        nextColor = colors[Random.Range(0, colors.Length)];


    }
    public void ApplySpriteByColorToFace(Color color)
    {
        curentSprite =
            color == colors[0] ? sprites[0] :
            color == colors[1] ? sprites[1] :
            color == colors[2] ? sprites[2] :
            color == colors[3] ? sprites[3] : defaultSprite;

        faceSpriteRenderer.sprite = curentSprite;
        StartCoroutine(FadeIn(faceSpriteRenderer,faceAppearDuration));
    }
    public IEnumerator FadeIn(SpriteRenderer sr, float appearDuration)
    {
        float t = 0f;
        Color c = sr.color;
        // 시작 알파 보정

        while (t < appearDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / appearDuration);
            // if (curve != null) u = curve.Evaluate(u);

            c.a = Mathf.Lerp(0, 1, u);
            sr.color = c;
            yield return null;
        }
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
        StartCoroutine(FadeIn(sr,ballAppearDuration));
    }

}
