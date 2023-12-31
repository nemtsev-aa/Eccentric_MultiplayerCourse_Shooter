using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {
    
    [SerializeField] private float _restartDelay = 3f;
    [SerializeField] private PlayerCharacter _player;
    [field: SerializeField] public Armory PlayerArmory { get; private set; }
    [SerializeField] private Squat _squat;
    [SerializeField] private float _mouseSensetivity = 2f;
    private MultiplayerManager _multiplayerManager;
    private bool _hold = false;
    private bool _hideCursor;

    private void Start() {
        _multiplayerManager = MultiplayerManager.Instance;
        PlayerArmory.OnActiveWeaponChanged += SendNewWeaponID;
        _hideCursor = true;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = _hideCursor ? CursorLockMode.Locked : CursorLockMode.None;
        }

        if (_hold) return;

        float mouseX = 0f;
        float mouseY = 0f;
        bool isShoot = false;

        if (_hideCursor) {
            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");
            isShoot = Input.GetMouseButton(0);
        }

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        _player.SetInput(h, v, -mouseY * _mouseSensetivity, mouseX * _mouseSensetivity);

        bool space = Input.GetKeyDown(KeyCode.Space);
        if (space) _player.Jump();

        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            _squat.SetSquatState(true);
            SendSquat();
        }

        if (Input.GetKeyUp(KeyCode.LeftControl)) {
            _squat.SetSquatState(false);
            SendSquat();
        }

        
        if (isShoot && PlayerArmory.ActiveWeapon.TryShoot(out ShootInfo shootInfo)) SendShoot(ref shootInfo);

        SendWeaponID();
        SendMove();
    }

    private void SendShoot(ref ShootInfo shootInfo) {
        shootInfo.key = _multiplayerManager.GetSessionID();
        string json = JsonUtility.ToJson(shootInfo);
        _multiplayerManager.SendMessage("shoot", json);
    }

    private void SendMove() {
        _player.GetMoveInfo(out Vector3 position, out Vector3 velocity, out float rotateX, out float rotateY);
        Dictionary<string, object> data = new Dictionary<string, object>() {
            {"pX", position.x},
            {"pY", position.y},
            {"pZ", position.z},
            {"vX", velocity.x},
            {"vY", velocity.y},
            {"vZ", velocity.z},
            {"rX", rotateX },
            {"rY", rotateY }
        };
        _multiplayerManager.SendMessage("move", data);
    }

    private void SendSquat() {

        Dictionary<string, object> data = new Dictionary<string, object>(){
            { "sq", _squat.IsSquating },
        };
        _multiplayerManager.SendMessage("squat", data);
    }

    public void Restart(int spawnIndex) {
        _multiplayerManager.SpawnPoints.GetPoint(spawnIndex, out Vector3 position, out Vector3 rotation);
        StartCoroutine(Hold());

        _player.transform.position = position;
        rotation.x = 0;
        rotation.z = 0;

        _player.transform.eulerAngles = rotation;
        _player.SetInput(0, 0, 0, 0);

        Dictionary<string, object> data = new Dictionary<string, object>() {
            {"pX", position.x},
            {"pY", position.y},
            {"pZ", position.z},
            {"vX", 0},
            {"vY", 0},
            {"vZ", 0},
            {"rX", 0},
            {"rY", rotation.y}
        };
        _multiplayerManager.SendMessage("move", data);
    }

    private IEnumerator Hold() {
        _hold = true;
        yield return new WaitForSecondsRealtime(_restartDelay);
        _hold = false;
    }

    private void SendWeaponID() {
        //int currentID = _playerArmory.CurrentWeaponID;

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            PlayerArmory.SetWeaponID(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            PlayerArmory.SetWeaponID(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            PlayerArmory.SetWeaponID(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) {
            PlayerArmory.SetWeaponID(3);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") >= 0.1f) {
            PlayerArmory.SetWeaponID(true);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f) {
            PlayerArmory.SetWeaponID(false);
        }
    }

    private void SendNewWeaponID(int value) {
        Dictionary<string, object> data = new Dictionary<string, object>() { { "wID", value } };
        _multiplayerManager.SendMessage("wID", data);
    }

    private void OnDisable() {
        PlayerArmory.OnActiveWeaponChanged -= SendNewWeaponID;
    }
}

[System.Serializable]
public struct ShootInfo {
    public string key;
    public int weaponID;
    public float pX;
    public float pY;
    public float pZ;
    public float dX;
    public float dY;
    public float dZ;
}

[System.Serializable]
public struct RestartInfo {
    public float x;
    public float z;
}
