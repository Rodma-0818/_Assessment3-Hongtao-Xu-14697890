using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip introMusic;

    [SerializeField]
    private AudioClip ghostNormalMusic;

    void Start()
    {
        StartCoroutine(PlayIntroThenNormal());
    }

    private IEnumerator PlayIntroThenNormal()
    {
        audioSource.loop = false;
        audioSource.clip = introMusic;
        audioSource.Play();

        float waitTime = Mathf.Min(introMusic.length, 3.0f);
        yield return new WaitForSeconds(waitTime);

        audioSource.clip = ghostNormalMusic;
        audioSource.loop = true;
        audioSource.Play();
    }
}