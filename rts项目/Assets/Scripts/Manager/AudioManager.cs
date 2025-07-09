using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingletonManager<AudioManager>
{
    [SerializeField] private List<AudioSource> SFX = new();
    [SerializeField] private List<AudioSource> BGM = new();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySFX(int _index)
    {
        if (_index >= SFX.Count)
        {
            return;
        }

        SFX[_index].pitch = Random.Range(0.8f, 1.2f);
        SFX[_index].Play();
    }

    public void PlayBGM(int _index)
    {
        if (_index >= BGM.Count)
        {
            return;
        }

        BGM[_index].Play();
    }
}
