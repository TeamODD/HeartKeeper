using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    [Header("보드 설정")]
    public int topRowCount = 12;
    public int totalRows = 6;
    public float bubbleSpacing = 1f;
    public float rowOffset = 0.5f;

    [Header("시소 참조")]
    public SeesawLean seesaw;
    public float gapFromSeesaw = 0.3f;

    [Header("구슬 프리팹")]
    public GameObject[] gemPrefabs;

    private List<List<Gem>> allGems = new List<List<Gem>>();
    private bool isInitialBoard = true;

    void Awake()
    {
        RemoveSceneGems(); // 씬에 있는 구슬 제거
    }

    void Start()
    {
        SetupHexBoard();
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
                DestroyImmediate(child.gameObject); // 에디터에서 즉시 제거
#else
                Destroy(child.gameObject); // 런타임에서는 일반 제거
#endif
            }
        }
    }

    void SetupHexBoard()
    {
        allGems.Clear();

        float seesawBottomY = GetSeesawBottomY();
        float startY = seesawBottomY - gapFromSeesaw;

        float gemRadius = bubbleSpacing / 2f;
        float verticalSpacing = gemRadius * Mathf.Sqrt(3); // = bubbleSpacing * 0.866f

        for (int row = 0; row < totalRows; row++)
        {
            int bubblesInRow = topRowCount - row;
            if (bubblesInRow < 1) break;

            float offsetX = (row % 2 == 0) ? 0f : gemRadius;

            List<Gem> gemRow = new List<Gem>();
            for (int col = 0; col < bubblesInRow; col++)
            {
                float posX = (col - (bubblesInRow - 1) / 2f) * bubbleSpacing + offsetX;
                float posY = startY - (row * verticalSpacing);

                Vector3 spawnPos = new Vector3(posX, posY, 0f);
                Gem newGem = SpawnGem(spawnPos);
                gemRow.Add(newGem);
            }
            allGems.Add(gemRow);
        }

        CheckMatches();
    }

    Gem SpawnGem(Vector3 spawnPos)
    {
        int index = Random.Range(0, gemPrefabs.Length);
        GameObject gemObj = Instantiate(gemPrefabs[index], spawnPos, Quaternion.identity);
        gemObj.transform.parent = transform;

        Gem newGem = gemObj.GetComponent<Gem>();

        Rigidbody2D rb = gemObj.GetComponent<Rigidbody2D>();
        if (rb == null) rb = gemObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        return newGem;
    }

    float GetSeesawBottomY()
    {
        if (seesaw == null)
        {
            Debug.LogWarning("SeesawLean이 연결되지 않았습니다. 기본값 Y=0 사용");
            return 0f;
        }

        Collider2D col = seesaw.GetComponent<Collider2D>();
        if (col != null) return col.bounds.min.y;

        SpriteRenderer sr = seesaw.GetComponent<SpriteRenderer>();
        if (sr != null) return sr.bounds.min.y;

        return seesaw.transform.position.y;
    }

    public void CheckMatches()
    {
        if (isInitialBoard) return;

        List<Gem> gemsToDestroy = new List<Gem>();

        for (int i = 0; i < allGems.Count; i++)
        {
            for (int j = 0; j < allGems[i].Count; j++)
            {
                Gem current = allGems[i][j];
                if (current == null) continue;

                if (j > 1 &&
                    allGems[i][j - 1] != null &&
                    allGems[i][j - 2] != null &&
                    allGems[i][j - 1].gemType == current.gemType &&
                    allGems[i][j - 2].gemType == current.gemType)
                {
                    gemsToDestroy.Add(current);
                    gemsToDestroy.Add(allGems[i][j - 1]);
                    gemsToDestroy.Add(allGems[i][j - 2]);
                }
            }
        }

        gemsToDestroy = new HashSet<Gem>(gemsToDestroy).ToList();

        foreach (var gem in gemsToDestroy)
        {
            if (gem != null)
                Destroy(gem.gameObject);
        }
    }
}