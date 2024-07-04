using UnityEngine;

[CreateAssetMenu(fileName = "AppMetricaInfo", menuName = "Create AppMetricaInfo")]
public class AppMetricaInfo : ScriptableObject
{
    [field: SerializeField] public string Key { get; private set; }
}
