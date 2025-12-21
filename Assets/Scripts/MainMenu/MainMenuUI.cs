using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class mainmenuUI : MonoBehaviour
{
    [SerializeField] Button PlayButton;
    [SerializeField] Button ExitButton;
    private void Awake()
    {
        PlayButton.onClick.AddListener(OnPlayButtonClicked);
        ExitButton.onClick.AddListener(OnPlayButtonClicked);
    }
    public void OnPlayButtonClicked()
    {
        SceneLoader.Load(SceneLoader.Scene.Adventure);
    }
    public void OnExitButtonClicked()
    {
        Application.Quit();
    }
}
