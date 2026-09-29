using UnityEngine;
using Fusion;

public class PlayerController : NetworkBehaviour
{
    [Networked]
    private Vector3 savePos { get; set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip saveClip;

    public override void Spawned()
    {
        savePos = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("SavePoint"))
        {
            if(savePos != collision.transform.position)
            {
                audioSource.PlayOneShot(saveClip);
            }
            savePos = collision.transform.position;
        }

        else if (collision.CompareTag("Trap"))
        {
            transform.position = savePos;
            audioSource.PlayOneShot(deathClip);
        }
    }
}
