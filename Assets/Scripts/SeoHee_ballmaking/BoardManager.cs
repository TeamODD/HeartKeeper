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

    public GameObject bubble;

    void Awake()
    {
        RemoveSceneGems();
    }

    void Start()
    {
        SetupHexBoard();
        isInitialBoard = false;
    }

    //private void FixedUpdate()
    //{
    //    GameObject curBubble = bubble.transform.GetChild(0).gameObject;
    //    if (curBubble != null)
    //    {
    //        Invoke("DelayTime", 0.5f);
    //        OnGemAttached(curBubble);
    //    }
    //}
    //private void DelayTime()
    //{

    //}

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

    // 충돌 혹은 붙음이 발생했을 때 이 메서드를 호출하도록 BoardManager가 직접 관리
    public void OnGemAttached(GameObject gemObj)
    {
        if (isInitialBoard) return;

        Gem gem = gemObj.GetComponent<Gem>();
        if (gem == null) return;

        Vector3 worldPos = gemObj.transform.position;

        float seesawBottomY = GetSeesawBottomY();
        float startY = seesawBottomY - gapFromSeesaw;

        int row = Mathf.RoundToInt((startY - worldPos.y) / verticalSpacing);
        if (row < 0 || row >= allGems.Count) return;

        int bubblesInRow = allGems[row].Count;
        if (bubblesInRow == 0) return;

        float rowWidth = (bubblesInRow - 1f) * bubbleSpacing;
        float startX = -rowWidth / 2f;
        float xOffset = worldPos.x - startX;
        int col = Mathf.RoundToInt(xOffset / bubbleSpacing);

        if (col < 0 || col >= bubblesInRow) return;

        Debug.Log($"[등록] 구슬 위치: world({worldPos.x:F2},{worldPos.y:F2}) → (row={row}, col={col}), 타입={gem.gemType}");

        allGems[row][col] = gem;

        CheckMatchesFromGem(row, col);
    }

    public void CheckMatchesFromGem(int startRow, int startCol)
    {
        Gem startGem = allGems[startRow][startCol];
        if (startGem == null) return;

        GemType type = startGem.gemType;

        int maxCols = allGems.Max(r => r.Count);
        bool[,] visited = new bool[allGems.Count, maxCols];

        List<Vector2Int> group = GetConnectedGroup(startRow, startCol, type, visited);

        Debug.Log($"[검사] 연결된 그룹 크기: {group.Count} 타입={type}");

        if (group.Count >= 3)
        {
            foreach (var pos in group)
            {
                int r = pos.x;
                int c = pos.y;
                if (r < 0 || r >= allGems.Count) continue;
                if (c < 0 || c >= allGems[r].Count) continue;

                Gem gem = allGems[r][c];
                if (gem != null)
                {
                    Debug.Log($"[삭제] 구슬 제거됨: (row={r}, col={c}) 타입={gem.gemType}");
                    allGems[r][c] = null;
                    Destroy(gem.gameObject);
                }
            }
        }
    }

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

                Gem neighbor = allGems[newRow][newCol];
                if (neighbor == null) continue;
                if (neighbor.gemType != type) continue;

                visited[newRow, newCol] = true;
                queue.Enqueue(new Vector2Int(newRow, newCol));
            }
        }

        return group;
    }
}
