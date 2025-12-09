using UnityEngine;
using UnityEngine.UI;

public class ReviveRescuerWorldUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image fillImage;
    [SerializeField] private Camera uiCamera;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.0f, 0f);

    private bool isActive;

    private void Awake()
    {
        if (uiCamera == null)
            uiCamera = Camera.main;

        if (canvas == null)
            canvas = GetComponentInChildren<Canvas>(true);
    }

    private void OnEnable()
    {
        if (canvas == null)
        {
            Debug.LogError("ReviveRescuerWorldUI: Canvas not assigned.", this);
            return;
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = uiCamera != null ? uiCamera : Camera.main;
        canvas.gameObject.SetActive(false);

        if (fillImage != null)
            fillImage.fillAmount = 0f;
    }

    private void LateUpdate()
    {
        if (canvas == null || !isActive)
            return;

        // The canvas object itself should already be positioned over the player in the prefab,
        // we only need to face the camera.
        if (uiCamera == null)
            uiCamera = Camera.main;

        if (uiCamera != null)
        {
            var toCam = canvas.transform.position - uiCamera.transform.position;
            canvas.transform.rotation = Quaternion.LookRotation(toCam, Vector3.up);
        }
    }

    public void BeginRevive()
    {
        isActive = true;

        if (canvas != null)
            canvas.gameObject.SetActive(true);

        if (fillImage != null)
            fillImage.fillAmount = 0f;
    }

    public void SetProgress(float value01)
    {
        if (fillImage != null)
        {
            //Debug.Log("fill ammount" + Mathf.Clamp01(value01));
            fillImage.fillAmount = Mathf.Clamp01(value01);
        }
    }

    public void EndRevive()
    {
        isActive = false;

        if (fillImage != null)
            fillImage.fillAmount = 0f;

        if (canvas != null)
            canvas.gameObject.SetActive(false);
    }
}
