using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteManager : MonoBehaviour
{

    // 비네트 조절을 위한 변수들
    public Volume redVigVolume;
    // 비네트 최대 세기 = 게임 오버시 비네트의 강도
    public float maxIntensity = 0.3f;
    // 이 변수로 비네트 조절
    private Vignette vig;
    private float currentIntensity;     // 현재 비네트 강도
    private float targetIntensity;      // 목표 비네트 강도
    public float changeSpeed = 5;       // 비네트 변화 속도
    private float intensityPerBubble;         // 버블 하나당 변화될 비네트 강도
    public int maxDiffBubble;

    public GameObject leftArea;
    public GameObject rightArea;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(redVigVolume.profile.TryGet<Vignette>(out vig))
        {
            currentIntensity = 0;
            targetIntensity = 0;
            intensityPerBubble = maxIntensity / (float)maxDiffBubble;

            vig.intensity.value = 0;
            vig.color.value = Color.red;

            Debug.Log("비네트 컴포넌트를 가져오는데 성공함");
            Debug.Log("버블 하나당 Intensity : " + intensityPerBubble + "증가");
        }
        else
        {
            Debug.LogError("비네트 컴포넌트를 가져오는데 실패함");
        }
    }

    // Update is called once per frame
    void Update()
    {
        int leftBubble = leftArea.GetComponent<CountInsideBox>().currentObjCount;
        int rightBubble = rightArea.GetComponent<CountInsideBox>().currentObjCount;

        int diff = Mathf.Abs(leftBubble - rightBubble);

        if(diff >= maxDiffBubble)
        {
            targetIntensity = maxIntensity;
        }
        else
        {
            targetIntensity = diff * intensityPerBubble;
        }
        
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * changeSpeed);

        vig.intensity.value = currentIntensity;
    }
}
