using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDItens : MonoBehaviour
{
    [SerializeField] private Image _healthBar, _pointsIcon;
    public Image healthBar { get; set; }
    public Image pointsIcon { get; set; }

    private void Awake()
    {
        healthBar = _healthBar;
        pointsIcon = _pointsIcon;
    }
}
