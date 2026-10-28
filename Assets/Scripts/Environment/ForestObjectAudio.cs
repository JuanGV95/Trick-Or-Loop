using UnityEngine;

public sealed class ForestObjectAudio : MonoBehaviour
{
    [SerializeField] private AudioSource woodSource;
    [SerializeField] private AudioSource distantSource;

    public void PlayWoodSound()
    {
        if (woodSource != null && woodSource.clip != null)
            woodSource.PlayOneShot(woodSource.clip);
    }

    public void PlayDistantSound()
    {
        if (distantSource != null && distantSource.clip != null)
            distantSource.PlayOneShot(distantSource.clip);
    }
}
