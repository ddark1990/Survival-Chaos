using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Doozy.Engine.UI;

namespace SurvivalChaos
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] UIView loadingScreenView;

        private void Awake()
        {
            StartCoroutine(UnloadScene());
        }

        private IEnumerator UnloadScene()
        {
            yield return new WaitForSeconds(5);

            loadingScreenView.Hide();

            yield return new WaitUntil(() => loadingScreenView.IsHiding);

            SceneManager.UnloadSceneAsync("Loading");
        }
    }
}
