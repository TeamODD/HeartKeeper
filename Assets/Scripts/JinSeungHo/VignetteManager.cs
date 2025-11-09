using System.Collections.Generic;
using System.Transactions;
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
    // 층별 감지 박스를 저장할 리스트 변수
    private GameObject[][] detectBox;

    public GameObject leftArea;
    public GameObject rightArea;
    // 게임 오버 조건 충족시 게임 오버 판정을 내기 위해 게임 오버 매니저 받아옴
    public GameObject gameOverManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 시작시 비네트 관련 설정
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

        // 맨 마지막 2개의 층만 감지하면 됨
        detectBox = new GameObject[2][];

        int floorNumber = GetComponent<SaveFloorNumber>().floorNumber;
        for(int i = 2; i > 0; i--)
        {
            string floorStr = "Floor" + (floorNumber - i);
            detectBox[i - 1] = GameObject.FindGameObjectsWithTag(floorStr);
        }
        // 몇 층을 가져왔는지 체크
        // Debug.Log(detectBox[0][0].name + "가 위치한 맨 마지막에서 첫번째 층을 얻어옴, 맨 마지막에서 첫번째 층 개수 = " + detectBox[0].Length);
        // Debug.Log(detectBox[1][0].name + "에서 맨 마지막에서 두번째 층을 얻어옴, 맨 마지막에서 두번째 층 개수 = " + detectBox[1].Length);
    
    }
        
    // Update is called once per frame
    void Update()
    {
        // 왼쪽 오른쪽 구역의 버블 수를 감지
        int leftBubble = leftArea.GetComponent<CountInsideBox>().currentObjCount;
        int rightBubble = rightArea.GetComponent<CountInsideBox>().currentObjCount;
        // 왼쪽 오른쪽 버블의 차이수를 저장
        int diff = Mathf.Abs(leftBubble - rightBubble);

        // 8층 9층의 버블 개수를 가져옴
        int secondToLastFloorBubbleCount = 0;
        int lastFloorBubbleCount = 0;

        // 9층의 버블 개수 카운트
        for (int i = 0; i < detectBox[0].Length; i++) {
            GameObject areaObj = detectBox[0][i];
            CheckBubble chkBubble = areaObj.GetComponent<CheckBubble>();

            if (chkBubble.isBubbleOn)
            {
                lastFloorBubbleCount++;
            }
        }
        // 8층의 버블 개수를 가져옴
        for (int i = 0; i < detectBox[1].Length; i++) {
            GameObject areaObj = detectBox[1][i];
            CheckBubble chkBubble = areaObj.GetComponent<CheckBubble>();

            if (chkBubble.isBubbleOn)
            {
                secondToLastFloorBubbleCount++;
            }
        }

        // 임시로 
        // Debug.Log("마지막층 버블 개수 = " + lastFloorBubbleCount);
        // Debug.Log("마지막에서 두번째 층 버블 개수 = " + secondToLastFloorBubbleCount);

        // 8층 9층 둘다 버블이 없으면 원래의 로직으로 진행
        if (lastFloorBubbleCount == 0 && secondToLastFloorBubbleCount == 0)
        {
            // 버블 차이수가 최대치와 같거나 그 이상이면 비네트 세기를 최대치 값으로 고정
            if (diff >= maxDiffBubble)
            {
                targetIntensity = maxIntensity;
            }
            else
            {   // 아니라면 비네트 세기를 차이수에 비례해 저장
                targetIntensity = diff * intensityPerBubble;
            }
            // 서서히 비네트가 목표 비네트 세기로 변하는 방향으로 지정
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * changeSpeed);
            // 비네트 세기 값 저장
            vig.intensity.value = currentIntensity;
        }

        // 만약 8층(floor7, 즉 detectBox[1]이 가지고 있는 박스들)에 버블이 하나라도 있다면 바로 비네트 세기를 최대치로 조정
        if (secondToLastFloorBubbleCount > 0)
        {
            targetIntensity = maxIntensity;
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime / changeSpeed * 10);
            vig.intensity.value = currentIntensity;     // 비네트 세기 적용
        }

        // 만약 9층(floor8, 즉 detectBox[0]가 가지고 있는 박스들)에 버블이 하나라도 있다면 바로 게임 오버
        if (lastFloorBubbleCount > 0)
        {
            gameOverManager.GetComponent<GameOverManager>().isGameOver = true;
        }
    }
}
