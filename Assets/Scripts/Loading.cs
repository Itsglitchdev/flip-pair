using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(nameof(LoadingScene));
    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator LoadingScene()
    {
        SceneName targetScene = MainMenuHandler.GetTargetScene();

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(targetScene.ToString());
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            if (asyncOperation.progress >= 0.9f)
            {
                asyncOperation.allowSceneActivation = true;
            }

            yield return new WaitForSeconds(2f);
        }
        
    }

}
