using Fusion;
using UnityEngine;

public class MapButton : NetworkBehaviour
{
    [SerializeField] private GameObject wall;

    private int objectCount = 0;

    [Networked]
    public bool isClicked { get; private set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (Object == null || !Object.IsValid)
            return;

        if (!Object.HasStateAuthority)
            return;

        if (collision.CompareTag("Player") || collision.CompareTag("Box"))
        {
            objectCount += 1;
            isClicked = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (Object == null || !Object.IsValid)
            return;

        if (!Object.HasStateAuthority)
            return;

        if (collision.CompareTag("Player") || collision.CompareTag("Box"))
        {
            objectCount -= 1;
            isClicked = objectCount > 0;
        }
    }
}
