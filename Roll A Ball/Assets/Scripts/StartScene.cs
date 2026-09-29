using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using TMPro;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class StartScene : NetworkRunnerCallbacks
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private NetworkObject playerConnectionPrefab;

    private NetworkRunner runner;

    private bool isLoaded = false;
    private bool isClicked = false;

    private GameObject textObject;

    private readonly Dictionary<NetworkId, NetworkObject> resumedObjects = new();

    private bool IsServerMode()
    {
        string[] args = System.Environment.GetCommandLineArgs();

        foreach (string arg in args)
        {
            if (arg == "-server")
                return true;
        }

        return Application.isBatchMode;
    }

    private void Start()
    {
        if (text == null)
            textObject = GameObject.Find("StatusText");

        if (textObject != null)
            text = textObject.GetComponent<TextMeshProUGUI>();

        if (FindFirstObjectByType<NetworkRunner>() != null)
            return;

        StartGame();
    }

    private void Update()
    {
        if (IsServerMode())
            return;

        if (runner == null)
            return;

        if (SceneManager.GetActiveScene().buildIndex != 0)
            return;

        if (text == null)
            text = FindFirstObjectByType<TextMeshProUGUI>();

        if (text == null)
            return;

        int playerCount = runner.ActivePlayers.Count();

        if (playerCount == 2)
            text.text = "Game Start";
        else
            text.text = "Waiting...";

        Debug.Log(
        $"[Start Update] " +
        $"Players={playerCount}, " +
        $"IsServer={runner.IsServer}, " +
        $"IsRunning={runner.IsRunning}, " +
        $"IsClicked={isClicked}"
    );
  
        if (runner.IsServer && playerCount == 2 && !isClicked)
        {
            Debug.Log("[GameStart 실행됨]");
            isClicked = true;

            int stageIndex = 1;

            runner.LoadScene(SceneRef.FromIndex(stageIndex));
        }

    }

    public async void StartGame()
    {
        runner = gameObject.AddComponent<NetworkRunner>();

        DontDestroyOnLoad(gameObject);

        runner.AddCallbacks(this);

        NetworkSceneManagerDefault sceneManager =
            gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            PlayerCount = 3,
            IsOpen = true,
            SceneManager = sceneManager
        });

        if (result.Ok)
        {
            Debug.Log(
                $"[Photon 접속 성공] " +
                $"Session={runner.SessionInfo.Name}, " +
                $"Player={runner.LocalPlayer}, " +
                $"IsServer={runner.IsServer}"
            );

            isLoaded = true;

            if (text != null)
                text.text = "Waiting...";
        }
        else
        {
            Debug.LogError(
                $"[Photon 접속 실패] " +
                $"{result.ShutdownReason}"
            );
        }
    }
    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log(
            $"[StartScene PlayerLeft] " +
            $"Left={player}, " +
            $"ActivePlayers={runner.ActivePlayers.Count()}"
        );
    }

    public override void OnPlayerJoined(
        NetworkRunner runner,
        PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        Debug.Log($"[PlayerConnection 생성] {player}");

        NetworkObject connectionObject =
            runner.Spawn(
                playerConnectionPrefab,
                Vector3.zero,
                Quaternion.identity,
                player
            );

        PlayerConnection connection =
            connectionObject.GetComponent<PlayerConnection>();

        connection.SetHost(player == runner.LocalPlayer);

        runner.SetPlayerObject(player, connectionObject);

        if (runner.ActivePlayers.Count() >= 2)
        {
            runner.SessionInfo.IsOpen = false;

            Debug.Log("[방 인원 제한] 2명 도달 → 입장 차단");
        }
    }

    public override void OnSceneLoadDone(NetworkRunner runner)
    {
        if (SceneManager.GetActiveScene().name != "Game")
            return;

        StartCoroutine(SetGameReadyWhenConnectionExists());
    }

    private IEnumerator SetGameReadyWhenConnectionExists()
    {
        while (runner != null)
        {
            if (runner.TryGetPlayerObject(
                runner.LocalPlayer,
                out NetworkObject connectionObject))
            {
                PlayerConnection connection =
                    connectionObject.GetComponent<PlayerConnection>();

                Debug.Log(
                    $"[Game 진입 완료] " +
                    $"Player={runner.LocalPlayer}"
                );

                connection.SetGameReady();

                yield break;
            }

            yield return null;
        }
    }

    public override async void OnHostMigration(
    NetworkRunner oldRunner,
    HostMigrationToken hostMigrationToken)
    {
        Debug.Log("[Host Migration] 시작");

        await oldRunner.Shutdown(
            destroyGameObject: false,
            shutdownReason: ShutdownReason.HostMigration
        );

        Destroy(oldRunner.gameObject);

        // StartScene / GameManager가 계속 살아 있도록 유지
        DontDestroyOnLoad(gameObject);

        // 기존 Game 씬 언로드
        Scene gameScene = SceneManager.GetSceneByName("Game");

        if (gameScene.IsValid())
        {
            TaskCompletionSource<bool> unloadTask =
                new TaskCompletionSource<bool>();

            AsyncOperation unloadOperation =
                SceneManager.UnloadSceneAsync(gameScene);

            if (unloadOperation != null)
            {
                unloadOperation.completed +=
                    _ => unloadTask.SetResult(true);

                await unloadTask.Task;
            }
        }

        // 새 Runner가 사용할 Game 씬
        NetworkSceneInfo sceneInfo = default;

        sceneInfo.AddSceneRef(
            SceneRef.FromIndex(1),
            LoadSceneMode.Additive,
            activeOnLoad: true
        );

        // Runner를 StartScene과 분리
        GameObject runnerObject =
            new GameObject("NetworkRunner");

        DontDestroyOnLoad(runnerObject);

        NetworkRunner newRunner =
            runnerObject.AddComponent<NetworkRunner>();

        runner = newRunner;

        newRunner.AddCallbacks(this);

        GameManager gameManager =
            GetComponent<GameManager>();

        if (gameManager != null)
        {
            gameManager.SetRunner(newRunner);
            newRunner.AddCallbacks(gameManager);
        }

        NetworkSceneManagerDefault sceneManager =
            runnerObject.AddComponent<NetworkSceneManagerDefault>();

        StartGameResult result =
            await newRunner.StartGame(new StartGameArgs
            {
                GameMode = hostMigrationToken.GameMode,
                HostMigrationToken = hostMigrationToken,
                HostMigrationResume = HostMigrationResume,
                Scene = sceneInfo,
                SceneManager = sceneManager
            });

        if (!result.Ok)
        {
            Debug.LogError(
                $"[Host Migration 실패] {result.ShutdownReason}"
            );

            Destroy(runnerObject);
            runner = null;
            return;
        }

        Debug.Log(
            $"[Host Migration 성공] " +
            $"Scene={SceneManager.GetActiveScene().name}, " +
            $"IsServer={newRunner.IsServer}, " +
            $"Players={newRunner.SessionInfo.PlayerCount}"
        );
    }

    public override void OnInput(
        NetworkRunner runner,
        NetworkInput input)
    {
        GameplayInput data = new GameplayInput();

        data.Horizontal =
            Input.GetAxisRaw("Horizontal");

        data.Buttons.Set(
            EInputButton.Jump,
            Input.GetKey(KeyCode.Space)
        );

        input.Set(data);
    }

    private void HostMigrationResume(NetworkRunner migrationRunner)
    {
        Debug.Log("[Host Migration Resume] 시작");

        resumedObjects.Clear();

        PlayerRef oldHostPlayer = PlayerRef.None;

        // 기존 Host 찾기
        foreach (
            NetworkObject resumeObject
            in migrationRunner.GetResumeSnapshotNetworkObjects())
        {
            if (resumeObject.TryGetBehaviour<PlayerConnection>(
                out PlayerConnection connection))
            {
                if (connection.IsHost)
                {
                    oldHostPlayer =
                        resumeObject.InputAuthority;

                    Debug.Log(
                        $"[Host Migration] " +
                        $"기존 Host={oldHostPlayer}"
                    );

                    break;
                }
            }
        }

        // NetworkObject 복구
        foreach (
            NetworkObject resumeObject
            in migrationRunner.GetResumeSnapshotNetworkObjects())
        {
            // 기존 Host의 객체는 복구하지 않음
            if (resumeObject.InputAuthority == oldHostPlayer)
                continue;

            NetworkTRSP trsp = null;

            bool hasTRSP =
                resumeObject.TryGetBehaviour<NetworkTRSP>(
                    out trsp
                );

            Vector3 position =
                hasTRSP
                    ? trsp.Data.Position
                    : Vector3.zero;

            Quaternion rotation =
                hasTRSP
                    ? trsp.Data.Rotation
                    : Quaternion.identity;

            NetworkId oldNetworkId =
                resumeObject.Id;

            migrationRunner.Spawn(
                resumeObject,
                position,
                rotation,
                onBeforeSpawned:
                    (newRunner, newObject) =>
                    {
                        newObject.CopyStateFrom(
                            resumeObject
                        );

                        if (
                            newRunner.IsServer &&
                            resumeObject.InputAuthority
                                != PlayerRef.None
                        )
                        {
                            newObject.AssignInputAuthority(
                                resumeObject.InputAuthority
                            );
                        }

                        if (
                            newObject.TryGetBehaviour<PlayerConnection>(
                                out PlayerConnection connection)
                        )
                        {
                            if (newRunner.IsServer)
                            {
                                connection.SetHost(
                                    newObject.InputAuthority ==
                                    newRunner.LocalPlayer
                                );
                            }
                        }

                        resumedObjects.Add(
                            oldNetworkId,
                            newObject
                        );
                    }
            );
        }

        // 씬에 배치된 NetworkObject 상태 복구
        foreach (
            var sceneObject
            in migrationRunner.GetResumeSnapshotNetworkSceneObjects())
        {
            sceneObject.Item1.CopyStateFrom(
                sceneObject.Item2
            );
        }

        // PlayerObject 연결 복구
        foreach (
            KeyValuePair<PlayerRef, NetworkId> pair
            in migrationRunner
                .GetResumeSnapshotNetworkObjectPlayerObjects())
        {
            if (pair.Key == oldHostPlayer)
                continue;

            if (
                resumedObjects.TryGetValue(
                    pair.Value,
                    out NetworkObject playerObject)
            )
            {
                migrationRunner.SetPlayerObject(
                    pair.Key,
                    playerObject
                );
            }
        }
         
        GameManager gameManager =
            FindFirstObjectByType<GameManager>();

        if (gameManager != null)
        {
            gameManager.SetRunner(
                migrationRunner
            );

            gameManager.RebuildPlayerList(
                migrationRunner
            );
        }

        if (migrationRunner.IsServer)
        {
            migrationRunner.SessionInfo.IsOpen = true;

            Debug.Log(
                $"[Host Migration] 방 재개방 : " +
                $"Players={migrationRunner.SessionInfo.PlayerCount}, " +
                $"IsOpen={migrationRunner.SessionInfo.IsOpen}"
            );
        }

        Debug.Log("[Host Migration Resume] 완료");
    }
}