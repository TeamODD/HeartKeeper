using UnityEngine;

public class NextBallColor : MonoBehaviour
{

    [SerializeField] private string color;
    private SpriteRenderer sr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }


    void SetNextColor()
    {
        string[] colors = { "yellow", "red", "blue", "green" };
        color = colors[UnityEngine.Random.Range(0, colors.Length)];

        Color c;
        // 케이스별 얼굴스프라이트 삽입 예정
        // 지금은 색깔로

        switch (color)
        {
            case "yellow": c = Color.yellow; break;
            case "red": c = Color.red; break;
            case "blue": c = Color.blue; break;
            case "green": c = Color.green; break;
            default: c = Color.white; break; // 예외시 화이트
        }

        sr.color = c;
    }
    
    void SetBall()
    {
        
    }
}
