using Fusion;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

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

    [Networked] public PlayerRole Role { get; set; }
    [Networked] private NetworkButtons PreviousButtons { get; set; }
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private Character[] _characters;
    [SerializeField, Min(1f)] private float _turnSpeed = 720f;
    [SerializeField, Min(0.01f)] private float _motionBlendDuration = 0.16f;

    [Networked] private int CharacterIndex { get; set; }

    private Transform _visualRoot;
    private Quaternion _visualRotation;
    private PlayableGraph _animationGraph;
    private AnimationMixerPlayable _motionMixer;
    private AnimationClipPlayable _runPlayable;
    private float _runBlend;
    private int _displayedIndex = -1;

    public override void FixedUpdateNetwork()
    {
        //잡기판정은 호스트가 실행
        if (!Object.HasStateAuthority)
            return;

        if (!GetInput(out NetworkInputData input))
            return;
        
        bool catchPressed = input.Buttons.WasPressed(PreviousButtons, InputButton.Catch);

        PreviousButtons = input.Buttons;

        if (Role != PlayerRole.Police || !catchPressed)
            return;

        if (TryGetComponent(out Police police))
        {
            police.TryCatch(Runner);
        }
    }

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

        ShowCharacter(CharacterIndex);
        AttachRole();
    }

    public override void Render()
    {
        if (CharacterIndex != _displayedIndex)
        {
            ShowCharacter(CharacterIndex);
            return;
        }

        float deltaTime = Time.deltaTime;
        _visualRotation = Quaternion.RotateTowards(_visualRotation, transform.rotation, _turnSpeed * deltaTime);
        _visualRoot.rotation = _visualRotation;

        bool isRunning = _movement.IsRunning;
        if (isRunning && _runBlend == 0f)
        {
            SetRunTime();
        }

        _runBlend = Mathf.MoveTowards(_runBlend, isRunning ? 1f : 0f, deltaTime / _motionBlendDuration);
        UpdateMotionWeights();
        _animationGraph.Evaluate(deltaTime);
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
        _visualRotation = transform.rotation;
        _visualRoot = new GameObject("Character Visual").transform;
        _visualRoot.SetParent(transform, false);

        GameObject model = Instantiate(character.Model, _visualRoot, false);
        model.name = character.Model.name;
        model.transform.localPosition = character.Offset;
        model.transform.localScale = Vector3.one * 0.65f;

        if (!model.TryGetComponent(out Animator animator))
        {
            animator = model.AddComponent<Animator>();
        }

        animator.applyRootMotion = false;
        _animationGraph = PlayableGraph.Create("Player Locomotion");
        _animationGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
        _motionMixer = AnimationMixerPlayable.Create(_animationGraph, 2);

        AnimationClipPlayable idlePlayable = AnimationClipPlayable.Create(_animationGraph, character.IdleClip);
        _runPlayable = AnimationClipPlayable.Create(_animationGraph, character.RunClip);
        _motionMixer.ConnectInput(0, idlePlayable, 0);
        _motionMixer.ConnectInput(1, _runPlayable, 0);

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(_animationGraph, "Locomotion", animator);
        output.SetSourcePlayable(_motionMixer);

        idlePlayable.SetTime((int)Runner.Tick * Runner.DeltaTime + index * 0.27f);
        SetRunTime();
        _runBlend = _movement.IsRunning ? 1f : 0f;
        UpdateMotionWeights();
        _animationGraph.Play();

        if (Runner.Config.PeerMode == NetworkProjectConfig.PeerModes.Multiple)
        {
            Runner.AddVisibilityNodes(model);
        }

        _animationGraph.Evaluate();
    }

    private void SetRunTime()
    {
        int elapsedTicks = (int)Runner.Tick - _movement.RunStartTick;
        _runPlayable.SetTime(Mathf.Max(0, elapsedTicks) * Runner.DeltaTime);
    }

    private void UpdateMotionWeights()
    {
        float runWeight = Mathf.SmoothStep(0f, 1f, _runBlend);
        _motionMixer.SetInputWeight(0, 1f - runWeight);
        _motionMixer.SetInputWeight(1, runWeight);
    }

    private void ClearCharacter()
    {
        DestroyAnimationGraph();

        if (_visualRoot != null)
        {
            _visualRoot.gameObject.SetActive(false);
            Destroy(_visualRoot.gameObject);
        }

        _visualRoot = null;
        _displayedIndex = -1;
    }

    private void OnDestroy()
    {
        DestroyAnimationGraph();
    }

    private void DestroyAnimationGraph()
    {
        if (_animationGraph.IsValid())
        {
            _animationGraph.Destroy();
        }
    }

    /// <summary>
    /// 역할에 맞는 스크립트 Prefab에 부여
    /// </summary>
    private void AttachRole()
    {
        switch (Role)
        {
            case PlayerRole.Thief:
                gameObject.AddComponent<Thief>();
                break;
            case PlayerRole.Police:
                gameObject.AddComponent<Police>();
                break;
        }
    }
}
