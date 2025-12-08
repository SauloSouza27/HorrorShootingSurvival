using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerColorSetter : MonoBehaviour
{
    [ColorUsage(true, true)] [SerializeField] private Color[] playerColors;
    //[SerializeField] private int targetMaterialIndex = 0; // Set this per part in the Inspector
    [SerializeField] private Material material;
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
            SkinnedMeshRenderer renderer = part.GetComponent<SkinnedMeshRenderer>();
            if (renderer == null) continue;
            

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block, part.targetMaterialIndex);
            block.SetColor("_EmissionColor", color);
            renderer.SetPropertyBlock(block, part.targetMaterialIndex);
        }
    }
}