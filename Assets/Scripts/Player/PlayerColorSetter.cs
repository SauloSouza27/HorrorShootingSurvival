using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerColorSetter : MonoBehaviour
{
    [SerializeField] private Color[] playerColors;
    [SerializeField] private int targetMaterialIndex = 0; // Set this per part in the Inspector
    public int playerIndex;
    private void Start()
    {
        playerIndex = GetComponent<PlayerInput>().playerIndex;
        Color colorToApply = playerColors[playerIndex % playerColors.Length];

        ApplyColorToParts(colorToApply);
    }

    

    private void ApplyColorToParts(Color color)
    {
        var parts = GetComponentsInChildren<PlayerColorPart>();
        int playerIndex = GetComponent<PlayerInput>().playerIndex;

        foreach (var part in parts)
        {
            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer == null) continue;

            Debug.Log($"Coloring {part.name} for player {playerIndex} with color {color}");

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block, part.targetMaterialIndex);
            block.SetColor("_BaseColor", color);
            renderer.SetPropertyBlock(block, part.targetMaterialIndex);
        }
    }
}