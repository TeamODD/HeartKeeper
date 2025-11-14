using UnityEngine;

public class GemEffectSpawner : MonoBehaviour
{
    [Header("ªˆ±Ú∫∞ ¿Ã∆Â∆Æ «¡∏Æ∆’")]
    public GameObject splashEffectRed;
    public GameObject splashEffectBlue;
    public GameObject splashEffectYellow;
    public GameObject splashEffectGreen;

    public void SpawnEffect(GemType type, Vector3 position)
    {
        GameObject prefab = GetEffectPrefab(type);
        if (prefab != null)
        {
            Instantiate(prefab, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($" ¿Ã∆Â∆Æ «¡∏Æ∆’¿Ã æ¯Ω¿¥œ¥Ÿ: {type}");
        }
    }

    GameObject GetEffectPrefab(GemType type)
    {
        switch (type)
        {
            case GemType.Red: return splashEffectRed;
            case GemType.Blue: return splashEffectBlue;
            case GemType.Yellow: return splashEffectYellow;
            case GemType.Green: return splashEffectGreen;
            default: return null;
        }
    }
}