using Colyseus;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerManager : ColyseusManager<MultiplayerManager> {
    [SerializeField] private PlayerCharacter _playerPrefab;
    [SerializeField] private EnemyController _enemyPrefab;
    private ColyseusRoom<State> _room;
    
    protected override void Awake() {
        base.Awake();
        Instance.InitializeClient();
        Connect();
    }
    private async void Connect() {
        Dictionary<string, object> data = new Dictionary<string, object>() {
            {"speed", _playerPrefab.Speed }
        };

        _room = await Instance.client.JoinOrCreate<State>("state_handler", data);
        _room.OnStateChange += OnChange;
    }

    private void OnChange(State state, bool isFirstState) {
        if (isFirstState == false) return;
        var player = state.players[_room.SessionId];
        state.players.ForEach((key, player) => {
            if (key == _room.SessionId) CreatePlayer(player);
            else CreateEnemy(key, player);
        });

        _room.State.players.OnAdd += CreateEnemy;
        _room.State.players.OnRemove += RemoveEnemy;
    }

    private void CreatePlayer(Player player) {
        var position = new Vector3(player.pX, player.pY, player.pZ);
        Instantiate(_playerPrefab, position, Quaternion.identity);
    }

    private void CreateEnemy(string key, Player player) {
        var position = new Vector3(player.pX, player.pY, player.pZ);
        var enemy = Instantiate(_enemyPrefab, position, Quaternion.identity);
        enemy.Init(player);

    }

    private void RemoveEnemy(string key, Player player) {

    }

    public void SendMessage(string key, Dictionary<string, object> data) {
        _room.Send(key, data);
    }

    public void SendMessage(string key, string data) {
        _room.Send(key, data);
    }

    public string GetSessionID() => _room.SessionId;

    protected override void OnDestroy() {
        base.OnDestroy();
        _room.Leave();
    }
}

