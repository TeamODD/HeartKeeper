using UnityEngine;

public class BGMSource : MonoBehaviour
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume01 = .5f;
    public bool loop = true;
    public float fadeIn = 0.5f; // 씬 들어올 때 페이드 인
}
