using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("공 이미지 속성")]
    [SerializeField] private Sprite[] ballSprites;     // 공의 스프라이트 배열
    [SerializeField] private float ballAppearDuration = 0.5f;

    [Header("초상화 속성")]
    [SerializeField] private GameObject face;          // face 오브젝트 (SpriteRenderer 있음)
    private SpriteRenderer faceSpriteRenderer;
    [SerializeField] private Sprite[] faceSprites;     // 얼굴용 스프라이트 배열
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
    [SerializeField] private float settingIntervalTime = 1.0f;
    [SerializeField] private float fireLimitTime = 3f;   // 3초 동안 입력 없으면 자동 발사
    [SerializeField] private int countDown = 3;

    private int currentIndex;
    private int nextIndex;

    private float nextFireBallTime = 0f;
    private bool canFire = false;
    private bool canCountDown = false;
    private bool oneTurn = false;

    void Start()
    {
        faceSpriteRenderer = face.GetComponent<SpriteRenderer>();
        faceSpriteRenderer.sprite = defaultFaceSprite;
        countText.text = "";

        currentIndex = Random.Range(0, ballSprites.Length);
        nextIndex = Random.Range(0, ballSprites.Length);

        SetBall();
    }

    void Update()
    {
        if (!canFire) return;

        nextFireBallTime += Time.deltaTime;

        // 3초 동안 아무 입력 없으면 자동 발사
        if (nextFireBallTime > fireLimitTime)
        {
            Debug.Log("⏱ 자동 발사됨!");
            FireBall();
        }

        // 발사 전 카운트다운 표시
        if (nextFireBallTime > fireLimitTime - countDown && canCountDown)
        {
            StartCoroutine(CountDownExact(countDown));
        }
    }

    // 클릭 시 즉시 발사
    public void OnClickBackGround()
    {
        if (canFire && currentBall != null)
        {
            Debug.Log("🎯 클릭 발사됨!");
            FireBall();
        }
    }

    public IEnumerator CountDownExact(int seconds, bool unscaled = false)
    {
        canCountDown = false;
        for (int s = seconds; s > 0; s--)
        {
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
        countText.text = "";
    }

    // 🔹 이벤트 구독 해제 함수
    void UnsubscribeBallEvent()
    {
        if (currentBall != null)
        {
            var bc = currentBall.GetComponent<BallController>();
            if (bc != null)
                bc.OnHitZone -= HandleBallHitZone;
        }
    }

    // 🔹 충돌 처리 (BallController에서 OnHitZone 이벤트로 호출)
    void HandleBallHitZone(string zoneName, Collider2D zoneCol)
    {
        if (zoneCol.CompareTag("Ball"))
        {
            Debug.Log("🎯 공에 충돌 — 다음 공 준비");
            oneTurn = false;
            UnsubscribeBallEvent();
            Invoke(nameof(SetBall), settingIntervalTime);
            return;
        }

        if (zoneCol.CompareTag("ReturnZone"))
        {
            Debug.Log("↩ ReturnZone 감지 — 공 되돌림");
            ReturnBall(currentBall);
            return;
        }
    }

    // 🔹 공 되돌리기
    public void ReturnBall(GameObject ball)
    {
        if (ball == null || ballSpawnPoint == null) return;

        ball.transform.position = ballSpawnPoint.position;

        var rb = ball.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        canFire = true;
        ball.GetComponent<BallController>().launched = false;
    }

    // 🔹 공 발사
    public void FireBall()
    {
        if (currentBall == null)
        {
            Debug.LogWarning("🚫 발사 실패: currentBall이 없음");
            return;
        }

        var bc = currentBall.GetComponent<BallController>();
        if (bc == null)
        {
            Debug.LogWarning("🚫 발사 실패: BallController 없음");
            return;
        }

        bc.Fire();
        canFire = false;
        Debug.Log("💥 공 발사!");
    }

    // 🔹 새 공 세팅
    public void SetBall()
    {
        oneTurn = true;
        nextFireBallTime = 0f;
        canFire = true;
        canCountDown = true;

        if (currentBall != null)
        {
            UnsubscribeBallEvent();
            Debug.Log("이전 공 이벤트 구독 해제 완료");
        }

        currentBall = Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation, parent.transform);
        InitBall(currentBall);
        ApplySpriteToBall(currentBall, ballSprites[currentIndex]);

        var bc = currentBall.GetComponent<BallController>();
        bc.OnHitZone += HandleBallHitZone;

        ApplySpriteToFace(faceSprites[nextIndex]);

        currentIndex = nextIndex;
        nextIndex = Random.Range(0, ballSprites.Length);
    }

    // 🔹 공에 이미지 적용
    public void ApplySpriteToBall(GameObject ball, Sprite sprite)
    {
        if (ball.TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.sprite = sprite;
            StartCoroutine(FadeIn(sr, ballAppearDuration));
        }
    }

    // 🔹 얼굴 이미지 적용
    public void ApplySpriteToFace(Sprite sprite)
    {
        if (sprite == null)
            sprite = defaultFaceSprite;

        faceSpriteRenderer.sprite = sprite;
        StartCoroutine(FadeIn(faceSpriteRenderer, faceAppearDuration));
    }

    // 🔹 페이드 인 효과
    public IEnumerator FadeIn(SpriteRenderer sr, float appearDuration)
    {
        float t = 0f;
        Color c = sr.color;
        c.a = 0f;
        sr.color = c;

        while (t < appearDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / appearDuration);
            sr.color = c;
            yield return null;
        }
    }

    // 🔹 공 초기화
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
