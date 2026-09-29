using Fusion;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Networked]
    public Color Color { get; set; }

    public void SetColor(Color color)
    {
        if (!HasStateAuthority)
            return;

        Color = color;
    }

    public override void Render()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = Color;
    }
}