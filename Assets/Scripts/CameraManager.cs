using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup targetGroup;
    private PlayerInputManager playerInputManager;

    void Start()
    {
        playerInputManager = GetComponent <PlayerInputManager>();
        playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    void OnPlayerJoined(PlayerInput playerInput)
    {
        GameObject playerObject = playerInput.gameObject;

        targetGroup.AddMember(playerObject.transform, 1f, 0f);
    }
}