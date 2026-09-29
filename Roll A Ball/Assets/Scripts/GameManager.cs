using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : NetworkRunnerCallbacks
{
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private NetworkRunner runner;

    private readonly Dictionary<PlayerRef, NetworkObject> playerObjects = new();

    private readonly Dictionary<PlayerRef, int> spawnIndices = new();

    private readonly Color[] playerColors =
    {
        new Color(0.95f, 0.25f, 0.25f),
        new Color(0.25f, 0.50f, 0.95f),
        new Color(0.30f, 0.85f, 0.40f),
        new Color(0.95f, 0.80f, 0.20f),
        new Color(0.75f, 0.35f, 0.90f),
        new Color(0.20f, 0.85f, 0.85f)
    };

    public void Initialize(NetworkRunner runner)
    {
        this.runner = runner;

        Debug.Log(
            $"[GameManager Initialize] " +
            $"IsServer={runner.IsServer}"
        );
    }

    public void SpawnReadyPlayer(PlayerRef player)
    {
        if (runner == null)
        {
            runner = FindFirstObjectByType<NetworkRunner>();
        }

        if (runner == null)
        {
            Debug.LogError("[SpawnReadyPlayer] NetworkRunner가 없습니다.");
            return;
        }

        if (!runner.IsServer)
            return;

        if (playerObjects.ContainsKey(player))
            return;

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[SpawnReadyPlayer] Spawn Points가 없습니다.");
            return;
        }

        int spawnIndex = GetFreeSpawnIndex();

        if (spawnIndex == -1)
        {
            Debug.LogError("[SpawnReadyPlayer] 사용 가능한 Spawn Point가 없습니다.");
            return;
        }

        Transform spawnPoint = spawnPoints[spawnIndex];

        NetworkObject playerObject = runner.Spawn(
            playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation,
            player
        );

        PlayerMove playerMove = playerObject.GetComponent<PlayerMove>();

        playerMove.SpawnIndex = spawnIndex;

        PlayerNameDisplay nameDisplay = playerObject.GetComponent<PlayerNameDisplay>();
        nameDisplay.PlayerName = $"P{spawnIndex + 1}";

        PlayerColor playerColor = playerObject.GetComponent<PlayerColor>();

        playerColor.SetColor(GetRandomAvailableColor());

        playerObjects.Add(player, playerObject);
        spawnIndices.Add(player, spawnIndex);

        Debug.Log(
            $"[Player Spawn] Player={player}, SpawnPoint={spawnIndex}"
        );
    }

    private int GetFreeSpawnIndex()
    {
        for (int i = 0;
             i < spawnPoints.Length;
             i++)
        {
            if (!spawnIndices.ContainsValue(i))
                return i;
        }

        return -1;
    }

    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log(
        $"[Player Left] " +
        $"LeftPlayer={player}, " +
        $"ActivePlayers={runner.ActivePlayers.Count()}, " +
        $"SessionPlayers={runner.SessionInfo.PlayerCount}, " +
        $"MaxPlayers={runner.SessionInfo.MaxPlayers}"
    );

        if (!runner.IsServer)
            return;

        if (playerObjects.TryGetValue(player, out NetworkObject playerObject))
        {
            runner.Despawn(playerObject);

            playerObjects.Remove(player);
            spawnIndices.Remove(player);

            Debug.Log($"[Player 제거] {player}");
        }

        if (runner.TryGetPlayerObject(player, out NetworkObject connectionObject))
        {
            runner.Despawn(connectionObject);

            Debug.Log($"[PlayerConnection 제거] {player}");
        }

        if (runner.ActivePlayers.Count() < 2)
        {
            runner.SessionInfo.IsOpen = true;

            Debug.Log("[방 인원 제한] 2명 미만 → 입장 허용");
        }
    }

    public void SetRunner(NetworkRunner runner)
    {
        this.runner = runner;
    }

    public void RebuildPlayerList(NetworkRunner runner)
    {
        playerObjects.Clear();
        spawnIndices.Clear();

        foreach (NetworkObject obj in runner.GetAllNetworkObjects())
        {
            if (!obj.TryGetComponent<PlayerMove>(
                out PlayerMove playerMove))
            {
                continue;
            }

            PlayerRef player =
                obj.InputAuthority;

            if (player == PlayerRef.None)
                continue;

            playerObjects[player] = obj;
            spawnIndices[player] =
                playerMove.SpawnIndex;
        }

        Debug.Log(
            $"[GameManager] Player 목록 복구 : " +
            $"{playerObjects.Count}명"
        );
    }

    private Color GetRandomAvailableColor()
    {
        List<Color> availableColors =
            new List<Color>(playerColors);

        foreach (NetworkObject playerObject in playerObjects.Values)
        {
            PlayerColor playerColor =
                playerObject.GetComponent<PlayerColor>();

            if (playerColor != null)
            {
                availableColors.Remove(playerColor.Color);
            }
        }

        return availableColors[
            Random.Range(0, availableColors.Count)
        ];
    }
    public async void ReturnToStart()
    {
        if (runner == null)
            return;

        Time.timeScale = 1f;

        NetworkRunner oldRunner = runner;
        runner = null;

        await oldRunner.Shutdown(
            destroyGameObject: true
        );

        SceneManager.LoadScene(0);
    }
}