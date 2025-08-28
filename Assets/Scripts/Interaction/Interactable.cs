using UnityEngine;

public class Interactable : MonoBehaviour
{
    // ⬇️ Use Renderer (works for MeshRenderer or SkinnedMeshRenderer)
    [SerializeField] protected Renderer mesh;

    [SerializeField] private Material highlightMaterial;
    protected Material defaultMaterial;

    // ⬇️ Let children disable highlight (ReviveTarget will override)
    public virtual bool SupportsHighlight => true;

    private void Start()
    {
        if (mesh == null)
            mesh = GetComponentInChildren<Renderer>(true); // safe lookup

        if (mesh != null)
            defaultMaterial = mesh.sharedMaterial;
    }

    protected void UpdateMeshAndMaterial(Renderer newMesh)
    {
        mesh = newMesh;
        defaultMaterial = newMesh != null ? newMesh.sharedMaterial : null;
    }

    public void HighlightActive(bool active)
    {
        if (!SupportsHighlight || mesh == null || highlightMaterial == null || defaultMaterial == null)
            return; // ⬅️ null-safe bailouts

        mesh.material = active ? highlightMaterial : defaultMaterial;
    }

    public virtual void Interaction(Player player)
    {
        Debug.Log($"Interacted with {name} by {player?.name}");
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
