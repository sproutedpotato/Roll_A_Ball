using Fusion;
using UnityEngine;
using Unity.Cinemachine;

public class CameraController : NetworkBehaviour
{
    private CinemachineCamera cinemachineCamera;
    private Transform myTarget;
    private Transform otherTarget;

    public override void Spawned()
    {
        if (!Object.HasInputAuthority)
            return;

        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();

        if (cinemachineCamera == null)
            return;

        myTarget = transform;
        cinemachineCamera.Follow = myTarget;
    }

    public void ViewOtherPlayer()
    {
        if (cinemachineCamera == null)
            return;

        CameraController[] controllers =
            FindObjectsByType<CameraController>(FindObjectsSortMode.None);

        foreach (CameraController controller in controllers)
        {
            if (controller == this)
                continue;

            if (controller.Object != null && controller.Object.IsValid)
            {
                otherTarget = controller.transform;
                break;
            }
        }

        if (otherTarget == null)
        {
            Debug.Log("상대 플레이어를 찾을 수 없습니다.");
            return;
        }

        cinemachineCamera.Follow = otherTarget;
    }

    public void ViewMyPlayer()
    {
        if (cinemachineCamera == null)
            return;

        cinemachineCamera.Follow = myTarget;
    }
}