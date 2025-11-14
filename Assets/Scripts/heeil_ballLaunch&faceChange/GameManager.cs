using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("공 이미지 속성")]
    [SerializeField] private Sprite[] ballSprites;
    [SerializeField] private float ballAppearDuration = 0.5f;

    [Header("초상화 속성")]
    [SerializeField] private GameObject face;
    private SpriteRenderer faceSpriteRenderer;
    [SerializeField] private Sprite[] faceSprites;
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
    [SerializeField] private float fireLimitTime = 3f;
    [SerializeField] private int countDown = 3;

    private int currentIndex;
    private int nextIndex;

    private float nextFireBallTime = 0f;
    private bool canFire = false;
    private bool canCountDown = false;
    private bool oneTurn = false;

    private List<GameObject> attachedBalls = new List<GameObject>();

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

        if (nextFireBallTime > fireLimitTime)
        {
            Debug.Log("⏱ 자동 발사됨!");
            FireBall();
        }

        if (nextFireBallTime > fireLimitTime - countDown && canCountDown)
        {
            StartCoroutine(CountDownExact(countDown));
        }
    }

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

    void UnsubscribeBallEvent()
    {
        if (currentBall != null)
        {
            var bc = currentBall.GetComponent<BallController>();
            if (bc != null)
                bc.OnHitZone -= HandleBallHitZone;
        }
    }

    void HandleBallHitZone(string zoneName, Collider2D zoneCol)
    {
        if (zoneCol.CompareTag("Ball"))
        {
            if (!attachedBalls.Contains(currentBall))
            {
                attachedBalls.Add(currentBall);

                var gem = currentBall.GetComponent<Gem>();
                string colorInfo = gem != null ? gem.gemType.ToString() : "Unknown";
                Vector3 pos = currentBall.transform.position;

                Debug.Log($"📌 고정된 공 등록됨 — 위치({pos.x:F2}, {pos.y:F2}), 색상={colorInfo}, 총 개수: {attachedBalls.Count}");
            }

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

    public void FireBall()
    {
        if (currentBall == null)
        {
            Debug.LogWarning("🚫 발사 실패: currentBall이 없음");
            return;
        }

        nextFireBallTime = 0;
        canCountDown = false;
        StopAllCoroutines();

        var bc = currentBall.GetComponent<BallController>();
        if (bc == null)
        {
            Debug.LogWarning("🚫 발사 실패: BallController 없음");
            return;
        }

        bc.Fire();
        canFire = false;
        Debug.Log("💥 공 발사!");

        StartCoroutine(ReloadAfterDelay(fireLimitTime));
    }

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

        Sprite selectedSprite = ballSprites[currentIndex];
        ApplySpriteToBall(currentBall, selectedSprite);

        var gem = currentBall.GetComponent<Gem>();
        if (gem != null)
        {
            gem.gemType = GetGemTypeFromSprite(selectedSprite);
            Debug.Log($"🎨 GemType 설정됨: {gem.gemType} (이미지: {selectedSprite.name})");
        }
        else
        {
            Debug.LogWarning("⚠️ Gem 컴포넌트를 찾을 수 없습니다 — 색상 설정 실패");
        }

        var bc = currentBall.GetComponent<BallController>();
        bc.OnHitZone += HandleBallHitZone;

        ApplySpriteToFace(faceSprites[nextIndex]);

        currentIndex = nextIndex;
        nextIndex = Random.Range(0, ballSprites.Length);
    }

    public GemType GetGemTypeFromSprite(Sprite sprite)
    {
        if (sprite == null) return GemType.Red;

        string name = sprite.name.ToLower();

        if (name.Contains("surpris")) return GemType.Green;
        if (name.Contains("angry")) return GemType.Red;
        if (name.Contains("sad")) return GemType.Blue;
        if (name.Contains("happy")) return GemType.Yellow;

        return GemType.Red;
    }

    public void ApplySpriteToBall(GameObject ball, Sprite sprite)
    {
        if (ball.TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.sprite = sprite;
            StartCoroutine(FadeIn(sr, ballAppearDuration));
        }
    }

    public void ApplySpriteToFace(Sprite sprite)
    {
        if (sprite == null)
            sprite = defaultFaceSprite;

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
            c.a = Mathf.Lerp(0f, 1f, t / appearDuration);
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

    public IEnumerator ReloadAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        oneTurn = false;
        UnsubscribeBallEvent();

        SetBall();
    }
}