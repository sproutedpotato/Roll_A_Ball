using UnityEngine;
using UnityEngine.EventSystems;

public class CameraButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private CameraController cameraController;

    public void OnPointerDown(PointerEventData eventData)
    {
        CameraController[] controllers = FindObjectsByType<CameraController>(FindObjectsSortMode.None);

        foreach (CameraController controller in controllers)
        {
            if (controller.Object != null && controller.Object.IsValid && controller.Object.HasInputAuthority)
            {
                cameraController = controller;
                break;
            }
        }

        if (cameraController == null)
        {
            Debug.Log("내 CameraController를 찾지 못함");
            return;
        }

        cameraController.ViewOtherPlayer();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (cameraController == null)
            return;

        cameraController.ViewMyPlayer();
    }
}