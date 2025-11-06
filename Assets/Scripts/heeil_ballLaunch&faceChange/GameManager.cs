using UnityEngine;

public class GameManager : MonoBehaviour
{

    
    [SerializeField] private string color;
    [SerializeField] private Transform ballSpawnPoint;
    
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private GameObject parent;
    [SerializeField] private GameObject face;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SpawnBall()
    {
        Instantiate(ballPrefab, ballSpawnPoint.position, ballSpawnPoint.rotation, parent.transform);
    }
}
