using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    [Header("보드 설정")]
    public int totalRows = 6;
    public float bubbleSpacing = 1f;
    public float verticalSpacing = 1f;
    public float gapFromSeesaw = 0.3f;

    [Header("시소 참조")]
    public SeesawLean seesaw;

    [Header("구슬 프리팹")]
    public GameObject[] gemPrefabs;

    public List<Gem> allGems = new List<Gem>();
    private bool isInitialBoard = true;

    void Awake()
    {
        RemoveSceneGems();
    }

    void Start()
    {
        SetupBoardWithoutMatches();
        isInitialBoard = false;
    }

    void RemoveSceneGems()
    {
        foreach (Transform child in transform)
        {
            Gem gem = child.GetComponent<Gem>();
            if (gem != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
        }
    }

    void SetupBoardWithoutMatches()
    {
        allGems.Clear();
        float seesawBottomY = GetSeesawBottomY();
        float startY = seesawBottomY - gapFromSeesaw;
        int[] rowGemCounts = new int[] { 13, 12, 0, 10, 0, 8 };

        for (int row = 0; row < rowGemCounts.Length; row++)
        {
            int count = rowGemCounts[row];
            if (count == 0) continue;

            float rowWidth = (count - 1f) * bubbleSpacing;
            float startX = -rowWidth / 2f;

            for (int col = 0; col < count; col++)
            {
                Vector3 pos = new Vector3(startX + col * bubbleSpacing, startY - row * verticalSpacing, 0f);
                int safeIndex = GetSafeGemIndex(pos);
                GameObject gemObj = Instantiate(gemPrefabs[safeIndex], pos, Quaternion.identity);
                gemObj.transform.parent = transform;

                Gem gem = gemObj.GetComponent<Gem>();
                allGems.Add(gem);

                Rigidbody2D rb = gemObj.GetComponent<Rigidbody2D>();
                if (rb == null) rb = gemObj.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
    }

    int GetSafeGemIndex(Vector3 spawnPos)
    {
        List<int> candidates = new List<int>();
        for (int i = 0; i < gemPrefabs.Length; i++) candidates.Add(i);

        while (candidates.Count > 0)
        {
            int index = candidates[Random.Range(0, candidates.Count)];
            GemType type = gemPrefabs[index].GetComponent<Gem>().gemType;

            if (!WouldFormMatch(spawnPos, type))
                return index;

            candidates.Remove(index);
        }

        return Random.Range(0, gemPrefabs.Length); // fallback
    }

    bool WouldFormMatch(Vector3 pos, GemType type)
    {
        int nearbySameType = 0;
        float matchRadius = bubbleSpacing * 1.1f;

        foreach (var gem in allGems)
        {
            if (gem == null || gem.gemType != type) continue;

            float dist = Vector2.Distance(pos, gem.transform.position);
            if (dist <= matchRadius)
            {
                nearbySameType++;
                if (nearbySameType >= 2) return true;
            }
        }

        return false;
    }

    float GetSeesawBottomY()
    {
        if (seesaw == null) return 0f;

        Collider2D col = seesaw.GetComponent<Collider2D>();
        if (col != null) return col.bounds.min.y;

        SpriteRenderer sr = seesaw.GetComponent<SpriteRenderer>();
        if (sr != null) return sr.bounds.min.y;

        return seesaw.transform.position.y;
    }

    public void OnGemAttached(GameObject gemObj)
    {
        if (isInitialBoard) return;

        Gem gem = gemObj.GetComponent<Gem>();
        if (gem == null) return;

        allGems.Add(gem);
    }

    public void RemoveGem(Gem gem)
    {
        allGems.Remove(gem);
    }
}