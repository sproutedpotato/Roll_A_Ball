using Fusion;
using TMPro;
using UnityEngine;

public class PlayerNameDisplay : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameText;

    [Networked]
    public NetworkString<_16> PlayerName { get; set; }

    public override void Render()
    {
        if (nameText == null)
            return;

        nameText.text = PlayerName.ToString();

        nameText.gameObject.SetActive(true);

        Color color = nameText.color;
        color.a = 1.0f;
        nameText.color = color;
    }
}