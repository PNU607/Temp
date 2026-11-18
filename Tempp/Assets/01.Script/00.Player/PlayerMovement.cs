using Fusion;
using UnityEngine;

/// <summary>
/// 플레이어 움직임 스크립트
/// </summary>
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            Vector3 direction = data.Direction.normalized;
            
            transform.position += direction * _moveSpeed * Runner.DeltaTime;
        }
    }
}
