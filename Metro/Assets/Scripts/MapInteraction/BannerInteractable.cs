using UnityEngine;

public class BannerInteractable : BaseInteractable
{
    [Header("Настройки баннера")]
    [SerializeField] private string bannerText = "Информация";
    [SerializeField] private float bannerDuration = 3f;

    protected override void ExecuteInteractionLogic()
    {
        if (BannerManager.Instance == null)
        {
            Debug.LogError($"[BannerInteractable] ОШИБКА: BannerManager.Instance равен null!");
            EndInteraction();
            return;
        }

        BannerManager.Instance.OnBannerHidden += OnBannerEnded;
        BannerManager.Instance.ShowBanner(bannerText, bannerDuration);
    }

    private void OnBannerEnded()
    {
        if (BannerManager.Instance != null)
        {
            BannerManager.Instance.OnBannerHidden -= OnBannerEnded;
        }

        EndInteraction();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        if (BannerManager.Instance != null)
        {
            BannerManager.Instance.OnBannerHidden -= OnBannerEnded;
        }
    }
}
