using UnityEngine;
using TMPro;
using System;

public class BuyWeaponWorldUI : MonoBehaviour
{

    public Canvas canvas;
    public TextMeshProUGUI weaponName;
    public TextMeshProUGUI weaponPrice;
    public TextMeshProUGUI interactDescription;

    public Camera uiCamera;




    void Start()
    {
        if(!uiCamera) uiCamera = Camera.main;
        interactDescription.text = "Press interaction to purchase";
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

    public void SetupBuyWeaponCanvas(String weaponNameInput, float weaponPriceInput)
    {
        weaponName.text = weaponNameInput;
        weaponPrice.text = "Buy weapon - Cost: " + weaponPriceInput;
    }

    public void SetupBuyAmmoCanvas(String weaponNameInput, float ammoPrice)
    {
        weaponName.text = weaponNameInput;
        weaponPrice.text = "Buy ammo - Cost: " + ammoPrice;
    }





}
