using System.Collections;
using System.Threading.Tasks;
using Agava.Wink;
using SmsAuthAPI.Program;
using TMPro;
using UnityEngine;

public class RemoteConfigText : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    [SerializeField] private string _remoteConfigName;
    [SerializeField] private string _fallbackText;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => SmsAuthApi.Initialized);

        Task<string> task = RemoteConfig.StringRemoteConfig(_remoteConfigName, string.Empty);
        yield return new WaitUntil(() => task.IsCompleted);

        string result = task.Result;
        _text.text = string.IsNullOrEmpty(result) ? _fallbackText : result;
    }
}
