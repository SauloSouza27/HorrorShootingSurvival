using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PulseAndDisapear : MonoBehaviour
{
    [Header("Press to Play Button")]
    [SerializeField] private TextMeshProUGUI pressButtontext;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float lifetimePressButton = 2f;

    private float timerPressButton;
    private bool growing = true;

    [Header("Get Ready")]
    [SerializeField] private TextMeshProUGUI getReadytext;
    [SerializeField] private float timeToGetReady = 10f;
    [SerializeField] float targetScale = 2000f;
    [SerializeField] [Range(0.01f, 1f)] float growFactor = 0.5f;

    private float timerGetReady;
    private float scale = 1f;

    PlayerInputManager playerInputManager;
    private bool playerJoined = false;

    void Start()
    {
        timerPressButton = lifetimePressButton;
        timerGetReady = timeToGetReady;

        playerInputManager = GetComponent<PlayerInputManager>();
        playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    void OnPlayerJoined(PlayerInput playerInput)
    {
        playerJoined = true;
    }

    void Update()
    {
        if (pressButtontext == null) return;

        PressButtonToPlay();

        if (getReadytext == null) return;

        GetReadyToSurvive();
    }

    public void PressButtonToPlay()
    {
        // Timer pra sumir
        if (playerJoined)
        {
            timerPressButton -= Time.deltaTime;
        }
        if (timerPressButton <= 0f)
        {
            Destroy(pressButtontext.gameObject); // destrói o objeto do texto
            return;
        }

        // Efeito de pulsar (crescer/diminuir)
        Vector3 scale = pressButtontext.transform.localScale;

        if (growing)
        {
            scale += Vector3.one * speed * Time.deltaTime;
            if (scale.x >= maxScale) growing = false;
        }
        else
        {
            scale -= Vector3.one * speed * Time.deltaTime;
            if (scale.x <= minScale) growing = true;
        }

        pressButtontext.transform.localScale = scale;
    }

    public void GetReadyToSurvive()
    {
        if (!playerJoined) return;

        if (timerGetReady > 0f)
        {
            timerGetReady -= Time.deltaTime;
            return;
        }
        if (timerGetReady <= 0f && !getReadytext.gameObject.activeSelf)
        {
            getReadytext.gameObject.SetActive(true);
        }

        // Crescimento exponencial (aumenta cada vez mais rápido)
        scale += growFactor * Time.deltaTime * (scale);

        getReadytext.transform.localScale = new Vector3(scale, scale, scale);

        // quando atingir o tamanho desejado, some
        if (getReadytext.transform.localScale.x >= targetScale)
        {
            Destroy(getReadytext.gameObject);
        }
    }
}
