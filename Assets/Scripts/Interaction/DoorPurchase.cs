using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorOpenMode { DisableBlockers, Slide, Rotate }

public class DoorPurchase : Interactable
{
    public override bool RemoveAfterInteract => true;

    [Header("Purchase")]
    [SerializeField] private int cost = 750;
    public int Cost => cost;

    [Header("Open Behaviour")]
    [SerializeField] private DoorOpenMode openMode = DoorOpenMode.DisableBlockers;
    [SerializeField] private List<GameObject> blockers = new List<GameObject>();
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Vector3 slideOffset = new Vector3(0, 0, 2f);
    [SerializeField] private float slideTime = 0.6f;
    [SerializeField] private Vector3 rotateAngles = new Vector3(0f, 90f, 0f);
    [SerializeField] private float rotateTime = 0.5f;

    [Header("Stones Phisics Mode")]
    [SerializeField] private float duracaoKinematic = 2f;
    [SerializeField] private float duracaoFadeOut = 2f;
    [SerializeField] private Transform[] pedras;
    private int quantidadePedras;
    private Renderer[] render;
    private Color corOriginal;
    
    [Header("Audio")]
    [SerializeField] private AudioClip openSFX;
    [Range(0f, 1f)] [SerializeField] private float openVolume = 1f;
    [SerializeField] private AudioClip failSFX;
    [Range(0f, 1f)] [SerializeField] private float failVolume = 1f;

    [Header("DEBUG Prompt")]
    [SerializeField] private bool debugPriceUI = true;
    [SerializeField] private Vector3 uiWorldOffset = new Vector3(0, 2f, 0);
    
    

    private bool opened;
    public bool IsOpened => opened;

    // track who is in range to show the prompt
    private readonly HashSet<Player> playersInRange = new HashSet<Player>();

    private void Awake()
    {
        // get pedras filhas
        quantidadePedras = transform.childCount - 1;
        pedras = new Transform[quantidadePedras];
        render = new Renderer[quantidadePedras];

        for (int i = 0; i < render.Length; i++)
        {
            pedras[i] = transform.GetChild(i);
            render[i] = pedras[i].GetComponent<Renderer>();
        }

        // guarda a cor original do primeiro material (assumindo que todos t�m a mesma)
        if (render.Length > 0)
            corOriginal = render[0].material.color;
    }

    public override void Interaction(Player player)
    {
        if (opened || player == null) return;

        var stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;

        if (!stats.SpendPoints(cost))
        {
            Play3D(failSFX, failVolume);
            Debug.Log("Not enough points to open door.");
            return;
        }

        Play3D(openSFX, openVolume);

        StartPedrasRigidBody();
        //StartCoroutine(OpenSequence());
    }

    // Abrir porta modo pedras caindo
    private void StartPedrasRigidBody()
    {
        DoorWorldUI doorWorldUI = transform.GetComponent<DoorWorldUI>();
        doorWorldUI.DestroyUI();

        opened = true;
        HighlightActive(false);
        this.gameObject.GetComponent<Renderer>().enabled = false;

        foreach (Transform t in pedras)
        {
            t.GetComponent<MeshRenderer>().enabled = true;
            t.GetComponent<Rigidbody>().isKinematic = false;
            t.gameObject.layer = LayerMask.NameToLayer("Ignore Player Collision");
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

        //foreach (Transform t in pedras)
        //{
        //    t.GetComponent<Rigidbody>().isKinematic = true;
        //}

        foreach (Transform t in pedras)
        {
            t.GetComponent<MeshCollider>().enabled = false;
        }

        while (tempo < duracaoFadeOut - 0.1f)
        {
            tempo += Time.deltaTime;
            float tQuadratico = tempo * tempo;
            float alpha = Mathf.Lerp(1f, 0f, tQuadratico / duracaoFadeOut);// for�a o alpha de 1 at� 0
            corAtual.a = alpha;

            foreach (Renderer r in render)
            {
                r.material.color = corAtual;
            }

            yield return null; // espera o pr�ximo frame
        }

        // garante que o alpha chega a 0
        corAtual.a = 0f;
        foreach (Renderer r in render)
        {
            r.material.color = corAtual;
        }

        gameObject.SetActive(false);
    }

    private IEnumerator OpenSequence()
    {
        opened = true;
        HighlightActive(false);

        switch (openMode)
        {
            case DoorOpenMode.DisableBlockers:
                DisableBlockers();
                break;
            case DoorOpenMode.Slide:
                if (doorTransform) yield return SlideRoutine(doorTransform, slideOffset, slideTime);
                DisableBlockers();
                break;
            case DoorOpenMode.Rotate:
                if (doorTransform) yield return RotateRoutine(doorTransform, rotateAngles, rotateTime);
                DisableBlockers();
                break;
        }
    }

    private void DisableBlockers()
    {
        foreach (var go in blockers)
        {
            if (!go) continue;
            foreach (var col in go.GetComponentsInChildren<Collider>(true)) col.enabled = false;
            go.SetActive(false);
        }
    }

    private IEnumerator SlideRoutine(Transform t, Vector3 offset, float duration)
    {
        Vector3 a = t.position, b = a + offset;
        float u = 0f;
        while (u < 1f) { u += Time.deltaTime / Mathf.Max(0.01f, duration); t.position = Vector3.Lerp(a, b, Mathf.SmoothStep(0,1,u)); yield return null; }
        t.position = b;
    }

    private IEnumerator RotateRoutine(Transform t, Vector3 angles, float duration)
    {
        Quaternion a = t.rotation, b = a * Quaternion.Euler(angles);
        float u = 0f;
        while (u < 1f) { u += Time.deltaTime / Mathf.Max(0.01f, duration); t.rotation = Quaternion.Slerp(a, b, Mathf.SmoothStep(0,1,u)); yield return null; }
        t.rotation = b;
    }

    // --- Interactable proximity tracking (to show prompt) ---
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        var p = other.GetComponent<Player>();
        if (p != null) playersInRange.Add(p);
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        var p = other.GetComponent<Player>();
        if (p != null) playersInRange.Remove(p);
    }
    
    private void Play3D(AudioClip clip, float volume)
    {
        if (clip == null || AudioManager.Instance == null) return;

        AudioManager.Instance.PlaySFX3D(
            clip,
            transform.position,
            volume,
            spatialBlend: 1f,
            minDistance: 4f,
            maxDistance: 35f
        );
    }

    private void OnGUI()
    {
        if (!debugPriceUI || IsOpened) return;
        if (playersInRange.Count == 0) return;
        var cam = Camera.main; if (!cam) return;

        // choose the nearest player (so text reflects *their* points)
        Player nearest = null; float minD = float.MaxValue;
        foreach (var p in playersInRange)
        {
            if (!p) continue;
            float d = Vector3.Distance(p.transform.position, transform.position);
            if (d < minD) { minD = d; nearest = p; }
        }
        if (!nearest) return;

        var stats = nearest.GetComponent<PlayerStats>();
        int points = stats ? stats.GetPoints() : 0;
        bool canAfford = stats && stats.CanAfford(cost);

        Vector3 screen = cam.WorldToScreenPoint(transform.position + uiWorldOffset);
        if (screen.z < 0) return;
        screen.y = Screen.height - screen.y;

        var rect = new Rect(screen.x - 120, screen.y - 40, 240, 38);
        GUI.color = new Color(0,0,0,0.7f);
        GUI.Box(rect, GUIContent.none);
        GUI.color = Color.white;

        string line1 = canAfford ? $"Press Interact to buy  ({cost})" : $"Not enough points  ({points}/{cost})";
        GUI.Label(new Rect(rect.x + 8, rect.y + 8, rect.width - 16, 22), line1);
    }

    public int GetCost()
    {
        return cost;
    }
}
