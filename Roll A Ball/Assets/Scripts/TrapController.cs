using UnityEngine;
using Fusion;

public class TrapController : NetworkBehaviour
{
    [SerializeField] private float rotateSpeed = 10f;

    public override void FixedUpdateNetwork()
    {
        transform.Rotate(0f, 0f, rotateSpeed * Runner.DeltaTime);
    }
}
