using UnityEngine;
using UnityEngine.UI;

public class HoveringRestartButton : MonoBehaviour
{
    public Sprite defaultSprite;
    public Sprite changeSprite;
    private Image targetImg;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetImg = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
