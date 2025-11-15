using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [Header("공 속성")]
    [SerializeField] private string color;
    [SerializeField] private Vector2 upWard;

    [Header("효과음 속성")]
    [SerializeField] private AudioClip shotClip;
    [SerializeField] private AudioClip attachedClip;
    [SerializeField] private AudioClip removedClip;
    private AudioSource sfx;
    public AudioSource deleteSfx;

    [Header("발사 속성")]
    [SerializeField] private GameObject arrow;
    public bool launched = false;
    public float launchSpeed;
    public float maxAimAngle = 180;
    public float aimMoveSpeed = 100;

    [Header("색깔별 이펙트 프리팹")]
    public GameObject splashEffectRed;
    public GameObject splashEffectBlue;
    public GameObject splashEffectYellow;
    public GameObject splashEffectGreen;

    private Rigidbody2D rb;
    private bool isRegistered = false;

    public event Action<string, Collider2D> OnHitZone;
    public event Action<string, Collider2D> OnCollisionBall;

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
        if (launched)
        {
            arrow.SetActive(false);

            if (!isRegistered && rb.linearVelocity.magnitude < 0.01f)
            {
                TryRegisterToBoard();
            }

            return;
        }
        else
        {
            arrow.SetActive(true);
            transform.rotation = Quaternion.Euler(0f, 0f, GetWiperAngle());
        }
    }

    float GetWiperAngle()
    {
        float half = maxAimAngle / 2;
        if (maxAimAngle <= 0f || aimMoveSpeed <= 0f) return 0f;

        float phase01 = Mathf.PingPong(Time.time * (aimMoveSpeed / (2f * half)), 1f);
        return Mathf.Lerp(-half, half, phase01);
    }

    public void Fire()
    {
        if (launched) return;

        if (shotClip != null)
            sfx.PlayOneShot(shotClip);

        rb.AddForce(transform.up * launchSpeed, ForceMode2D.Impulse);
        launched = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name.Contains("OutZone"))
            gameObject.SetActive(false);

        OnHitZone?.Invoke(other.name, other);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("💥 충돌체 감지됨");
        OnHitZone?.Invoke(col.collider.name, col.collider);
    }

    void TryRegisterToBoard()
    {
        if (isRegistered) return;
        if (rb.linearVelocity.magnitude > 0.01f) return;

        var boardManager = FindObjectOfType<BoardManager>();
        if (boardManager == null)
        {
            Debug.LogWarning("🚫 BoardManager를 찾을 수 없습니다.");
            return;
        }

        boardManager.OnGemAttached(gameObject);
        isRegistered = true;

        var gem = GetComponent<Gem>();
        if (gem != null)
        {
            Debug.Log($"✅ BoardManager에 등록됨: 위치({transform.position}), 색상={gem.gemType}");
            CheckMatchAndDestroy(gem);
        }
        else
        {
            Debug.LogWarning("⚠️ Gem 컴포넌트를 찾을 수 없어 색상 정보를 확인할 수 없습니다.");
        }
    }

    void CheckMatchAndDestroy(Gem selfGem)
    {
        float matchRadius = 1.1f;
        List<Gem> group = new List<Gem>();
        Queue<Gem> queue = new Queue<Gem>();
        HashSet<Gem> visited = new HashSet<Gem>();

        queue.Enqueue(selfGem);
        visited.Add(selfGem);

        while (queue.Count > 0)
        {
            Gem current = queue.Dequeue();
            group.Add(current);

            Collider2D[] hits = Physics2D.OverlapCircleAll(current.transform.position, matchRadius);
            foreach (var hit in hits)
            {
                Gem neighbor = hit.GetComponent<Gem>();
                if (neighbor == null || visited.Contains(neighbor)) continue;
                if (neighbor.gemType != selfGem.gemType) continue;

                float dist = Vector2.Distance(current.transform.position, neighbor.transform.position);
                if (dist <= matchRadius)
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (group.Count >= 3)
        {
            Debug.Log($"💥 연결된 같은 색 구슬 {group.Count}개 → 삭제");

            var board = FindObjectOfType<BoardManager>();

            foreach (var gem in group)
            {
                deleteSfx.Play();
                SpawnEffect(gem.gemType, gem.transform.position);
            }

            foreach (var gem in group)
            {
                board?.RemoveGem(gem);
                Destroy(gem.gameObject);
            }
        }
    }

    void SpawnEffect(GemType type, Vector3 position)
    {
        GameObject prefab = GetEffectPrefab(type);
        if (prefab != null)
        {
            Instantiate(prefab, position, Quaternion.identity);

            // ✅ 이펙트 위치에서 삭제 효과음 재생
            if (removedClip != null)
            {
                // AudioSource.PlayClipAtPoint(removedClip, position);
                deleteSfx.Play();
                Debug.Log("🔊 삭제 효과음 재생됨 (이펙트 위치)");
            }
        }
        else
        {
            Debug.LogWarning($"🚫 이펙트 프리팹이 없습니다: {type}");
        }
    }

    GameObject GetEffectPrefab(GemType type)
    {
        switch (type)
        {
            case GemType.Red: return splashEffectRed;
            case GemType.Blue: return splashEffectBlue;
            case GemType.Yellow: return splashEffectYellow;
            case GemType.Green: return splashEffectGreen;
            default: return null;
        }
    }
}