using Fusion;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [System.Serializable]
    private struct Character
    {
        public GameObject Model;
        public AnimationClip RunClip;
        public AnimationClip IdleClip;
        public Vector3 Offset;
    }

    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private Character[] _characters;

    [Networked] private int CharacterIndex { get; set; }

    private GameObject _model;
    private AnimationClip _runClip;
    private AnimationClip _idleClip;
    private AnimationClip _sampledClip;
    private int _displayedIndex = -1;
    private float _sampledTime = -1f;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            int usedCharacters = 0;
            foreach (PlayerRef player in Runner.ActivePlayers)
            {
                if (Runner.TryGetPlayerObject(player, out NetworkObject playerObject) &&
                    playerObject != Object &&
                    playerObject.TryGetComponent(out NetworkPlayer character))
                {
                    usedCharacters |= 1 << character.CharacterIndex;
                }
            }

            for (int index = 0; index < _characters.Length; index++)
            {
                if ((usedCharacters & (1 << index)) == 0)
                {
                    CharacterIndex = index;
                    break;
                }
            }
        }

        Render();
    }

    public override void Render()
    {
        if (CharacterIndex != _displayedIndex)
        {
            ShowCharacter(CharacterIndex);
        }

        if (_model == null)
        {
            return;
        }

        AnimationClip clip;
        float elapsed;
        if (_movement.IsRunning)
        {
            int elapsedTicks = (int)Runner.Tick - _movement.RunStartTick;
            clip = _runClip;
            elapsed = Mathf.Max(0, elapsedTicks) * Runner.DeltaTime;
        }
        else
        {
            clip = _idleClip;
            elapsed = (int)Runner.Tick * Runner.DeltaTime + CharacterIndex * 0.27f;
        }

        float sampleTime = Mathf.Repeat(elapsed, clip.length);
        if (clip != _sampledClip || sampleTime != _sampledTime)
        {
            clip.SampleAnimation(_model, sampleTime);
            _sampledClip = clip;
            _sampledTime = sampleTime;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        ClearCharacter();
    }

    private void ShowCharacter(int index)
    {
        ClearCharacter();
        _displayedIndex = index;

        Character character = _characters[index];
        _model = Instantiate(character.Model, transform, false);
        _model.name = character.Model.name;
        _model.transform.localPosition = character.Offset;
        _model.transform.localScale = Vector3.one * 0.65f;

        if (!_model.TryGetComponent<Animator>(out _))
        {
            _model.AddComponent<Animator>();
        }

        _runClip = character.RunClip;
        _idleClip = character.IdleClip;

        if (Runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
        {
            Runner.AddVisibilityNodes(_model);
        }
    }

    private void ClearCharacter()
    {
        if (_model != null)
        {
            _model.SetActive(false);
            Destroy(_model);
        }

        _model = null;
        _displayedIndex = -1;
        _sampledClip = null;
        _sampledTime = -1f;
    }
}
