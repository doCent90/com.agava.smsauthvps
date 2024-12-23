﻿using System;
using System.Collections;
using UnityEngine;
using SmsAuthAPI.Program;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    /// <summary>
    ///     Starting auth services and cloud saves.
    /// </summary>
    [DefaultExecutionOrder(-123), Preserve]
    public class Boot : MonoBehaviour, IBoot
    {
        private const float TimeOutTime = 60f;

        [SerializeField] private int _bundlIdVersion = 1;
        [SerializeField] private WinkAccessManager _winkAccessManager;
        [SerializeField] private WinkSignInHandlerUI _winkSignInHandlerUI;
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private LoadingProgressBar _loadingProgressBar;
        [SerializeField] private bool _restartAfterAuth = true;

        private Coroutine _signInProcess;
        private PreloadService _preloadService;
        private bool _bootStarted = false;

        public static Boot Instance { get; private set; }

        public event Action Restarted;

        private void OnDestroy()
        {
            _winkSignInHandlerUI?.Dispose();
        }

        private IEnumerator Start()
        {
            Debug.Log("#Boot# : Start plugin initialize");
            Debug.Log("#Boot# name device " + SystemInfo.deviceName);

            DontDestroyOnLoad(this);

            if (_winkSignInHandlerUI == null || _winkAccessManager == null)
                throw new NullReferenceException("#Boot# : Some Auth Component is Missing On Boot!");

            if (Instance == null)
                Instance = this;

            _preloadService = new(_winkSignInHandlerUI, _bundlIdVersion);
            _winkAccessManager.Initialize();
            _winkAccessManager.AuthorizationSuccessfully += OnSuccessfully;
            yield return _preloadService.Preparing();

            if (_preloadService.IsPluginAwailable)
            {
                yield return _winkSignInHandlerUI.Initialize();
                SmsAuthApi.DownloadCloudSavesProgress += OnDownloadCloudSavesProgress;

                yield return _winkAccessManager.Construct();
                _winkSignInHandlerUI.StartSevice(_winkAccessManager);
                _winkSignInHandlerUI.Construct();
                yield return _winkAccessManager.TryQuickAccess();

                _signInProcess = StartCoroutine(OnStarted());
                yield return _signInProcess;

                _bootStarted = true;

                var loadingScene = _sceneLoader.LoadGameScene();
                SmsAuthApi.DownloadCloudSavesProgress -= OnDownloadCloudSavesProgress;
                _loadingProgressBar.Enable();

                yield return new WaitUntil(() => { _loadingProgressBar.SetProgress(loadingScene.progress, 0.5f, 1.0f); return loadingScene.isDone; });

                _loadingProgressBar.Disable();
                AnalyticsWinkService.SendStartApp(appId: Application.identifier);
            }
            else
            {
                yield return _winkSignInHandlerUI.Initialize();
                _loadingProgressBar.Disable();
                _sceneLoader.LoadGameScene();
            }
        }

        private void OnDownloadCloudSavesProgress(float progress)
            => _loadingProgressBar.SetProgress(progress, 0.0f, 0.5f);

        private IEnumerator OnStarted()
        {
            yield return new WaitWhile(() => SmsAuthApi.Initialized == false);

            if (WinkAccessManager.Instance.HasAccess == false && WinkAccessManager.Instance.Authenficated == false)
                _winkSignInHandlerUI.OpenStartWindow();

            yield return new WaitUntil(() => (WinkAccessManager.Instance.HasAccess == true || _winkSignInHandlerUI.IsAnyWindowEnabled == false));

            if (UnityEngine.PlayerPrefs.HasKey(SmsAuthAPI.DTO.TokenLifeHelper.Tokens))
            {
                yield return new WaitUntil(() => WinkAccessManager.Instance.Authenficated == true);

                if (WinkAccessManager.Instance.HasAccess)
                    yield return CloudSavesLoading();
            }

#if UNITY_EDITOR || TEST
            Debug.Log($"Boot: App Started. Authenficated: {WinkAccessManager.Instance.Authenficated}");
            Debug.Log($"Boot: App Started. Authorized: {WinkAccessManager.Instance.HasAccess}");
#endif
            if (AdsAppView.Program.Boot.Instance != null)
                yield return AdsAppView.Program.Boot.Instance.Construct(WinkAccessManager.Instance.HasAccess);

            yield return new WaitUntil(() => _winkSignInHandlerUI.IsAnyWindowEnabled == false);

            _signInProcess = null;
        }

        private void OnSuccessfully()
        {
            if (_bootStarted == false)
                return;

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

        private IEnumerator CloudSavesLoading()
        {
#if UNITY_EDITOR || TEST
            Debug.Log($"Boot: Try load cloud saves");
#endif

            Coroutine cancelation = null;
            cancelation = StartCoroutine(TimeOutWaiting());

            var task = SmsAuthAPI.Utility.PlayerPrefs.Load();
            yield return new WaitUntil(() => task.IsCompleted);

            if (cancelation != null)
                StopCoroutine(cancelation);
        }

        private IEnumerator TimeOutWaiting()
        {
            yield return new WaitForSecondsRealtime(TimeOutTime);

            StopCoroutine(_signInProcess);
            _winkSignInHandlerUI.CloseAllWindows();
            _winkSignInHandlerUI.OpenWindow(WindowType.Fail);

#if UNITY_EDITOR || TEST
            Debug.Log($"Boot: Time Out!");
#endif
        }
    }
}
