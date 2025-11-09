using UnityEngine;
using UnityEngine.UI;   // Image/RawImage 등 Graphic 공통

public class HoverGray : MonoBehaviour
{
    [SerializeField] private Graphic target; // 비워두면 Button.image 사용
    [SerializeField] private float gray = 0.6f;

    Color original;

    void Awake()
    {
        if (!target)
        {
            var btn = GetComponent<Button>();
            if (btn) target = btn.image;               // 버튼이면 기본 이미지
            else     target = GetComponent<Graphic>(); // 혹시 직접 붙어있다면
        }
        if (target) original = target.color;
    }

    public void SetGray()
    {
        if (!target) return;
        var a = target.color.a; // 알파 유지
        target.color = new Color(gray, gray, gray, a);
    }

    public void SetNormal()
    {
        if (!target) return;
        target.color = new Color(original.r, original.g, original.b, target.color.a);
    }
}
