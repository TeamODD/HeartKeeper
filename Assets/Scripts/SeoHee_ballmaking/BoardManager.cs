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

    public List<List<Gem>> allGems = new List<List<Gem>>();
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

                int prefabIndex = Random.Range(0, gemPrefabs.Length);
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

    // 발사된 구슬이 보드에 붙을 때 호출
    public void RegisterGemAt(int row, int col, Gem gem)
    {
        Debug.Log($"[등록] 구슬 등록됨: ({row}, {col}) 타입: {gem.gemType}");

        while (allGems.Count <= row)
            allGems.Add(new List<Gem>());

        while (allGems[row].Count <= col)
            allGems[row].Add(null);

        allGems[row][col] = gem;

        CheckMatchesFromGem(row, col);
    }

    public void CheckMatchesFromGem(int startRow, int startCol)
    {
        if (isInitialBoard) return;
        if (allGems[startRow][startCol] == null) return;

        GemType type = allGems[startRow][startCol].gemType;
        bool[,] visited = new bool[allGems.Count, allGems.Max(r => r.Count)];
        List<Vector2Int> group = GetConnectedGroup(startRow, startCol, type, visited);

        Debug.Log($"[검사] 연결된 그룹 크기: {group.Count}");

        if (group.Count >= 3)
        {
            foreach (var pos in group)
            {
                Gem gem = allGems[pos.x][pos.y];
                if (gem != null)
                {
                    Debug.Log($"[삭제] 구슬 제거됨: ({pos.x}, {pos.y}) 타입: {gem.gemType}");
                    allGems[pos.x][pos.y] = null;
#if UNITY_EDITOR
                    DestroyImmediate(gem.gameObject);
#else
                    Destroy(gem.gameObject);
#endif
                }
            }
        }
    }

    // 전방향 탐색 (8방향)
    List<Vector2Int> GetConnectedGroup(int startRow, int startCol, GemType type, bool[,] visited)
    {
        List<Vector2Int> group = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startRow, startCol));
        visited[startRow, startCol] = true;

        int[][] directions = new int[][]
        {
            new int[]{-1, 0}, new int[]{1, 0},
            new int[]{0, -1}, new int[]{0, 1},
            new int[]{-1, -1}, new int[]{-1, 1},
            new int[]{1, -1}, new int[]{1, 1}
        };

        while (queue.Count > 0)
        {
            Vector2Int pos = queue.Dequeue();
            group.Add(pos);

            foreach (var dir in directions)
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