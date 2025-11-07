using UnityEngine;

public class CountInsideBox : MonoBehaviour
{
    public int currentObjCount = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        currentObjCount++;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        currentObjCount--;
    }
}
