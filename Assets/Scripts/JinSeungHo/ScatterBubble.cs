using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ScatterBubble : MonoBehaviour
{
    // 게임 내에 존재하는 모든 버블들을 저장하는 배열
    GameObject[] bubbles;
    string bubbleLayerName = "Bubble";

    private void Start()
    {
        List<GameObject> foundBubble = new List<GameObject>();
        int targetLayer = LayerMask.NameToLayer(bubbleLayerName);

        // CS0618 때문에 정렬 포함된 거로 함
        GameObject[] allGameObj = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        
        foreach (GameObject obj in allGameObj)
        {
            if (obj.layer == targetLayer) {
                foundBubble.Add(obj);
            }
        }

        // 찾아낸 모든 버블들을 배열에 저장
        bubbles = foundBubble.ToArray();
    }

    // 게임오버매니저에서 호출될 함수
    // 게임 오버시 구슬들이 중력의 작용(-1, 위로 가야하므로)받아서 위로 떨어짐
    public void ScatteringBubble()
    {
        foreach (GameObject b in bubbles)
        {
            // 우선 버블이 다시 중력의 영향을 받게 함
            Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;

            // 버블의 중력 방향이 위를 향하고, 회전이 가능하게 함
            rb.gravityScale = -1;
            rb.freezeRotation = false;
            // 원래 시소가 충격을 받지 않게 하기 위해 무게 값을 0에 가깝게 했었음
            // 이제 시소가 고정되었고, 버블이 위로 잘 떨어지게 끔 무게를 10으로 함
            rb.mass = 10;
        }
    }
}
