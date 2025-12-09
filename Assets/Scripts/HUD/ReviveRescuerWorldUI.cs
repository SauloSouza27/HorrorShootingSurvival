using UnityEngine;
using UnityEngine.UI;

public class ReviveRescuerWorldUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Slider progressSlider;
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

        if (progressSlider == null && canvas != null)
            progressSlider = canvas.GetComponentInChildren<Slider>(true);
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

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
        }
    }

    private void LateUpdate()
    {
        if (canvas == null || !isActive)
            return;

        if (uiCamera == null)
            uiCamera = Camera.main;

        // ⬇️ Position the canvas above the rescuer using worldOffset
        Vector3 targetPos = transform.position + worldOffset;
        canvas.transform.position = targetPos;

        // Face the camera
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

        if (progressSlider != null)
            progressSlider.value = 0f;
    }

    public void SetProgress(float value01)
    {
        if (progressSlider != null)
            progressSlider.value = Mathf.Clamp01(value01);
    }

    public void EndRevive()
    {
        isActive = false;

        if (progressSlider != null)
            progressSlider.value = 0f;

        if (canvas != null)
            canvas.gameObject.SetActive(false);
    }
}
