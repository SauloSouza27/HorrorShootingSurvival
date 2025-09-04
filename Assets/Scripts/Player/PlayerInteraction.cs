using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private List<Interactable> interactables = new List<Interactable>();
    private Interactable closestInteractable;
    


    private void Start()
    {
        Player player = GetComponent<Player>();

        player.controls.Character.Interaction.performed += context => InteractWithClosest();
    }
    
    private void InteractWithClosest()
    {
        // Clean dead/disabled entries first
        interactables.RemoveAll(i => i == null || !i.isActiveAndEnabled);

        if (closestInteractable == null) return;

        var player = GetComponent<Player>();
        var target = closestInteractable; // copy in case closest changes during call

        target.Interaction(player);

        // If the interactable wants to be removed (e.g., pickup), remove it.
        if (target != null && target.RemoveAfterInteract)
        {
            interactables.Remove(target);
        }

        UpdateClosestInteractable();
    }

    public void UpdateClosestInteractable()
    {
        // Clean dead/disabled entries
        interactables.RemoveAll(i => i == null || !i.isActiveAndEnabled);

        // Un-highlight old
        closestInteractable?.HighlightActive(false);

        closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (var i in interactables)
        {
            if (i == null) continue;
            float d = Vector3.Distance(transform.position, i.transform.position);
            if (d < closestDistance)
            {
                closestDistance = d;
                closestInteractable = i;
            }
        }

        closestInteractable?.HighlightActive(true);
    }
    
    public List<Interactable> GetInteractables() => interactables;
}
