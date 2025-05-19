using UnityEngine;

public enum EquipType { SideEquip, BackEquip };
public enum HoldType { CommonHold = 1, LowHold, HighHold}

public class WeaponModel : MonoBehaviour
{
    public WeaponType weaponType;
    public EquipType equipType;
    public HoldType holdType;

    public Transform gunPoint;
    public Transform holdPoint;
}
