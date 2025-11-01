using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoaderCallback : MonoBehaviour
{
    private bool isFirstUpdate = true;
    [SerializeField] Image loadingProgress;
    private void Update()
    {
        if (isFirstUpdate)
        {
            isFirstUpdate = false;
            Loader.LoaderCallback();
        }
        loadingProgress.fillAmount = Loader.GetLoadingProgress();
    }
}