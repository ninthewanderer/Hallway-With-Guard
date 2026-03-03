using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseMenu : MonoBehaviour
{
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
    
    public void PlayAgain()
    {
        SceneManager.LoadScene("LevelBlockout_Test");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Application.Quit();
    }
}
