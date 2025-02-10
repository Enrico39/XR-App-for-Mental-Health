using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    // Metodo per caricare una scena specifica per nome
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Metodo per ricaricare la scena attuale
    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Metodo per uscire dal gioco
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game is exiting...");
    }
}
