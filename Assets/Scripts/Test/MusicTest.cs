using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTest : MonoBehaviour
{
    [SerializeField]private string musicName = "";
    [SerializeField][Range(0,1)]
    private float musicValue = 0.1f;
    [SerializeField]private bool isPlay = false;
    private bool isPlaying = false;


    private void Update()
    {
        if(isPlay && !isPlaying)
        {
            MusicManager.Instance.PlayMusic(musicName);
            isPlaying = true;
        }
        else if(!isPlay && isPlaying)
        {
            MusicManager.Instance.StopMusic();
            isPlaying = false;
        }
        MusicManager.Instance.ChangeMusicValue(musicValue);
    }
}
