using UnityEngine;
using TMPro;

public class DoorWorldUI : MonoBehaviour
{
    public Canvas canvas;
    public TextMeshProUGUI doorPrice;

    public Camera uiCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(!uiCamera) uiCamera = Camera.main;
        DoorPurchase doorPurchase = transform.GetComponentInParent<DoorPurchase>();
        doorPrice.text = "$ " + doorPurchase.GetCost();        
    }

    void Update()
    {
        FaceCamera();
    }

    void FaceCamera()
    {
        if(!canvas || !uiCamera) return;
        var toCam = canvas.transform.position - uiCamera.transform.position;
        canvas.transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
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

    public void DestroyUI()
    {
        canvas.enabled = false;
    }

}
