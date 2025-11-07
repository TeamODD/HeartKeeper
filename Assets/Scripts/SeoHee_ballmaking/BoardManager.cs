using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BoardManager : MonoBehaviour
{
    [Header("보드 설정")]
    public int topRowCount = 12;       // 맨 위 줄 구슬 개수
    public int totalRows = 6;          // 전체 줄 수
    public float bubbleSpacing = 1f;   // 구슬 간 간격
    public float startYOffset = 5f;    // 보드 위치 높이

    [Header("구슬 프리팹")]
    public GameObject[] gemPrefabs;

    private Gem[,] allGems;
    private bool isInitialBoard = true; // 초기 세팅 구분용

    void Start()
    {
        SetupTrapezoidBoard();
        isInitialBoard = false; // 초기 생성 후 false 전환
    }

    // 사다리꼴 형태 보드 생성
    void SetupTrapezoidBoard()
    {
        allGems = new Gem[topRowCount, totalRows];
        float startY = startYOffset;

        for (int row = 0; row < totalRows; row++)
        {
            int bubblesInRow = topRowCount - row * 2;
            if (bubblesInRow < 1) break;

            float rowWidth = (bubblesInRow - 1) * bubbleSpacing;
            float startX = -rowWidth / 2f;

            for (int col = 0; col < bubblesInRow; col++)
            {
                SpawnGem(col, row, startX, startY);
            }
        }

        // 초기 세팅 후에도 혹시 매칭 생겼는지 점검
        // (사라지지는 않지만, 이후 매칭용 로직은 위해 확인 가능)
        CheckMatches();
    }

    // 구슬 생성 함수 (중력X, 매칭 방지)
    void SpawnGem(int col, int row, float startX, float startY)
    {
        int index = Random.Range(0, gemPrefabs.Length);
        Vector3 spawnPos = new Vector3(startX + col * bubbleSpacing, startY - row * bubbleSpacing, 0);
        GameObject gemObj = Instantiate(gemPrefabs[index], spawnPos, Quaternion.identity);
        gemObj.transform.parent = transform;

        Gem newGem = gemObj.GetComponent<Gem>();
        allGems[col, row] = newGem;

        // 물리 속성: 천장 고정
        Rigidbody2D rb = gemObj.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gemObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // 초기 세팅 중에는 3개 이상 연속 방지
        while (isInitialBoard && MatchesAt(col, row))
        {
            DestroyImmediate(gemObj);
            index = Random.Range(0, gemPrefabs.Length);
            gemObj = Instantiate(gemPrefabs[index], spawnPos, Quaternion.identity);
            gemObj.transform.parent = transform;
            newGem = gemObj.GetComponent<Gem>();
            allGems[col, row] = newGem;
        }
    }

    // 특정 위치에 3개 이상 같은 색이 연속되는지 확인
    bool MatchesAt(int x, int y)
    {
        Gem current = allGems[x, y];
        if (current == null) return false;

        // 가로 검사
        if (x > 1 && allGems[x - 1, y] != null && allGems[x - 2, y] != null)
        {
            if (allGems[x - 1, y].gemType == current.gemType &&
                allGems[x - 2, y].gemType == current.gemType)
                return true;
        }

        // 세로 검사
        if (y > 1 && allGems[x, y - 1] != null && allGems[x, y - 2] != null)
        {
            if (allGems[x, y - 1].gemType == current.gemType &&
                allGems[x, y - 2].gemType == current.gemType)
                return true;
        }

        return false;
    }

    // 3개 이상 같은 색이면 구슬 제거
    public void CheckMatches()
    {
        if (isInitialBoard) return; // 초기 세팅 중에는 제거 안 함

        List<Gem> gemsToDestroy = new List<Gem>();

        for (int x = 0; x < topRowCount; x++)
        {
            for (int y = 0; y < totalRows; y++)
            {
                Gem current = allGems[x, y];
                if (current == null) continue;

                // 가로 매칭
                if (x > 1 &&
                    allGems[x - 1, y] != null &&
                    allGems[x - 2, y] != null &&
                    allGems[x - 1, y].gemType == current.gemType &&
                    allGems[x - 2, y].gemType == current.gemType)
                {
                    gemsToDestroy.Add(current);
                    gemsToDestroy.Add(allGems[x - 1, y]);
                    gemsToDestroy.Add(allGems[x - 2, y]);
                }

                // 세로 매칭
                if (y > 1 &&
                    allGems[x, y - 1] != null &&
                    allGems[x, y - 2] != null &&
                    allGems[x, y - 1].gemType == current.gemType &&
                    allGems[x, y - 2].gemType == current.gemType)
                {
                    gemsToDestroy.Add(current);
                    gemsToDestroy.Add(allGems[x, y - 1]);
                    gemsToDestroy.Add(allGems[x, y - 2]);
                }
            }
        }

        // 중복 제거
        gemsToDestroy = new HashSet<Gem>(gemsToDestroy).ToList();

        // 제거 처리
        foreach (var gem in gemsToDestroy)
        {
            if (gem != null)
            {
                Destroy(gem.gameObject);
            }
        }
    }
}
