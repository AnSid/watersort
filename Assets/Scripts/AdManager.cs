using System;
using System.Collections;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

    // Временно: демо-ID для теста. После проверки вернуть реальные:
    //private const string BANNER_ID = "demo-banner-yandex";
    //private const string INTERSTITIAL_ID = "demo-interstitial-yandex";
    private const string BANNER_ID = "R-M-20129527-1";
    private const string INTERSTITIAL_ID = "R-M-20129527-2";

    private const float INTERSTITIAL_COOLDOWN = 60f;

    // ---- Баннер ----
    private Banner _banner;
    private bool _bannerLoaded = false;
    private bool _bannerVisible = false;

    /// <summary>
    /// Событие видимости баннера. true — баннер виден на экране,
    /// false — скрыт / не загружен / реклама выключена.
    /// </summary>
    public event Action<bool> OnBannerVisibilityChanged;

    public bool IsBannerVisible => _bannerVisible;

    // ---- Межстраничная ----
    private InterstitialAdLoader _interstitialLoader;
    private Interstitial _interstitial;
    private bool _interstitialReady = false;
    private float _lastInterstitialShowTime = -999f;

    private bool _initialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() { InitializeAds(); }

    void OnDestroy()
    {
        DestroyBanner();
        DestroyInterstitial();
        if (Instance == this) Instance = null;
    }

    void InitializeAds()
    {
        if (_initialized) return;
        _initialized = true;

        if (!Application.isMobilePlatform)
        {
            Debug.Log("[AdManager] Не мобильная платформа — реклама не загружается.");
            return;
        }

        if (!GameSettings.AdsEnabled)
        {
            Debug.Log("[AdManager] Реклама выключена в настройках.");
            return;
        }

        _interstitialLoader = new InterstitialAdLoader();
        PreloadInterstitial();
        CreateBanner();
    }

    // ============================================================
    // БАННЕР
    // ============================================================

    void CreateBanner()
    {
        if (_banner != null) return;

        int screenWidthDp = ScreenUtils.ConvertPixelsToDp((int)Screen.safeArea.width);
        BannerAdSize adSize = BannerAdSize.Sticky(screenWidthDp);

        _banner = new Banner(adSize, AdPosition.BottomCenter);
        _banner.OnAdLoaded += HandleBannerLoaded;
        _banner.OnAdFailedToLoad += HandleBannerFailedToLoad;

        AdRequest request = new AdRequest(BANNER_ID);
        _banner.LoadAd(request);

        Debug.Log("[AdManager] Баннер создан, загрузка начата.");
    }

    void HandleBannerLoaded(object sender, EventArgs args)
    {
        _bannerLoaded = true;
        Debug.Log("[AdManager] Баннер загружен.");
        SetBannerVisible(true);
    }

    void HandleBannerFailedToLoad(object sender, AdFailureEventArgs args)
    {
        _bannerLoaded = false;
        Debug.LogWarning($"[AdManager] Баннер не загружен: {args.Message}");
        SetBannerVisible(false);
    }

    public void ShowBanner()
    {
        if (_banner == null)
        {
            if (GameSettings.AdsEnabled && Application.isMobilePlatform)
                CreateBanner();
            return;
        }

        if (_bannerLoaded)
        {
            _banner.Show();
            SetBannerVisible(true);
        }
    }

    public void HideBanner()
    {
        if (_banner != null)
            _banner.Hide();

        SetBannerVisible(false);
    }

    void DestroyBanner()
    {
        if (_banner != null)
        {
            _banner.Destroy();
            _banner = null;
        }
        _bannerLoaded = false;
        SetBannerVisible(false);
    }

    void SetBannerVisible(bool visible)
    {
        if (_bannerVisible == visible) return;
        _bannerVisible = visible;
        OnBannerVisibilityChanged?.Invoke(visible);
    }

    // ============================================================
    // МЕЖСТРАНИЧНАЯ
    // ============================================================

    async void PreloadInterstitial()
    {
        if (_interstitialLoader == null) return;
        if (_interstitialReady) return;

        try
        {
            _interstitial = await _interstitialLoader.LoadAd(new AdRequest(INTERSTITIAL_ID));
            _interstitialReady = true;

            if (_interstitial != null)
            {
                _interstitial.OnAdDismissed += HandleInterstitialDismissed;
                _interstitial.OnAdFailedToShow += HandleInterstitialFailedToShow;
            }

            Debug.Log("[AdManager] Межстраничная загружена.");
        }
        catch (AdLoadingException e)
        {
            _interstitialReady = false;
            Debug.LogWarning($"[AdManager] Межстраничная не загружена: {e.Message}");
        }
    }

    void HandleInterstitialDismissed(object sender, EventArgs args)
    {
        _interstitialReady = false;
        DestroyInterstitial();
        PreloadInterstitial();
    }

    void HandleInterstitialFailedToShow(object sender, AdFailureEventArgs args)
    {
        _interstitialReady = false;
        Debug.LogWarning($"[AdManager] Межстраничная не показана: {args.Message}");
    }

    public bool TryShowInterstitial()
    {
        if (!Application.isMobilePlatform) return false;
        if (!GameSettings.AdsEnabled) return false;
        if (!_interstitialReady || _interstitial == null) return false;

        if (Time.unscaledTime - _lastInterstitialShowTime < INTERSTITIAL_COOLDOWN)
        {
            Debug.Log("[AdManager] Cooldown межстраничной не прошёл.");
            return false;
        }

        _lastInterstitialShowTime = Time.unscaledTime;
        _interstitial.Show();
        Debug.Log("[AdManager] Межстраничная показана.");
        return true;
    }

    void DestroyInterstitial()
    {
        if (_interstitial != null)
        {
            _interstitial.Destroy();
            _interstitial = null;
        }
        _interstitialReady = false;
    }

    // ============================================================
    // РЕАКЦИЯ НА ТУМБЛЕР
    // ============================================================

    public void OnAdsEnabledChanged()
    {
        if (GameSettings.AdsEnabled)
        {
            if (_interstitialLoader == null)
                _interstitialLoader = new InterstitialAdLoader();

            // Сброс cooldown: после включения рекламы следующая
            // межстраничная покажется не раньше чем через полный cooldown.
            _lastInterstitialShowTime = -999f;

            // Пересоздаём баннер с нуля — после Hide SDK может не вернуть вьюху.
            DestroyBanner();
            CreateBanner();

            PreloadInterstitial();
        }
        else
        {
            // Полное уничтожение, а не Hide — чтобы SDK не оставил висеть нативную вьюху.
            DestroyBanner();
        }
    }
}