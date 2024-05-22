using System;
using System.Collections;
using UnityEngine;
using SmsAuthAPI.Program;

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
        [SerializeField] private bool _restartAfterAuth = true;

        private Coroutine _signInProcess;

        public static Boot Instance { get; private set; }

        public event Action Restarted;

        private void OnDestroy() => _winkSignInHandlerUI.Dispose();

        private IEnumerator Start()
        {
            DontDestroyOnLoad(this);

            if (_winkSignInHandlerUI == null || _winkAccessManager == null)
                throw new NullReferenceException("Boot: Some Auth Component is Missing On Boot!");

            if (Instance == null)
                Instance = this;

            _startLogoPresenter.Construct();
            _startLogoPresenter.ShowLogo();

            _winkAccessManager.Construct();
            yield return _winkSignInHandlerUI.Construct(_winkAccessManager);

            yield return new WaitForSecondsRealtime(_startLogoPresenter.LogoDuration);
            yield return _startLogoPresenter.HidingLogo();
            yield return new WaitWhile(() => Application.internetReachability == NetworkReachability.NotReachable);

            _winkSignInHandlerUI.CloseAllWindows();
            _signInProcess = StartCoroutine(OnStarted());
            yield return _signInProcess;

            _sceneLoader.LoadGameScene();
            _startLogoPresenter.CloseBootView();
        }

        private IEnumerator OnStarted()
        {
            yield return new WaitWhile(() => SmsAuthApi.Initialized == false);

            if (UnityEngine.PlayerPrefs.HasKey(FirsttimeStartApp) == false)
            {
                _winkSignInHandlerUI.OpenSignWindow();
                UnityEngine.PlayerPrefs.SetString(FirsttimeStartApp, "true");

                yield return new WaitUntil(() => (WinkAccessManager.Instance.HasAccess == true || _winkSignInHandlerUI.IsAnyWindowEnabled == false));

                if (WinkAccessManager.Instance.HasAccess)
                {
                    yield return CloudSavesLoading();
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
                    yield return new WaitUntil(() => WinkAccessManager.Instance.HasAccess == true);
                    yield return CloudSavesLoading();
                }
                else
                {
                    OnSkiped();
                }
#if UNITY_EDITOR || TEST
                Debug.Log($"Boot: App Started. SignIn: {WinkAccessManager.Instance.HasAccess}");
#endif
            }

            _signInProcess = null;
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

        private IEnumerator CloudSavesLoading()
        {
#if UNITY_EDITOR || TEST
            Debug.Log($"Boot: Try load cloud saves");
#endif
            Coroutine cancelation = null;
            cancelation = StartCoroutine(TimeOutWaiting());

            var task = SmsAuthAPI.Utility.PlayerPrefs.Load();
            yield return new WaitWhile(() => task.IsCompleted == false);

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

        private void OnSkiped()
        {
            _winkAccessManager.Successfully += OnSuccessfully;
#if UNITY_EDITOR || TEST
            Debug.Log($"Boot: SignIn skiped");
#endif
        }
    }
}