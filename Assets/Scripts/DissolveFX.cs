using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveController : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer baseEnemy;

    private Material[] skinnedMaterials;

    [SerializeField] private float dissolveRate = 0.0125f;
    [SerializeField] private float refreshrate = 0.025f;

    void Start()
    {
        if (baseEnemy != null)
        {
            skinnedMaterials = baseEnemy.materials;
        }
    }

    public void Dissolve()
    {
        StartCoroutine(DissolveCo());
    }

    IEnumerator DissolveCo()
    {
        if(skinnedMaterials.Length > 0)
        {
            float counter1 = 0;
            while (skinnedMaterials[0].GetFloat("_DissolveAmount") < 1)
            {
                counter1 += dissolveRate;
                for (int i = 0; i < skinnedMaterials.Length; i++)
                {
                    skinnedMaterials[i].SetFloat("_DissolveAmount", counter1);
                }

                yield return new WaitForSeconds(refreshrate);
            }
        }
    }
}