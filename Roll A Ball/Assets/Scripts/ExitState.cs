using UnityEngine;
using Fusion;

public class ExitState : NetworkBehaviour
{
    [SerializeField] private ExitDoor exitDoor_1;
    [SerializeField] private ExitDoor exitDoor_2;
    [SerializeField] private GameObject resultPanel;

    private void Update()
    {
        if (exitDoor_1 == null || exitDoor_2 == null)
            return;

        if (!exitDoor_1.Object || !exitDoor_1.Object.IsValid)
            return;

        if (!exitDoor_2.Object || !exitDoor_2.Object.IsValid)
            return;

        if (exitDoor_1.IsTriggered &&  exitDoor_2.IsTriggered)
        {
            resultPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}
