#nullable enable

using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Mover : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _gravity;
    [SerializeField] private float _maxFallSpeed;

    [Header("Networking")]
    [SerializeField] private int _reconcileSkips;

    private CharacterController _characterController = null!;
    private Vector2 _movementDirection;
    private float _verticalVelocity;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        TimeManager.OnTick += OnTick;
        _characterController = GetComponent<CharacterController>();
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        if (TimeManager is not null)
            TimeManager.OnTick -= OnTick;
    }

    public void OnMove(InputValue value)
    {
        _movementDirection = value.Get<Vector2>();
        Debug.Log(_movementDirection);
    }

    private void OnTick()
    {
        if (IsOwner)
        {
            Reconcile(default, false);
            BuildMoveData(out var moveData);
            Move(moveData, false);
        }

        if (IsServer)
        {
            Move(default, true);

            if (TimeManager.Tick % (1 + _reconcileSkips) != 0)
                return;

            BuildReconcileData(out var reconcileData);
            Reconcile(reconcileData, true);
        }
    }

    private void BuildMoveData(out MoveData moveData)
    {
        moveData = default;
        moveData.Forward = _movementDirection.y;
        moveData.Right = _movementDirection.x;
    }

    private void BuildReconcileData(out ReconcileData reconcileData)
    {
        reconcileData = default;
        var trans = transform;
        reconcileData.Position = trans.position;
        reconcileData.Rotation = trans.rotation;
    }

    [Replicate]
    private void Move(MoveData moveData, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        var tickDelta = (float)TimeManager.TickDelta;
        var movementDirection = new Vector3(moveData.Right, 0, moveData.Forward).normalized;
        var movement = movementDirection * (_moveSpeed * tickDelta);

        if (_characterController.isGrounded)
            _verticalVelocity = 0;
        else
            _verticalVelocity -= _gravity * tickDelta;

        _verticalVelocity = Mathf.Min(_verticalVelocity, _maxFallSpeed);

        movement.y = _verticalVelocity;
        _characterController.Move(movement);
    }

    [Reconcile]
    private void Reconcile(ReconcileData reconcileData, bool asServer, Channel channel = Channel.Unreliable)
    {
        transform.position = reconcileData.Position;
        // ReSharper disable once Unity.InefficientPropertyAccess
        transform.rotation = reconcileData.Rotation;
    }

    private struct MoveData : IReplicateData
    {
        public float Forward;
        public float Right;

        private uint _tick;

        public void Dispose()
        {
        }

        public uint GetTick()
        {
            return _tick;
        }

        public void SetTick(uint value)
        {
            _tick = value;
        }
    }

    private struct ReconcileData : IReconcileData
    {
        public Vector3 Position;
        public Quaternion Rotation;

        private uint _tick;

        public void Dispose()
        {
        }

        public uint GetTick()
        {
            return _tick;
        }

        public void SetTick(uint value)
        {
            _tick = value;
        }
    }
}