using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }   //  NEW

    [SerializeField] private CinemachineTargetGroup targetGroup;
    private PlayerInputManager playerInputManager;
    public GameObject playerHUD;
    public Transform playerHUDSlots;
    public GameObject defeatMenu;
    public Transform perkSlots;

    // set players hud colors
    [SerializeField] private Color p1Color, p2Color, p3Color, p4Color;

    private void Awake()   //  NEW
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    void Start()
    {
        playerInputManager = GetComponent<PlayerInputManager>();
        playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    //  Small helper so we can reuse logic
    public void AddTarget(Transform target, float weight = 1f, float radius = 0f)
    {
        if (targetGroup == null || target == null) return;
        targetGroup.AddMember(target, weight, radius);
    }

    public void RemoveTarget(Transform target)
    {
        if (targetGroup == null || target == null) return;
        targetGroup.RemoveMember(target);
    }

    void OnPlayerJoined(PlayerInput playerInput)
    {
        GameObject playerObject = playerInput.gameObject;

        // Add player to camera group
        AddTarget(playerObject.transform, 1f, 0f);

        // Instancia a HUD
        GameObject newHUD = Instantiate(playerHUD, playerHUDSlots, false);

        // Encontra o script de vida no jogador
        PlayerHealth playerHp = playerInput.GetComponent<PlayerHealth>();

        // Conecta a barra de vida com o HP do player
        HealthBar healthBarScript = newHUD.GetComponentInChildren<HealthBar>();
        playerHp.healthBar = healthBarScript;

        // Conecta as armas de cada player
        PlayerWeaponController playerWeaponController = playerInput.GetComponent<PlayerWeaponController>();
        AmmoCount ammoCount = newHUD.GetComponentInChildren<AmmoCount>();
        Image weaponSprite = newHUD.transform.Find("Weapons").GetComponentInChildren<Image>();

        playerWeaponController.ammoCount = ammoCount;
        playerWeaponController.weaponSprite = weaponSprite;

        // Conecta a pontuação de cada player
        PlayerStats playerStats = playerInput.GetComponent<PlayerStats>();
        ScoreCount scoreCount = newHUD.GetComponentInChildren<ScoreCount>();
        playerStats.scoreCount = scoreCount;

        // Conecta os slots de perks
        Transform perkSlots = newHUD.transform.Find("PerksSlots");
        playerStats.perkSlots = perkSlots;

        // Conecta a tela de morte para cada jogador
        playerHp.defeatScreen = defeatMenu;

        // Set HUD color for multiplayer
        PlayerHUDItens phi = newHUD.GetComponent<PlayerHUDItens>();
        SetPlayerHUDColor(phi, playerInput);

        // NEW: link this HUD to the player so we can destroy it on respawn
        var hudLink = playerObject.GetComponent<PlayerHUDLink>();
        if (hudLink == null)
            hudLink = playerObject.AddComponent<PlayerHUDLink>();

        hudLink.hudRoot = newHUD;
    }


    public void SetPlayerHUDColor(PlayerHUDItens phi, PlayerInput playerInput)
    {
        int playerIndex = playerInput.playerIndex;
        
        if (playerIndex == 0)
        {
            phi.healthBar.color = p1Color;
            phi.pointsIcon.color = p1Color;
        }
        if (playerIndex == 1)
        {
            phi.healthBar.color = p2Color;
            phi.pointsIcon.color = p2Color;
        }
        if (playerIndex == 2)
        {
            phi.healthBar.color = p3Color;
            phi.pointsIcon.color = p3Color;
        }
        if (playerIndex == 3)
        {
            phi.healthBar.color = p4Color;
            phi.pointsIcon.color = p4Color;
        }

    }
}
