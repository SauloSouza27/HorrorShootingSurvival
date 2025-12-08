using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveFX : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer skinnedMesh;

    private Material[] skinnedMaterials;

    [SerializeField] private float dissolveRate = 0.0125f;
    [SerializeField] private float refreshrate = 0.025f;

    //[SerializeField] private Transform[] eyes = new Transform[2];
    //[SerializeField] private float timeFadeOutEyes = 1f;

    void Start()
    {
        if (skinnedMesh != null)
        {
            skinnedMaterials = skinnedMesh.materials;
        }
    }

    public void Dissolve()
    {
        StartCoroutine(DissolveCo());
        //StartCoroutine(FadeOutEyes());
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

    //private IEnumerator FadeOutEyes()
    //{
    //    Light light = eyes[0].transform.GetComponent<Light>();
    //    float tempo = 0f;
    //    float intensidadeAtual = light.intensity;

    //    while (tempo < timeFadeOutEyes - 0.1f)
    //    {
    //        tempo += Time.deltaTime;
    //        float tQuadratico = tempo * tempo;
    //        float intensidade = Mathf.Lerp(1f, 0f, tQuadratico / timeFadeOutEyes);// força o alpha de 1 até 0
    //        intensidadeAtual = intensidade;

    //        foreach (Transform t in eyes)
    //        {
    //            t.GetComponent<Light>().intensity = intensidade;
    //        }

    //        yield return null; // espera o próximo frame
    //    }
    //}
}