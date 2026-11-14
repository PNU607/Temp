using UnityEngine;

/// <summary>
/// GameScene UI 관리 스크립트
/// </summary>
public class GameSceneUI : MonoBehaviour
{
    public void OnClickLeaveRoom()
    {
        NetworkManager manager = FindFirstObjectByType<NetworkManager>();

        if (manager == null)
        {
            Debug.LogError("No network manager found!");
            return;
        }

        manager.LeaveRoom();
    }
}
