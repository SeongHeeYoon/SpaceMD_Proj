using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField]
    private GameObject gameOverPanel;

    private void Awake()
    {
        Instance = this;
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);

        Time.timeScale = 0;
    }

    public void Restart()
    {
        Time.timeScale = 1;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
