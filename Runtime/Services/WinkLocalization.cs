using System;
using UnityEngine;
using Lean.Localization;
using UnityEngine.Scripting;

namespace Agava.Wink
{
    [Preserve]
    public class WinkLocalization : MonoBehaviour
    {
        private const string SavedGameSystemLang = nameof(SavedGameSystemLang);

        [SerializeField] private LeanLocalization _leanLocalization;

        public static WinkLocalization Instance;

        private void Awake()
        {
            DontDestroyOnLoad(this);

            Instance = this;
            SystemLanguage language;

            if (UnityEngine.PlayerPrefs.HasKey(SavedGameSystemLang))
            {
                Enum.TryParse(UnityEngine.PlayerPrefs.GetString(SavedGameSystemLang), out SystemLanguage parsedLang);
                language = parsedLang;
            }
            else
            {
                language = Application.systemLanguage;
            }

            ChangeLang(language);
        }

        public void SetCurrentLang(SystemLanguage lang)
        {
            ChangeLang(lang);
            UnityEngine.PlayerPrefs.SetString(SavedGameSystemLang, lang.ToString());
        }

        private void ChangeLang(SystemLanguage lang)
        {
            switch (lang)
            {
                case SystemLanguage.Russian:
                    SetLang(SystemLanguage.Russian.ToString());
                    break;
                default:
                    SetLang(SystemLanguage.English.ToString());
                    break;
            }
        }

        private void SetLang(string lang)
        {
            _leanLocalization.DetectLanguage = LeanLocalization.DetectType.None;
            _leanLocalization.CurrentLanguage = lang;
            _leanLocalization.SetCurrentLanguage(lang);
        }
    }
}