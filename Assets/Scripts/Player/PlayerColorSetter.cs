using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerColorSetter : MonoBehaviour
{
    [SerializeField] private Color[] playerColors;
    //[SerializeField] private int targetMaterialIndex = 0; // Set this per part in the Inspector
    [SerializeField] private Material material;
    public int playerIndex;
    private Color colorToApply;

    private void Start()
    {
        playerIndex = GetComponent<PlayerInput>().playerIndex;
        colorToApply = playerColors[playerIndex % playerColors.Length];

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
            block.SetColor("_EmissionColor", color * 1.5f);
            renderer.SetPropertyBlock(block, part.targetMaterialIndex);
        }
    }
}