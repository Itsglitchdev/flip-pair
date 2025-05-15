using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{

    [SerializeField] private Button startButton;
    private static SceneName targetScene;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startButton.onClick.AddListener(() => LoadScene(SceneName.FlipPair));
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LoadScene(SceneName scene)
    {
        targetScene = scene;
        SceneManager.LoadScene(SceneName.Loading.ToString());
    }
    
    public static SceneName GetTargetScene()
    {
        return targetScene;
    }

}
