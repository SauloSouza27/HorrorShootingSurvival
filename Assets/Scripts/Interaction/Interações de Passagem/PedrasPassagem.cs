using UnityEngine;
using System.Collections;

public class PedrasPassagem : MonoBehaviour
{
    [SerializeField] private float duracaoKinematic = 2f;
    [SerializeField] private float duracaoFadeOut = 2f;
    [SerializeField] private Transform[] pedras;
    private int quantidadePedras;
    private Renderer[] render;
    private Color corOriginal;

    private void Start()
    {
        quantidadePedras = transform.childCount;
        pedras = new Transform[quantidadePedras];
        render = new Renderer[quantidadePedras];

        for (int i = 0; i < quantidadePedras; i++)
        {
            pedras[i] = transform.GetChild(i).transform;
            render[i] = pedras[i].GetComponent<Renderer>();
        }

        // guarda a cor original do primeiro material (assumindo que todos têm a mesma)
        if (render.Length > 0)
            corOriginal = render[0].material.color;

        StartPedrasRigidBody();
    }

    private void StartPedrasRigidBody()
    {
        foreach (Transform t in pedras)
        {
            if (gameObject.GetComponent<Rigidbody>() != null)
            {
                t.GetComponent<Rigidbody>().isKinematic = false;
            }
        }

        // inicia o fade depois de um tempo
        Invoke(nameof(IniciarFadeOut), duracaoKinematic);
    }

    private void IniciarFadeOut()
    {
        StartCoroutine(FadeOutPedras());
    }

    private IEnumerator FadeOutPedras()
    {
        float tempo = 0f;
        Color corAtual = corOriginal;

        foreach (Transform t in pedras)
        {
            if (gameObject.GetComponent<Rigidbody>() != null)
            {
                t.GetComponent<Rigidbody>().isKinematic = true;
            }
        }

        foreach (Transform t in pedras)
        {
            if (gameObject.GetComponent<MeshCollider>() != null)
            {
                t.GetComponent<MeshCollider>().enabled = false;
            }
        }

        while (tempo < duracaoFadeOut - 0.1f)
        {
            tempo += Time.deltaTime;
            float tQuadratico = tempo * tempo;
            float alpha = Mathf.Lerp(1f, 0f, tQuadratico / duracaoFadeOut);// força o alpha de 1 até 0
            corAtual.a = alpha;

            foreach (Renderer r in render)
            {
                r.material.color = corAtual;
            }

            yield return null; // espera o próximo frame
        }

        // garante que o alpha chega a 0
        corAtual.a = 0f;
        foreach (Renderer r in render)
        {
            r.material.color = corAtual;
        }

        gameObject.SetActive(false);
    }
}
