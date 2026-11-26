using UnityEngine;
using Fusion;

public enum InputButton
{
    Catch
}

/// <summary>
/// 네트워크 이동 입력을 받을 구조체
/// </summary>
public struct NetworkInputData : INetworkInput
{
    public Vector3 Direction;
    public NetworkButtons Buttons;
}
