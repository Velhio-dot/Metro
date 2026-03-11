using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BannerInteraction : MonoBehaviour, IInteractable
{
    [Header("Banner Settings")]
    [SerializeField] private string bannerText = "Здесь могла быть ваша реклама";
    [SerializeField] private float displayDuration = 3f;

    [Header("UI References")]
    [SerializeField] private GameObject bannerPanel; // Панель внизу экрана
    [SerializeField] private TextMeshProUGUI bannerTextUI; // Текст на панели

    private bool isDisplaying = false;
    private float displayTimer = 0f;

    void Start()
    {
        // Автоматически находим UI если не назначен
        if (bannerPanel == null)
        {
            bannerPanel = GameObject.Find("BannerPanel");
        }

        if (bannerTextUI == null && bannerPanel != null)
        {
            bannerTextUI = bannerPanel.GetComponentInChildren<TextMeshProUGUI>();
        }

        // Скрываем панель при старте
        if (bannerPanel != null)
        {
            bannerPanel.SetActive(false);
        }
    }

    void Update()
    {
        // Таймер отображения
        if (isDisplaying)
        {
            displayTimer -= Time.deltaTime;
            if (displayTimer <= 0f)
            {
                HideBanner();
            }
        }
    }

    // Реализация интерфейса IInteractable
    public void Interact()
    {
        ShowBanner();
    }

    public void ShowBanner()
    {
        if (bannerPanel != null && bannerTextUI != null)
        {
            bannerTextUI.text = bannerText;
            bannerPanel.SetActive(true);
            isDisplaying = true;
            displayTimer = displayDuration;

            Debug.Log($"Показан баннер: {bannerText}");
        }
        else
        {
            Debug.LogError("BannerInteraction: UI элементы не найдены!");
        }
    }

    public void HideBanner()
    {
        if (bannerPanel != null)
        {
            bannerPanel.SetActive(false);
            isDisplaying = false;
        }
    }

    // Для отображения в инспекторе
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}