using UnityEngine;
using Fusion;

/// <summary>
/// 네트워크 이동 입력을 받을 구조체
/// </summary>
public struct NetworkInputData : INetworkInput
{
    public Vector3 Direction;
}
