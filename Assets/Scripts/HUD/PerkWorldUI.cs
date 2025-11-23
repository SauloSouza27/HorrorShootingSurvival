using TMPro;
using Unity.VisualScripting;
using Unity.Cinemachine;
using UnityEngine.UI;
using System;
using UnityEngine;

public class PerkWorldUI : MonoBehaviour
{
    public Canvas canvas;
    public TextMeshProUGUI perkName;
    public TextMeshProUGUI perkDescription;
    public TextMeshProUGUI perkPrice;

    public Camera uiCamera;




    void Start()
    {
        if(!uiCamera) uiCamera = Camera.main;
        PerkMachine perkMachine = transform.GetComponentInParent<PerkMachine>();
        PerkType perkType = perkMachine.GetPerkType();
        perkName.text = perkMachine.GetPerkName(perkType);
        perkDescription.text = perkMachine.GetPerkDescription(perkType);
        perkPrice.text = "Price: " + perkMachine.GetPerkPrice();
        
    }

    void Update()
    {
        FaceCamera();
    }

    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player")) return;
        ShowUI();
    }

    void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag("Player")) return;
        HideUI();
    }

    void ShowUI()
    {
        canvas.gameObject.SetActive(true);
    }

    void HideUI()
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


}
