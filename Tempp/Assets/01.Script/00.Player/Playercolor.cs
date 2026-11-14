using Fusion;
using UnityEngine;

/// <summary>
/// 플레이어의 색 구분
/// </summary>
public class Playercolor : NetworkBehaviour
{
    [SerializeField] private Renderer _renderer;

    /// <summary>
    /// 공 색깔
    /// </summary>
    private static readonly Color[] _colors =
    {
        new Color(0.26f, 0.53f, 0.95f),
        new Color(0.94f, 0.39f, 0.39f),
        new Color(0.29f, 0.73f, 0.51f),
        new Color(0.95f, 0.75f, 0.28f)
    };
    private Material _material;

    public override void Spawned()
    {
        if (_renderer == null)
        {
            Debug.LogError("Renderer is Not Found");
            return;
        }

        int playerId = Object.InputAuthority.PlayerId;
        int colorIndex = (playerId - 1) % _colors.Length;

        if (colorIndex < 0)
            colorIndex = 0;

        _material = _renderer.material;
        _material.color = _colors[colorIndex];
    }

    private void OnDestroy()
    {
        if (_material != null)
            Destroy(_material);
    }
}
