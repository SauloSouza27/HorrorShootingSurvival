using UnityEngine;

public class FloatingRotatingItem : MonoBehaviour
{
    [Header("Floating")]
    [SerializeField] private float floatAmplitude = 0.25f;   // how high it moves up/down
    [SerializeField] private float floatFrequency = 1.5f;    // how fast it bobs

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 60f;      // degrees per second around Y

    private Vector3 startPosition;
    private float phaseOffset;

    private void OnEnable()
    {
        // Save starting position each time it’s enabled (good for pooled objects)
        startPosition = transform.position;

        // Little random offset so multiple items don't bob in perfect sync
        phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        // Vertical bobbing
        float yOffset = Mathf.Sin((Time.time + phaseOffset) * floatFrequency) * floatAmplitude;
        Vector3 newPos = startPosition + new Vector3(0f, yOffset, 0f);
        transform.position = newPos;

        // Spin around Y (top-down friendly)
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}