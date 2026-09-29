using UnityEngine;
using Fusion;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

public class WallController : NetworkBehaviour
{
    [SerializeField] private Vector2 dir;
    [SerializeField] private Vector2 moveDistance;
    [SerializeField] private float moveSpeed;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;

    [SerializeField] private MapButton[] button;

    private Vector2 startPos;
    private bool soundCheck;

    public override void Spawned()
    {
        startPos = transform.position;
    }
    public override void FixedUpdateNetwork()
    {
        Vector2 targetPos = startPos;
        foreach (var btn in button)
        {
            if (btn.isClicked)
            {
                if (!soundCheck)
                {
                    soundCheck = true;
                    audioSource.PlayOneShot(clip);
                }
                
                targetPos = startPos + dir * moveDistance;
                break;
            }
        }

        if(transform.position.x == startPos.x && transform.position.y == startPos.y && soundCheck)
        {
            soundCheck = false;
        }
        

        transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Runner.DeltaTime);
    }

}
