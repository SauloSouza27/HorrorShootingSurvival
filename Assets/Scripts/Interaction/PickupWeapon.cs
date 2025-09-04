using UnityEngine;

public class PickupWeapon : Interactable
{
    public bool RequiresPlayer => true;

    [SerializeField] private Weapon_Data weaponData;
    [SerializeField] private Weapon weapon;
    [SerializeField] private BackupWeaponModel[] models;

    private bool oldWeapon;
    private bool isClaimed;
    private Collider pickupCollider;
    
    public new virtual bool RemoveAfterInteract => true;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider>();
        if (pickupCollider == null)
            pickupCollider = gameObject.AddComponent<SphereCollider>(); // fallback
        // keep as non-trigger if you want physics pickup area; trigger is fine too
        // ((SphereCollider)pickupCollider).isTrigger = true; // optional
    }

    private void Start()
    {
        if (!oldWeapon)
            weapon = new Weapon(weaponData);

        UpdateGameObject();
    }

    public void SetupPickupWeapon(Weapon newWeapon, Transform from)
    {
        oldWeapon = true;
        weapon = newWeapon;
        weaponData = newWeapon.WeaponData;

        transform.position = from.position + new Vector3(0f, .75f, 0f);
        UpdateGameObject();
    }

    [ContextMenu("UpdateItemModel")]
    public void UpdateGameObject()
    {
        gameObject.name = "PickupWeapon - " + weaponData.weaponType;
        UpdateItemModel();
    }

    public void UpdateItemModel()
    {
        foreach (var model in models)
        {
            model.gameObject.SetActive(false);
            if (model.WeaponType == weaponData.weaponType)
            {
                model.gameObject.SetActive(true);
                UpdateMeshAndMaterial(model.GetComponent<MeshRenderer>());
            }
        }
    }

    // ✅ Only the player-based interaction exists now
    public override void Interaction(Player player)
    {
        if (isClaimed || player == null) return;

        var weaponController = player.GetComponent<PlayerWeaponController>();
        if (weaponController == null) return;

        // claim immediately to avoid race with another player
        isClaimed = true;
        if (pickupCollider) pickupCollider.enabled = false;
        HighlightActive(false);

        weaponController.PickupWeapon(weapon);

        // Return to pool / destroy
        //ObjectPool.instance.ReturnObject(0, gameObject);
        Destroy(gameObject);
    }
    
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        // No caching a controller here—pickup strictly uses the interacting player argument.
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }
}
