using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [Header("UI")]
    public Animator fadeAnimator;
    public float fadeDuration = 2f;

    [Header("Botones")]
    public Button playButton;
    
    public void OnPlayClicked()
    {
        // Desactivar botones mientras se hace el fade
        playButton.interactable = false;

        // Lanzar animación de fade y cargar escena después
        StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        // Activar animación de fade
        fadeAnimator.SetTrigger("Fade");

        // Esperar a que termine el fade
        yield return new WaitForSeconds(fadeDuration);

        // Cargar escena principal
        SceneManager.LoadScene("LevelTest");
    }
}
