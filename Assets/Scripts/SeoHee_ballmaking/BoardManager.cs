using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    [Header("보드 설정")]
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
#if UNITY_EDITOR
                DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
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

                // 구슬 타입 중복 3개 방지
                int prefabIndex = 0;
                GemType candidateType;
                while (true)
                {
                    prefabIndex = Random.Range(0, gemPrefabs.Length);
                    GameObject prefab = gemPrefabs[prefabIndex];
                    Gem prefabGem = prefab.GetComponent<Gem>();
                    if (prefabGem == null) continue;

                    candidateType = prefabGem.gemType;

                    if (col > 1 &&
                        gemRow[col - 1] != null &&
                        gemRow[col - 2] != null &&
                        gemRow[col - 1].gemType == candidateType &&
                        gemRow[col - 2].gemType == candidateType)
                    {
                        continue;
                    }
                    break;
                }

                GameObject gemObj = Instantiate(gemPrefabs[prefabIndex], spawnPos, Quaternion.identity);
                gemObj.transform.parent = transform;

                Gem newGem = gemObj.GetComponent<Gem>();
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
        if (seesaw == null) return 0f;

        Collider2D col = seesaw.GetComponent<Collider2D>();
        if (col != null) return col.bounds.min.y;

        SpriteRenderer sr = seesaw.GetComponent<SpriteRenderer>();
        if (sr != null) return sr.bounds.min.y;

        return seesaw.transform.position.y;
    }

    // --------------------- 여기부터 3개 이상 그룹 삭제 ---------------------
    public void CheckMatches()
    {
        if (isInitialBoard) return;

        HashSet<Gem> gemsToDestroy = new HashSet<Gem>();
        bool[,] visited = new bool[allGems.Count, allGems.Max(r => r.Count)];

        for (int row = 0; row < allGems.Count; row++)
        {
            for (int col = 0; col < allGems[row].Count; col++)
            {
                if (allGems[row][col] == null || visited[row, col]) continue;

                List<Vector2Int> group = GetConnectedGroup(row, col, allGems[row][col].gemType, visited);
                if (group.Count >= 3)
                {
                    foreach (var pos in group)
                        gemsToDestroy.Add(allGems[pos.x][pos.y]);
                }
            }
        }

        foreach (var gem in gemsToDestroy)
        {
            if (gem != null)
            {
                Destroy(gem.gameObject);
            }
        }
    }

    // BFS/DFS로 연결된 같은 색 구슬 그룹 찾기
    List<Vector2Int> GetConnectedGroup(int startRow, int startCol, GemType type, bool[,] visited)
    {
        List<Vector2Int> group = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startRow, startCol));
        visited[startRow, startCol] = true;

        int[][] directionsEven = new int[][]
        {
            new int[]{0,1}, new int[]{1,0}, new int[]{1,-1}, new int[]{0,-1}, new int[]{-1,-1}, new int[]{-1,0}
        };
        int[][] directionsOdd = new int[][]
        {
            new int[]{0,1}, new int[]{1,1}, new int[]{1,0}, new int[]{0,-1}, new int[]{-1,0}, new int[]{-1,1}
        };

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();
            group.Add(pos);

            int[][] dirs = (pos.x % 2 == 0) ? directionsEven : directionsOdd;

            foreach (var dir in dirs)
            {
                int newRow = pos.x + dir[0];
                int newCol = pos.y + dir[1];

                if (newRow < 0 || newRow >= allGems.Count) continue;
                if (newCol < 0 || newCol >= allGems[newRow].Count) continue;
                if (visited[newRow, newCol]) continue;
                if (allGems[newRow][newCol] == null) continue;
                if (allGems[newRow][newCol].gemType != type) continue;

                visited[newRow, newCol] = true;
                queue.Enqueue(new Vector2Int(newRow, newCol));
            }
        }

        return group;
    }
}
