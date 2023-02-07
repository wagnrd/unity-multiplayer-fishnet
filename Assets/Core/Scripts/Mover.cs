#nullable enable

using System;
using FishNet.Object;
using UnityEngine;
using UnityEngine.Assertions;

public class Mover : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _rotationSpeed;

    private CharacterController _characterController = null!;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();

        Assert.IsNotNull(_characterController);
    }


    private struct MoveData
    {
        public float Forward;
        public float Left;
    }

    private struct ReconcileData
    {
        public Vector3 Position;
        public Vector3 Rotation;
    }
}