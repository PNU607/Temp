using UnityEngine;
using Fusion;

/// <summary>
/// 경찰 스크립트
/// </summary>
public class Police : Player
{
    [SerializeField, Min(0f)] private float _catchRange = 2f;
    [SerializeField, Range(0f, 360f)] private float _viewAngle = 150f;

    public void TryCatch(NetworkRunner runner)
    {
        // 다른 곳에서 호출해도 호스트에서만 판정.
        if (!runner.IsServer)
            return;

        foreach (PlayerRef player in runner.ActivePlayers)
        {
            if (!runner.TryGetPlayerObject(player, out NetworkObject obj))
                continue;

            if (!obj.TryGetComponent(out NetworkPlayer target))
                continue;

            if (target.Role != PlayerRole.Thief)
                continue;

            Vector3 toTarget = target.transform.position - transform.position;
            
            if (toTarget.sqrMagnitude > _catchRange * _catchRange)
                continue;

            toTarget.y = 0f;

            Vector3 forward = transform.forward;
            forward.y = 0f;

            float angle = Vector3.Angle(forward, toTarget);

            if (angle > _viewAngle * 0.5f)
                continue;

            Debug.Log("잡았다!");
            return;
        }
    }
}
