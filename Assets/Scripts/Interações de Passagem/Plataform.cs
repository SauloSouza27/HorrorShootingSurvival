using UnityEngine;

public class Plataform : MonoBehaviour
{
    Vector3 startPosition;
    public float velocidade = 1.0f;
    public float alturaMax = 1.62f, alturaMin = 0.04f;
    bool flag = false;
    GameObject player;

    void Start()
    {
        startPosition = gameObject.transform.position;
        player = GameObject.FindWithTag("Player").gameObject;
    }

    void Update()
    {
        IsUpOrDown();
        PlataformMove();
    }

    private void PlataformMove()
    {
        if (!flag)
        {
            gameObject.transform.position += Vector3.up * Time.deltaTime * velocidade;
        }
        else
        {
            gameObject.transform.position += Vector3.up * Time.deltaTime * - velocidade;
        }
    }

    private void IsUpOrDown()
    {
        float currentYposition = transform.position.y;

        if (currentYposition >= alturaMax && flag == false)
        {
            flag = true;
        }
        if (currentYposition <= alturaMin && flag == true)
        {
            flag = false;
        }
    }
}
