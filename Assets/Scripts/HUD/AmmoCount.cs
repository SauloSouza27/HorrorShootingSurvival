using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AmmoCount : MonoBehaviour
{
    public TextMeshProUGUI ammoText;

    public void UpdateAmmo(int currentAmmo, int totalAmmo)
    {
        ammoText.text = currentAmmo + " / " + totalAmmo;
    }

}
