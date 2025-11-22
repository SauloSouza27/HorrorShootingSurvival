using UnityEngine;

public class DrawGizmos : MonoBehaviour
{

    [SerializeField] private Color color = Color.green;
    void OnDrawGizmos()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
