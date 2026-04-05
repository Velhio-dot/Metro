using UnityEngine;
using TMPro;
using System;
using System.Collections;

public class BannerManager : MonoBehaviour
{
    public static BannerManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject bannerPanel;
    [SerializeField] private TextMeshProUGUI bannerText;

    [Header("Настройки")]
    [SerializeField] private float defaultDuration = 3f;

    public event Action OnBannerHidden;

    private Coroutine currentBanner;
    private bool isShowing = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (bannerPanel != null)
            {
                bannerPanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowBanner(string text, float duration = -1)
    {
        Debug.Log($"[BannerManager] ShowBanner called! Text: '{text}'");

        if (currentBanner != null)
            StopCoroutine(currentBanner);

        if (duration < 0)
            duration = defaultDuration;

        if (bannerText != null)
        {
            bannerText.text = text;
            Debug.Log($"[BannerManager] Text updated on bannerText.");
        }
        else
        {
            Debug.LogError("[BannerManager] FATAL ERROR: bannerText reference is NULL in the inspector!");
        }
            
        if (bannerPanel != null)
        {
            bannerPanel.SetActive(true);
            Debug.Log($"[BannerManager] bannerPanel activated. IsActiveInHierarchy: {bannerPanel.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("[BannerManager] FATAL ERROR: bannerPanel reference is NULL in the inspector!");
        }
            
        isShowing = true;

        currentBanner = StartCoroutine(HideAfterDelay(duration));
    }

    IEnumerator HideAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        HideBanner();
    }

    public void HideBanner()
    {
        if (bannerPanel != null)
            bannerPanel.SetActive(false);

        isShowing = false;
        currentBanner = null;
        OnBannerHidden?.Invoke();
    }

    public bool IsShowing => isShowing;
}
