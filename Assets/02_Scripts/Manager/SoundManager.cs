using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{ 
    public static SoundManager instance;

    private Dictionary<string, AudioClip> soundDict;
    private AudioSource bgmPlayer;
    private AudioSource sfxPlayer;

    [SerializeField] private AudioClip[] audioClips;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Init()
    {
        bgmPlayer = gameObject.AddComponent<AudioSource>();
        sfxPlayer = gameObject.AddComponent<AudioSource>();

        bgmPlayer.loop = true;
        bgmPlayer.volume = 0.5f;

        bgmPlayer.spatialBlend = 0f;
        sfxPlayer.spatialBlend = 0f;

        soundDict = new Dictionary<string, AudioClip>();

        if(audioClips != null)
        {
            foreach (var clip in audioClips)
            {
                soundDict[clip.name] = clip;
            }
        }        
    }
    public void PlayOnSFX(string soundName)
    {
        if(soundDict.TryGetValue(soundName, out var clip))
        {
            sfxPlayer.clip = clip;
            sfxPlayer.PlayOneShot(clip);
        }
    }
    public void PlayOnBGM(string soundName)
    {
        if(soundDict.TryGetValue(soundName, out var clip))
        {
            bgmPlayer.clip = clip;
            bgmPlayer.Play();            
        }
    }
}
