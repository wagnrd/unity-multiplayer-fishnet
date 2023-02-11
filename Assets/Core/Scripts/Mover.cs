#nullable enable

using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Mover : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotationSpeed;

    private CharacterController _characterController = null!;
    private Vector2 _movementDirection;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        TimeManager.OnTick += TimeManager_OnTick;
        _characterController = GetComponent<CharacterController>();
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        if (TimeManager is not null)
            TimeManager.OnTick -= TimeManager_OnTick;
    }

    public void OnMove(InputValue value)
    {
        _movementDirection = value.Get<Vector2>();
        Debug.Log(_movementDirection);
    }

    private void TimeManager_OnTick()
    {
        if (IsOwner)
        {
            Reconcile(default, false);
            BuildActions(out var moveData);
            Move(moveData, false);
        }

        if (IsServer)
        {
            Move(default, true);

            if (TimeManager.Tick % 3 != 0)
                return;

            var reconcileData = new ReconcileData
            {
                Position = transform.position
            };

            Reconcile(reconcileData, true);
        }
    }

    private void BuildActions(out MoveData moveData)
    {
        moveData = default;
        moveData.Backward = _movementDirection.y;
        moveData.Left = _movementDirection.x;
    }

    [Replicate]
    private void Move(MoveData moveData, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
    {
        var tickDelta = (float)TimeManager.TickDelta;
        var movementDirection = new Vector3(moveData.Left, 0, moveData.Backward).normalized;
        var movement = movementDirection * (_moveSpeed * tickDelta);
        _characterController.Move(movement);
    }

    [Reconcile]
    private void Reconcile(ReconcileData reconcileData, bool asServer, Channel channel = Channel.Unreliable)
    {
        transform.position = reconcileData.Position;
    }

    private struct MoveData : IReplicateData
    {
        public float Backward;
        public float Left;

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