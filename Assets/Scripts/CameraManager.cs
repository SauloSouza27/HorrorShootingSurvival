using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup targetGroup;
    private PlayerInputManager playerInputManager;
    public GameObject playerHUD;
    public Transform playerHUDSlots;
    public GameObject defeatMenu;

    void Start()
    {
        playerInputManager = GetComponent <PlayerInputManager>();
        playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    void OnPlayerJoined(PlayerInput playerInput)
    {
        GameObject playerObject = playerInput.gameObject;

        targetGroup.AddMember(playerObject.transform, 1f, 0f);

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

        // Conecta os slots de perks (não funciona ainda?)
        Transform playerPerksSlots = newHUD.transform.Find("PerksSlots");

        // Conecta a tela de morte para cada jogador
        playerHp.defeatScreen = defeatMenu;
        
    }
}