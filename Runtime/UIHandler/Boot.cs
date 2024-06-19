using System;
using System.Collections;
using UnityEngine;
using SmsAuthAPI.Program;
using System.Threading.Tasks;

namespace Agava.Wink
{
    /// <summary>
    ///     Starting auth services and cloud saves.
    /// </summary>
    [DefaultExecutionOrder(-123)]
    public class Boot : MonoBehaviour, IBoot
    {
        private const string FirsttimeStartApp = nameof(FirsttimeStartApp);
        private const float TimeOutTime = 60f;

        [SerializeField] private WinkAccessManager _winkAccessManager;
        [SerializeField] private WinkSignInHandlerUI _winkSignInHandlerUI;
        [SerializeField] private StartLogoPresenter _startLogoPresenter;
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private LoadingProgressBar _loadingProgressBar;
        [SerializeField] private bool _restartAfterAuth = true;

        public static Boot Instance { get; private set; }

        public event Action Restarted;

        private void OnDestroy() => _winkSignInHandlerUI.Dispose();

        private async void Start()
        {
            DontDestroyOnLoad(this);

            if (_winkSignInHandlerUI == null || _winkAccessManager == null)
                throw new NullReferenceException("Boot: Some Auth Component is Missing On Boot!");

            if (Instance == null)
                Instance = this;

            SmsAuthApi.DownloadCloudSavesProgress += OnDownloadCloudSavesProgress;

            _startLogoPresenter.Construct();
            _startLogoPresenter.ShowLogo();

            _winkAccessManager.Construct();
            _winkSignInHandlerUI.Construct(_winkAccessManager);

            await _winkSignInHandlerUI.Initialize();
            await Task.Delay((int)_startLogoPresenter.LogoDuration * 1000);

            StartCoroutine(_startLogoPresenter.HidingLogo());

            OnStarted();

            _startLogoPresenter.CloseBootView();
            var loadingScene = _sceneLoader.LoadGameScene();
            SmsAuthApi.DownloadCloudSavesProgress -= OnDownloadCloudSavesProgress;
            _loadingProgressBar.Enable();

            while (Application.internetReachability == NetworkReachability.NotReachable)
                await Task.Yield();

            _loadingProgressBar.SetProgress(loadingScene.progress, 0.5f, 1.0f);

            while (loadingScene.isDone == false)
                await Task.Yield();

            _loadingProgressBar.Disable();
        }

        private void OnDownloadCloudSavesProgress(float progress)
        {
            _loadingProgressBar.SetProgress(progress, 0.0f, 0.5f);
        }

        private async void OnStarted()
        {
            while (SmsAuthApi.Initialized == false)
                await Task.Yield();

            if (UnityEngine.PlayerPrefs.HasKey(FirsttimeStartApp) == false)
            {
                _winkSignInHandlerUI.OpenSignWindow();
                UnityEngine.PlayerPrefs.SetString(FirsttimeStartApp, "true");
                AnalyticsWinkService.SendSubscribeOfferWindow();

                while ((WinkAccessManager.Instance.HasAccess == true || _winkSignInHandlerUI.IsAnyWindowEnabled == false))
                    await Task.Yield();

                if (WinkAccessManager.Instance.HasAccess)
                {
                    await CloudSavesLoading();
#if UNITY_EDITOR || TEST
                    Debug.Log($"Boot: App First Started. SignIn successfully");
#endif
                }
                else
                {
                    OnSkiped();
                }
            }
            else
            {
                if (UnityEngine.PlayerPrefs.HasKey(SmsAuthAPI.DTO.TokenLifeHelper.Tokens))
                {
                    while (WinkAccessManager.Instance.HasAccess == false)
                        await Task.Yield();

                    if (WinkAccessManager.Instance.HasAccess)
                        await CloudSavesLoading();
                }
                else
                {
                    OnSkiped();
                }
#if UNITY_EDITOR || TEST
                Debug.Log($"Boot: App Started. SignIn: {WinkAccessManager.Instance.HasAccess}");
#endif
            }
        }

        private void OnSuccessfully()
        {
            _winkAccessManager.Successfully -= OnSuccessfully;

#if UNITY_EDITOR || TEST
            Debug.Log($"Boot: Access Successfully");
#endif
            StartCoroutine(Loading());
            IEnumerator Loading()
            {
                yield return CloudSavesLoading();
                Restarted?.Invoke();

                if (_restartAfterAuth)
                    _sceneLoader.LoadGameScene();
            }
        }

        private async Task<bool> CloudSavesLoading()
        {
#if UNITY_EDITOR || TEST
            Debug.Log($"Boot: Try load cloud saves");
#endif
            Coroutine cancelation = null;
            cancelation = StartCoroutine(TimeOutWaiting());

            var result = await SmsAuthAPI.Utility.PlayerPrefs.Load();

            if (result)
            {
                if (cancelation != null)
                    StopCoroutine(cancelation);

                return true;
            }
            else
            {
                return false;
            }
        }

        private IEnumerator TimeOutWaiting()
        {
            yield return new WaitForSecondsRealtime(TimeOutTime);
            _winkSignInHandlerUI.CloseAllWindows();
            _winkSignInHandlerUI.OpenWindow(WindowType.Fail);
#if UNITY_EDITOR || TEST
            Debug.Log($"Boot: Time Out!");
#endif
        }

        private void OnSkiped()
        {
            _winkAccessManager.Successfully += OnSuccessfully;
#if UNITY_EDITOR || TEST
            Debug.Log($"Boot: SignIn skiped");
#endif
        }
    }
}