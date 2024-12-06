using System.Collections;
using System.Threading.Tasks;
using Agava.Wink;
using Lean.Localization;
using SmsAuthAPI.Program;
using TMPro;
using UnityEngine;

public class RemoteConfigText : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private string _remoteConfigName;
    [SerializeField, LeanTranslationName] private string _translationName;
    [SerializeField] private string _fallbackText;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => SmsAuthApi.Initialized);

        string translation = LeanLocalization.GetTranslationText(_translationName, _fallbackText);

        Task task = RemoteConfig.StringRemoteConfig(_remoteConfigName, string.Empty);
        yield return new WaitUntil(() => task.IsCompleted);
    }
}
