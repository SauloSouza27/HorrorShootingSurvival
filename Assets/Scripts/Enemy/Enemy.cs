using UnityEngine;


public class ZombieEnemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float damageDistance = 1.5f;

    private Player targetPlayer;

    private void Start()
    {
        targetPlayer = FindObjectOfType<Player>(); // Simple single-player target
    }

    private void Update()
    {
        if (targetPlayer == null) return;

        // Move toward player
        Vector3 direction = (targetPlayer.transform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // Face the player
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

        // Check distance to "kill"
        float distance = Vector3.Distance(transform.position, targetPlayer.transform.position);
        if (distance <= damageDistance)
        {
            //IDamageable damageable = targetPlayer.gameObject.GetComponent<IDamageable>();
            //damageable?.TakeDamage();
            KillPlayer();
        }
    }
    
    

    private void KillPlayer()
    {
        Debug.Log("Player has been killed by a zombie.");
        GameManager.Instance.GameOver();
    }
}