using UnityEngine;

public class BGMSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;

    private void Awake()
    {
        audioSource.PlayOneShot(clip);
    }
}
