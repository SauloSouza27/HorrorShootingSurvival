using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AmmoCount : MonoBehaviour
{
    public TextMeshProUGUI ammoText;

    public void UpdateAmmo(int currentAmmo, int maxAmmo, int totalAmmo)
    {
        ammoText.text = "Ammo: " + currentAmmo + " / " + maxAmmo + " (" + totalAmmo + ")";
    }

}
