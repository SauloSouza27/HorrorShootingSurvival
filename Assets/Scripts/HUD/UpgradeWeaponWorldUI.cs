using UnityEngine;
using TMPro;
using System;

public class UpgradeWeaponWorldUI : MonoBehaviour
{

    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI upgradeName;
    [SerializeField] private TextMeshProUGUI priceDescription;
    [SerializeField] private TextMeshProUGUI interactDescription;

    [SerializeField] private Camera uiCamera;




    void Start()
    {
        if(!uiCamera) uiCamera = Camera.main;
        interactDescription.text = "";
    }

    void Update()
    {
        FaceCamera();
    }


    public void ShowUI()
    {
        canvas.gameObject.SetActive(true);
    }

    public void HideUI()
    {
        canvas.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if(!canvas)
        {
            Debug.LogError("Canvas reference missing.", this);
            return;
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = uiCamera;
        canvas.gameObject.SetActive(false);
    }



    void FaceCamera()
    {
        if(!canvas || !uiCamera) return;
        var toCam = canvas.transform.position - uiCamera.transform.position;
        canvas.transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
    }

    public void SetupUpgradeWeaponCanvas(String weaponNameInput, int tier, int upgradeCost)
    {
        upgradeName.text = weaponNameInput + " +" + (tier + 1);
        priceDescription.text = "Upgrade $ " + upgradeCost;
        interactDescription.text = "Press interact to upgrade weapon";
    }

    public void SetupMaxedWeaponCanvas()
    {
        upgradeName.text = "";
        priceDescription.text = "Maxed Weapon!";
        interactDescription.text = "";
    }

}
