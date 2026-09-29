using Fusion;
using UnityEngine;

public class PlayerConnection : NetworkBehaviour
{
    [Networked]
    public NetworkBool IsInGame { get; set; }

    [Networked]
    public NetworkBool IsHost { get; set; }

    public override void Spawned()
    {
        DontDestroyOnLoad(this);

        Debug.Log(
            $"[Connection Spawned] Player={Object.InputAuthority}, " +
            $"IsInputAuthority={Object.HasInputAuthority}, " +
            $"IsServer={Runner.IsServer}, " +
            $"IsHost={IsHost}"
        );
    }

    public void SetHost(bool value)
    {
        if (!Object.HasStateAuthority)
            return;

        IsHost = value;

        Debug.Log(
            $"[Host 설정] Player={Object.InputAuthority}, " +
            $"IsHost={IsHost}"
        );
    }

    public void SetGameReady()
    {
        Debug.Log(
            $"[SetGameReady 호출] Player={Object.InputAuthority}"
        );

        RPC_SetGameReady();
    }

    [Rpc(
        sources: RpcSources.InputAuthority,
        targets: RpcTargets.StateAuthority
    )]
    private void RPC_SetGameReady()
    {
        Debug.Log(
            $"[RPC_SetGameReady 수신] Player={Object.InputAuthority}"
        );

        IsInGame = true;

        Debug.Log(
            $"[IsInGame 변경] Player={Object.InputAuthority}, " +
            $"IsInGame={IsInGame}"
        );

        GameManager gameManager =
            FindFirstObjectByType<GameManager>();

        Debug.Log(
            $"[GameManager 검색] " +
            $"결과={(gameManager != null ? "찾음" : "없음")}"
        );

        if (gameManager != null)
        {
            gameManager.SpawnReadyPlayer(
                Object.InputAuthority
            );
        }
    }
}