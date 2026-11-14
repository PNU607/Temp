using System;
using UnityEngine;
using Fusion;
using TMPro;
using System.Threading.Tasks;

/// <summary>
/// 1. 방 생성 요청
/// 2. 방 참가 요청
/// 3. NetworkRunner 생성과 관리
/// 4. 네트워크 연결 성공/실패 처리
/// </summary>
public class NetworkManager : MonoBehaviour
{
    [Header("UI")] 
    [SerializeField] private TMP_InputField roomNameInputField;
    [SerializeField] private TextMeshProUGUI statusText;
    
    private NetworkRunner _networkRunner;
    private bool _isConnecting;

    /// <summary>
    /// 방 생성 버튼
    /// </summary>
    public void CreateRoom()
    {
        StartSessions(GameMode.Host);
    }

    /// <summary>
    /// 방 참가 버튼
    /// </summary>
    public void JoinRoom()
    {
        StartSessions(GameMode.Client);
    }
    
    /// <summary>
    /// 방 생성과 연결 / 네트워크 연결
    /// </summary>
    private async void StartSessions(GameMode mode)
    {
        if (_isConnecting || _networkRunner != null)
            return;

        string roomName = roomNameInputField.text.Trim();

        if (string.IsNullOrEmpty(roomName))
        {
            SetStatus("Input Room name");
            return;
        }

        _isConnecting = true;
        SetStatus("Connecting Sever...");

        try
        {
            // NetworkRunner 생성
            _networkRunner = gameObject.AddComponent<NetworkRunner>();

            var result = await _networkRunner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SessionName = roomName,
                PlayerCount = 4
            });

            if (result.Ok)
            {
                SetStatus($"Connecting Success! Room Name : {roomName}");
                Debug.Log($"Photon 연결 성공. {mode}");
            }

            else
            {
                SetStatus($"Connecting Fail! Room Name : {roomName}");
                Debug.LogWarning("Connecting Fail");

                await CleanupRunner();
            }
        }

        catch (Exception e)
        {
            SetStatus("Connecting Fail While Loading");
            Debug.LogException(e);

            await CleanupRunner();
        }

        finally
        {
            _isConnecting = false;
        }
    }

    /// <summary>
    /// 네트워크 끊기
    /// </summary>
    private async Task CleanupRunner()
    {
        if (_networkRunner == null)
            return;
        
        NetworkRunner networkRunner = _networkRunner;

        if (networkRunner.IsRunning)
            await networkRunner.Shutdown();
        
        Destroy(networkRunner);
        _networkRunner = null;
    }

    /// <summary>
    /// Statur 글자 변경 메서드
    /// </summary>
    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
