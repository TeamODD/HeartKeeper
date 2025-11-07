using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    // Audio Source
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource shot;
    [SerializeField] private AudioSource remove;
    [SerializeField] private AudioSource attached;
    [SerializeField] private Slider BGMslider;
    [SerializeField] private Slider SFXslider;
    [Range(0f, 1f)]
    [SerializeField] private float startVolume = 0.5f;
    [Header("공통 SFX 볼륨 (0~1)")]
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.5f;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (BGMslider != null)  BGMslider.value = startVolume;
        if (SFXslider != null)  SFXslider.value = startVolume;
    }
    public void OnBGMChanged(float value)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = value;   // AudioSource 볼륨은 0~1 사이
        }
    }

    public void Play_shot()     => PlayWithVolume(shot);
    public void Play_remove()   => PlayWithVolume(remove);
    public void Play_attached() => PlayWithVolume(attached);
    private void PlayWithVolume(AudioSource src)
    {
        if (src == null) return;
        src.volume = sfxVolume;   // 공통 볼륨 적용
        src.Play();
    }
    public void OnSFXChanged(float value)
    {
        sfxVolume = value;
        // 어짜피 음악이 재생할때마다 반영된다면 굳이 필요없음.
        // if (shot != null) shot.volume = sfxVolume;
        // if (remove != null) remove.volume = sfxVolume;
        // if (attached != null) attached.volume = sfxVolume;
    }
}
