using Fusion;
using Fusion.Sockets;
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;


/// <summary>
/// 플레이어 소환 스크립트
/// </summary>
public class NetworkPlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkObject _playerPrefab;

    private readonly Dictionary<PlayerRef, NetworkObject> _players = new();

    private bool _gameSceneReady;

    /// <summary>
    /// 플레이어 참가 시 호출
    /// </summary>
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer || !_gameSceneReady)
            return;

        SpawnPlayer(runner, player);
    }

    /// <summary>
    /// 플레이어 퇴장 시 호출
    /// </summary>
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
            return;

        if (_players.TryGetValue(player, out NetworkObject obj))
        {
            runner.Despawn(obj);
            _players.Remove(player);
        }
    }

    /// <summary>
    /// GameScene 로딩 완료
    /// </summary>
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        _gameSceneReady = true;

        if (!runner.IsServer)
            return;

        foreach (PlayerRef player in runner.ActivePlayers)
        {
            SpawnPlayer(runner, player);
        }
    }

    /// <summary>
    /// 플레이어 생성
    /// </summary>
    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (_players.ContainsKey(player))
            return;

        if (_playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab not found");
            return;
        }

        int index = _players.Count;

        Vector3 position = new Vector3(
            (index % 2) * 3f - 1.5f,
            0.5f,
            (index / 2) * 3f - 1.5f
        );

        PlayerRole role = player == runner.LocalPlayer ? PlayerRole.Thief : PlayerRole.Police;
        NetworkObject obj = runner.Spawn(_playerPrefab, position, Quaternion.identity, player,
            onBeforeSpawned: (spawnRunner, spawnedObject) =>
            {
                NetworkPlayer networkPlayer = spawnedObject.GetComponent<NetworkPlayer>();
                networkPlayer.Role = role;    
            });
        
        runner.SetPlayerObject(player, obj);

        _players.Add(player, obj);
        
        Debug.Log($"Spawn Player : {player}");
    }
    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData data = new NetworkInputData();

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            input.Set(data);
            return;
        }

        if (keyboard.wKey.isPressed)
            data.Direction += Vector3.forward;

        if (keyboard.sKey.isPressed)
            data.Direction += Vector3.back;

        if (keyboard.aKey.isPressed)
            data.Direction += Vector3.left;

        if (keyboard.dKey.isPressed)
            data.Direction += Vector3.right;

        data.Direction = data.Direction.normalized;
        
        data.Buttons.Set(InputButton.Catch, keyboard.fKey.isPressed);

        input.Set(data);
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason reason)
    {
        Debug.Log("Network End");
        
        NetworkManager manager = GetComponent<NetworkManager>();

        if (manager != null)
            manager.HandleNetworkShutdown(reason);
    }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress address, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessions) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken token) { }
    public void OnSceneLoadStart(NetworkRunner runner) { _gameSceneReady = false; }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ReadOnlySpan<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
