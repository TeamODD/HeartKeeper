using System.Collections;
using UnityEngine;
using TMPro;

public class GameManagerRe : MonoBehaviour
{
    [Header("색깔 속성")]
    [SerializeField] private Color[] colors = { Color.red, Color.yellow, Color.green, Color.blue };
    private Color currentColor;
    private Color nextColor;

    [Header("공 이미지 속성")]
    [SerializeField] private Sprite[] ballSprites;           // colors 순서대로 공에 사용할 스프라이트 배열
    [SerializeField] private float ballAppearDuration = 0.5f;

    [Header("초상화 속성")]
    [SerializeField] private GameObject face;
    private SpriteRenderer faceSpriteRenderer;
    [SerializeField] private Sprite[] faceSprites;           // colors 순서대로 얼굴 이미지 스프라이트 배열
    [SerializeField] private Sprite defaultFaceSprite;
    [SerializeField] private float faceAppearDuration = 1f;

    [Header("소환 속성")]
    [SerializeField] private Transform ballSpawnPoint;
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject parent;

    [Header("발사 속성")]
    [SerializeField] private GameObject clickPanel;
    [SerializeField] private GameObject currentBall;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private float maxAimAngle = 120f;
    [SerializeField] private float aimMoveSpeed = 50f;
    [SerializeField] private float launchSpeed = 10f;
    [SerializeField] private float settingIntervalTime = 3f;
    [SerializeField] private float fireLimitTime = 5f;
    [SerializeField] private int countDown = 3;

    private float nextFireBallTime = 0f;
    private bool canFire = false;
    private bool canCountDown = false;
    public bool oneTurn = false;

    private void Start()
    {
        faceSpriteRenderer = face.GetComponent<SpriteRenderer>();
        faceSpriteRenderer.sprite = defaultFaceSprite;
        countText.text = "";

        currentColor = colors[Random.Range(0, colors.Length)];
        nextColor = colors[Random.Range(0, colors.Length)];

        SetBall();
    }

    private void Update()
    {
        nextFireBallTime += Time.deltaTime;

        if (nextFireBallTime > fireLimitTime && canFire)
        {
            oneTurn = false;
            FireBall();
        }

        if (nextFireBallTime > fireLimitTime - countDown && canCountDown)
        {
            StartCoroutine(CountDownExact(countDown));
        }
    }

    public IEnumerator CountDownExact(int seconds, bool unscaled = false)
    {
        canCountDown = false;

        for (int s = seconds; s > 0; s--)
        {
            Debug.Log(s);
            countText.text = s.ToString();

            float t = 1f;
            while (t > 0f)
            {
                if (!oneTurn)
                {
                    countText.text = "";
                    yield break;
                }
                t -= unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }
        }

        oneTurn = false;
        countText.text = "";
    }

    void realizeHandleBallHitZone()
    {
        if (currentBall != null)
        {
            currentBall.GetComponent<BallController>().OnHitZone -= HandleBallHitZone;
        }
    }

    void HandleBallHitZone(string zoneName, Collider2D zoneCol)
    {
        if (zoneCol.CompareTag("ReturnZone"))
        {
            if (!oneTurn)
            {
                Invoke(nameof(SetBall), settingIntervalTime);
                realizeHandleBallHitZone();
                Debug.Log("구독 해제2");
                return;
            }
            Debug.Log("ReturnZone1");
            ReturnBall(currentBall);
            return;
        }
        // 필요시 다른 구역 로직 추가
    }

    public void ReturnBall(GameObject ball)
    {
        if (ball == null || ballSpawnPoint == null) return;

        ball.transform.position = ballSpawnPoint.position;
        var rb = ball.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        canFire = true;
        ball.GetComponent<BallController>().launched = false;
    }

    public void FireBall()
    {
        if (currentBall != null)
        {
            currentBall.GetComponent<BallController>().Fire();
            canFire = false;
        }
    }

    public void SetBall()
    {
        oneTurn = true;
        nextFireBallTime = 0f;
        canFire = true;
        canCountDown = true;

        if (currentBall != null)
        {
            realizeHandleBallHitZone();
            Debug.Log("구독 해제 ");
        }

        currentBall = Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation, parent.transform);
        InitBall(currentBall);

        ApplySpriteToBall(currentBall, currentColor);

        currentBall.GetComponent<BallController>().OnHitZone += HandleBallHitZone;

        ApplySpriteByColorToFace(nextColor);

        currentColor = nextColor;
        nextColor = colors[Random.Range(0, colors.Length)];
    }

    public void ApplySpriteToBall(GameObject ball, Color color)
    {
        if (ball.TryGetComponent<SpriteRenderer>(out var sr))
        {
            Sprite sprite = null;
            for (int i = 0; i < colors.Length; i++)
            {
                if (color == colors[i] && i < ballSprites.Length)
                {
                    sprite = ballSprites[i];
                    break;
                }
            }
            if (sprite == null)
            {
                Debug.LogWarning("매칭되는 공 스프라이트가 없습니다. 기본값 사용");
                sprite = ballSprites.Length > 0 ? ballSprites[0] : null;
            }
            sr.sprite = sprite;
            StartCoroutine(FadeIn(sr, ballAppearDuration));
        }
    }

    public void ApplySpriteByColorToFace(Color color)
    {
        Sprite sprite = null;
        for (int i = 0; i < colors.Length; i++)
        {
            if (color == colors[i] && i < faceSprites.Length)
            {
                sprite = faceSprites[i];
                break;
            }
        }
        if (sprite == null)
        {
            sprite = defaultFaceSprite;
        }

        faceSpriteRenderer.sprite = sprite;
        StartCoroutine(FadeIn(faceSpriteRenderer, faceAppearDuration));
    }

    public IEnumerator FadeIn(SpriteRenderer sr, float appearDuration)
    {
        float t = 0f;
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        while (t < appearDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / appearDuration);
            c.a = Mathf.Lerp(0f, 1f, u);
            sr.color = c;
            yield return null;
        }
    }

    public void InitBall(GameObject ball)
    {
        if (ball.TryGetComponent<BallController>(out var bc))
        {
            bc.maxAimAngle = maxAimAngle;
            bc.aimMoveSpeed = aimMoveSpeed;
            bc.launchSpeed = launchSpeed;
        }
    }
}
