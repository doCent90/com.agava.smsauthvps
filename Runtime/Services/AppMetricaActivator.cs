using Io.AppMetrica;
using UnityEngine;

public static class AppMetricaActivator
{
    private const string FirtsAppMetricaLaunch = nameof(FirtsAppMetricaLaunch);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Activate()
    {
        string key = "e8883556-3e2e-4706-a0a5-a7eb677fe077";

        AppMetrica.Activate(new AppMetricaConfig(key)
        {
            FirstActivationAsUpdate = !IsFirstLaunch(),
        });
#if UNITY_EDITOR || TEST
        Debug.Log($"AppMetrica start: {key}");
#endif
    }

    private static bool IsFirstLaunch()
    {
        if (PlayerPrefs.HasKey(FirtsAppMetricaLaunch))
        {
            return false;
        }
        else
        {
            PlayerPrefs.SetString(FirtsAppMetricaLaunch, "true");
            return true;
        }
    }
}

