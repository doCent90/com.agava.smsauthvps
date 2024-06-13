using System;
using UnityEngine;

namespace Agava.Wink
{
    /// <summary>
    ///     Load target scene after auth.
    /// </summary>
    internal class SceneLoader : MonoBehaviour
    {
        [SerializeField] private string _startSceneName;

        private void Start()
        {
            DontDestroyOnLoad(this);

            if (string.IsNullOrEmpty(_startSceneName))
                throw new NullReferenceException("Start Name Scene is Empty on Boot!");
        }

        /// <summary>
        ///     Load game scene.
        /// </summary>
        internal AsyncOperation LoadGameScene() => UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_startSceneName);
        internal AsyncOperation LoadScene(string sceneName) => UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
        internal AsyncOperation LoadScene(int sceneBuildIndex) => UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneBuildIndex);
    }
}