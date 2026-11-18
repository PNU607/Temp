using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;

    [Networked] public NetworkBool IsRunning { get; private set; }
    [Networked] public int RunStartTick { get; private set; }

    public override void FixedUpdateNetwork()
    {
        if (!GetInput(out NetworkInputData data) || data.Direction.sqrMagnitude == 0f)
        {
            IsRunning = false;
            return;
        }

        if (!IsRunning)
        {
            RunStartTick = Runner.Tick;
            IsRunning = true;
        }

        Vector3 direction = data.Direction.normalized;
        transform.position += direction * _moveSpeed * Runner.DeltaTime;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
