using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponController : MonoBehaviour
{
    private PlayerInput playerInput; // Reference to PlayerInput
    private InputAction fireAction; // Fire action input
    private Animator animator;
    
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private Transform gunPoint;

    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform aim;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>(); // Get the PlayerInput component
        animator = GetComponentInChildren<Animator>(); // Get the Animator from children
        
        AssignInputEvents(); // Assign the input actions
        
    }

    private void AssignInputEvents()
    {
        var controls = playerInput.actions; // Get the controls from PlayerInput

        // Fire action
        fireAction = controls["Fire"]; // Bind to Fire action in Input system
        fireAction.performed += ctx => Shoot(); // When performed, call Shoot()
    }

    private void Shoot()
    {
        
        
        GameObject newBullet = Instantiate(bulletPrefab, gunPoint.position, Quaternion.LookRotation(gunPoint.forward));
        
        newBullet.GetComponent<Rigidbody>().linearVelocity = BulletDirection() * bulletSpeed;
        
        Destroy(newBullet, 10);
        animator.SetTrigger("Fire"); // Trigger the fire animation
    }

    public Vector3 BulletDirection()
    {
        Vector3 direction = (aim.position - gunPoint.position).normalized;
        
        direction.y = 0;
        
        weaponHolder.LookAt(aim);
        gunPoint.LookAt(aim);
        
        return direction;
    }

    public Transform GunPoint() => gunPoint;

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(weaponHolder.position, weaponHolder.position + weaponHolder.forward * 25);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(gunPoint.position, gunPoint.position + BulletDirection() * 25);
    }
}