using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    [Header("보드 설정")]
    public int topRowCount = 13;
    public int totalRows = 6;
    public float bubbleSpacing = 1f;
    public float rowOffset = 0.5f;
    public float verticalSpacing = 1f;

    [Header("시소 참조")]
    public SeesawLean seesaw;
    public float gapFromSeesaw = 0.3f;

    [Header("구슬 프리팹")]
    public GameObject[] gemPrefabs;

    private List<List<Gem>> allGems = new List<List<Gem>>();
    private bool isInitialBoard = true;

    void Awake()
    {
        RemoveSceneGems();
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
                DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
        }
    }

    void SetupHexBoard()
    {
        allGems.Clear();

        float seesawBottomY = GetSeesawBottomY();
        float startY = seesawBottomY - gapFromSeesaw;

        int[] rowGemCounts = new int[] { 13, 12, 0, 10, 0, 8 };

        for (int row = 0; row < rowGemCounts.Length; row++)
        {
            int bubblesInRow = rowGemCounts[row];
            List<Gem> gemRow = new List<Gem>();

            if (bubblesInRow == 0)
            {
                allGems.Add(gemRow);
                continue;
            }

            float rowWidth = (bubblesInRow - 1f) * bubbleSpacing;
            float startX = -rowWidth / 2f;

            for (int col = 0; col < bubblesInRow; col++)
            {
                float posX = startX + col * bubbleSpacing;
                float posY = startY - row * verticalSpacing;
                Vector3 spawnPos = new Vector3(posX, posY, 0f);

                // 구슬 타입 중복 연속 3개 방지 로직
                int prefabIndex = 0;
                GemType candidateType;
                while (true)
                {
                    prefabIndex = Random.Range(0, gemPrefabs.Length);
                    GameObject prefab = gemPrefabs[prefabIndex];
                    Gem prefabGem = prefab.GetComponent<Gem>();
                    if (prefabGem == null)
                    {
                        Debug.LogError("gemPrefabs[" + prefabIndex + "]에 Gem 컴포넌트가 없습니다.");
                        continue;
                    }
                    candidateType = prefabGem.gemType;

                    if (col > 1 &&
                        gemRow[col - 1] != null &&
                        gemRow[col - 2] != null &&
                        gemRow[col - 1].gemType == candidateType &&
                        gemRow[col - 2].gemType == candidateType)
                    {
                        // 바로 앞 두 구슬과 같은 타입이면 다시 뽑기
                        continue;
                    }
                    break;
                }

                GameObject gemObj = Instantiate(gemPrefabs[prefabIndex], spawnPos, Quaternion.identity);
                gemObj.transform.parent = transform;

                Gem newGem = gemObj.GetComponent<Gem>();
                if (newGem == null)
                {
                    Debug.LogError("생성된 구슬에 Gem 컴포넌트가 없습니다.");
                    continue;
                }
                gemRow.Add(newGem);

                Rigidbody2D rb = gemObj.GetComponent<Rigidbody2D>();
                if (rb == null) rb = gemObj.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }

            allGems.Add(gemRow);
        }

        CheckMatches();
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
            {
                Destroy(gem.gameObject);
            }
        }
    }
}
