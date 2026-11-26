using System;
using Fusion;
using UnityEngine;

/// <summary>
/// 플레이어의 이동 담당 스크립트
/// </summary>
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;

    [Networked] public NetworkBool IsRunning { get; private set; }
    [Networked] public int RunStartTick { get; private set; }
    
    private NetworkCharacterController _controller;

    private void Awake()
    {
        _controller = GetComponent<NetworkCharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        Vector3 direction = Vector3.zero;
        
        if (GetInput(out NetworkInputData input))
            direction = input.Direction.normalized;
        
        bool wantsToRun = direction.sqrMagnitude > 0f;
        
        if (wantsToRun && !IsRunning)
            RunStartTick = Runner.Tick;
        
        IsRunning = wantsToRun;

        _controller.Velocity = direction * _controller.maxSpeed;
        _controller.Move(direction);
    }
}
