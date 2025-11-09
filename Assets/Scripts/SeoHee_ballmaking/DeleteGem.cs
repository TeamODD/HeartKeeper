//using UnityEngine;
//using System.Collections.Generic;

//namespace BubbleBoard
//{
//    [RequireComponent(typeof(BoardGem))]
//    public class DeleteGem : MonoBehaviour
//    {
//        private BoardManager boardManager;
//        private BoardGem thisGem;
//        private bool isRegistered = false; // 중복 등록 방지

//        private void Start()
//        {
//            thisGem = GetComponent<BoardGem>();
//            boardManager = FindObjectOfType<BoardManager>();
//        }

//        private void OnCollisionEnter2D(Collision2D collision)
//        {
//            if (isRegistered) return;

//            BoardGem otherGem = collision.gameObject.GetComponent<BoardGem>();
//            if (otherGem == null) return; // 다른 구슬이 아니면 무시

            
//            Vector2 hitPosition = transform.position;
//            Vector2Int nearestSlot = FindNearestSlot(hitPosition);

            
//            boardManager.RegisterGemAt(nearestSlot.x, nearestSlot.y, thisGem);
//            isRegistered = true;

            
//            boardManager.EnableDeletion();
//        }

//        // 위치를 기반으로 가장 가까운 슬롯(행, 열) 계산
//        private Vector2Int FindNearestSlot(Vector2 pos)
//        {
//            float minDist = float.MaxValue;
//            int bestRow = 0;
//            int bestCol = 0;

//            for (int r = 0; r < boardManager.allGems.Count; r++)
//            {
//                for (int c = 0; c < boardManager.allGems[r].Count; c++)
//                {
//                    Vector3 slotPos = boardManager.allGems[r][c] != null
//                        ? boardManager.allGems[r][c].transform.position
//                        : new Vector3(999f, 999f, 0f); // 비어있으면 멀리 보냄

//                    float dist = Vector2.Distance(pos, slotPos);
//                    if (dist < minDist)
//                    {
//                        minDist = dist;
//                        bestRow = r;
//                        bestCol = c;
//                    }
//                }
//            }

//            return new Vector2Int(bestRow, bestCol);
//        }
//    }
//}


