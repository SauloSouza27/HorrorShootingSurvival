using System;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    protected MeshRenderer mesh;
    public virtual bool RequiresPlayer => false;

    [SerializeField] private Material highlightMaterial;
    protected Material defaultMaterial;

    private void Start()
    {
        if (mesh == null)
            mesh = GetComponentInChildren<MeshRenderer>();

        defaultMaterial = mesh.sharedMaterial;
    }

    // Back-compat overload: still here if something calls it without a Player
    public virtual void Interaction()
    {
        // Default behavior when NO player context is needed
        Debug.Log("Interacted with " + gameObject.name);
    }

    // ✅ Null-safe: no null deref when player is not provided
    public virtual void Interaction(Player player)
    {
        // Default behavior when a player is passed in
        Debug.Log("Interacted with " + gameObject.name + " by " + player?.name);
    }

    protected void UpdateMeshAndMaterial(MeshRenderer newMesh)
    {
        mesh = newMesh;
        defaultMaterial = newMesh.sharedMaterial;
    }

    public void HighlightActive(bool active)
    {
        mesh.material = active ? highlightMaterial : defaultMaterial;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        var playerInteraction = other.GetComponent<PlayerInteraction>();
        if (playerInteraction == null) return;

        playerInteraction.GetInteractables().Add(this);
        playerInteraction.UpdateClosestInteractable();
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        var playerInteraction = other.GetComponent<PlayerInteraction>();
        if (playerInteraction == null) return;

        playerInteraction.GetInteractables().Remove(this);
        playerInteraction.UpdateClosestInteractable();
    }
}

