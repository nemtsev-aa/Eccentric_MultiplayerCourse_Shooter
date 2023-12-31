using Colyseus.Schema;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Character {
    [Header("Move Settings")]
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] float _minHeadAngle = -90f;
    [SerializeField] float _maxHeadAngle = 90f;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private CheckFly _checkFly;
    [SerializeField] private float _jumpDelay = 0.2f;
    [Header("Vision Settings")]
    [SerializeField] private Transform _cameraPoint;
    [SerializeField] Transform _head;
    [Header("Services")]
    [SerializeField] private Health _health;
    [field: SerializeField] public InfoView InfoView { get; private set; }

    private float _inputH;
    private float _inputV;
    private float _rotateX;
    private float _rotateY;
    private float _currentRotateX;
    private float _jumpTime;


    private void Start() {
        Transform camera = Camera.main.transform;
        camera.parent = _cameraPoint;
        camera.localPosition = Vector3.zero;
        camera.localRotation = Quaternion.identity;
        _health.SetMax(MaxHealth);
        _health.SetCurrent(MaxHealth);
    }

    private void Update() {
        RotateX(_rotateX);
    }

    private void FixedUpdate() {
        Move();
        RotateY();
    }

    public void SetInput(float h, float v, float rotateX, float rotateY) {
        _inputH = h;
        _inputV = v;
        _rotateX = rotateX;
        _rotateY += rotateY;
    }

    private void Move() {
        Vector3 velocity = (transform.forward * _inputV + transform.right * _inputH).normalized * Speed;
        velocity.y = _rigidbody.velocity.y;
        Velocity = velocity;
        _rigidbody.velocity = Velocity;
    }

    public void GetMoveInfo(out Vector3 position, out Vector3 velocity, out float rotateX, out float rotateY) {
        position = transform.position;
        velocity = _rigidbody.velocity;
        rotateX = _head.localEulerAngles.x;
        rotateY = transform.eulerAngles.y;
    }

    public void RotateX(float value) {
        _currentRotateX = Mathf.Clamp(_currentRotateX + value, _minHeadAngle, _maxHeadAngle);
        _head.localEulerAngles = new Vector3(_currentRotateX, 0, 0);
    }

    private void RotateY() {
        _rigidbody.angularVelocity = new Vector3(0f, _rotateY, 0f);
        _rotateY = 0;
    }

    internal void OnChange(List<DataChange> changes) {

        foreach (var dataChange in changes) {
            switch (dataChange.Field) {
                case "loss":
                    MultiplayerManager.Instance.LossCounter.SetPlayerLoss((byte)dataChange.Value);
                    Debug.Log("OnChange: loss");
                    InfoView.ShowInfoMessage(InfoType.Restart);
                    break;
                case "currentHP":
                    _health.SetCurrent((sbyte)dataChange.Value);
                    break;
                case "kill":
                    Debug.Log("OnChange: kill");
                    InfoView.ShowInfoMessage(InfoType.Kill);
                    break;
                case "headSh":
                    Debug.Log("OnChange: headSh");
                    InfoView.ShowInfoMessage(InfoType.HeadShoot);
                    break;
                default:
                    Debug.LogWarning($"{dataChange.Field} not handled");
                    break;
            }
        }
    }

    public void Jump() {
        if (_checkFly.IsFly) return;
        if (Time.time - _jumpTime < _jumpDelay) return;

        _jumpTime = Time.time;
        _rigidbody.AddForce(0, _jumpForce, 0, ForceMode.VelocityChange);
    }

}
