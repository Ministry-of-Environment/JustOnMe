using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private TMP_Text SelectDifficultyText;

    private void Start()
    {
        UpdataSelectDifficulty();
    }

    public void UpdataSelectDifficulty()
    {
        if (GameSessionManager.Instance != null)
        {
            SelectDifficultyText.text = $"Difficulty: {GameSessionManager.Instance.SelectedDifficulty}";
        }
    }

    public void OnClickStart()
    {
        SceneManager.LoadScene("House");
    }

    public void OnClickQuit()
    {
        // 에디터에서는 작동 안함, 빌드에서만 작동!
        Application.Quit();
    }
}