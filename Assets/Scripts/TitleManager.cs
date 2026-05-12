using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public void OnClickStart()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnClickQuit()
    {
        // 에디터에서는 작동 안함, 빌드에서만 작동!
        Application.Quit();
    }
}