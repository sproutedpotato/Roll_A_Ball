using UnityEngine;
using Fusion;

public class ExitDoor : NetworkBehaviour
{
    [Networked]
    public bool IsTriggered { get; private set; }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Object == null || !Object.IsValid)
            return;

        if (!Object.HasStateAuthority)
            return;

        if (collision.CompareTag("Player"))
        {
            IsTriggered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (Object == null || !Object.IsValid)
            return;

        if (!Object.HasStateAuthority)
            return;

        if (collision.CompareTag("Player"))
        {
            IsTriggered = false;
        }
    }

}
