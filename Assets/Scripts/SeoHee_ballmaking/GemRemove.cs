using UnityEngine;
using System.Collections.Generic;

public class GemRemover : MonoBehaviour
{
    public List<List<Gem>> allGems;

    public void ForceRemoveMatches()
    {
        if (allGems == null || allGems.Count == 0) return;

        bool[,] visited = new bool[allGems.Count, GetMaxColumnCount()];
        HashSet<Vector2Int> gemsToRemove = new HashSet<Vector2Int>();

        for (int row = 0; row < allGems.Count; row++)
        {
            for (int col = 0; col < allGems[row].Count; col++)
            {
                if (allGems[row][col] == null || visited[row, col]) continue;

                List<Vector2Int> group = GetConnectedGroup(row, col, allGems[row][col].gemType, visited);
                if (group.Count >= 3)
                {
                    foreach (var pos in group)
                        gemsToRemove.Add(pos);
                }
            }
        }

        foreach (var pos in gemsToRemove)
        {
            Gem gem = allGems[pos.x][pos.y];
            if (gem != null)
            {
                allGems[pos.x][pos.y] = null;
#if UNITY_EDITOR
                DestroyImmediate(gem.gameObject);
#else
                Destroy(gem.gameObject);
#endif
            }
        }
    }

    int GetMaxColumnCount()
    {
        int max = 0;
        foreach (var row in allGems)
        {
            if (row.Count > max) max = row.Count;
        }
        return max;
    }

    List<Vector2Int> GetConnectedGroup(int startRow, int startCol, GemType type, bool[,] visited)
    {
        List<Vector2Int> group = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startRow, startCol));
        visited[startRow, startCol] = true;

        int[][] directions = new int[][]
        {
            new int[]{-1, 0},  new int[]{1, 0},
            new int[]{0, -1},  new int[]{0, 1},
            new int[]{-1, -1}, new int[]{-1, 1},
            new int[]{1, -1},  new int[]{1, 1}
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