using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Rofl : MonoBehaviour
{
    [SerializeField] AudioClip AudioClipNormal;
    [SerializeField] AudioClip AudioClipRofl;
    [SerializeField] AudioSource AudioSource;
    [SerializeField] Button Btn;
    private int firstStageRepeatTimes =3;
    private int now;
    private void Awake()
    {
        Btn.onClick.AddListener(OnBtnClick);
    }

    private void OnBtnClick()
    {
        now++;
        AudioClip clip;
        clip = AudioClipNormal;
        if (now == firstStageRepeatTimes)
        {
           clip = AudioClipRofl;
        }
        if (now >= firstStageRepeatTimes)
        {
            int random = Random.Range(0, 2);
            if (random % 2 == 0)
                clip = AudioClipRofl;
            else
                clip = AudioClipNormal;
        }
        AudioSource.clip = clip;
        AudioSource.Play();
    }
}
