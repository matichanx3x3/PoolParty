using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using FMOD.Studio;
using FMODUnity;

public class MainMenu : MonoBehaviour
{

    [Header("UI")]
    public Animator fadeAnimator;
    public float fadeDuration = 2f;

    [Header("Botones")]
    public Button playButton;


    public void OnPlayClicked()
    {
        LoadBanks();
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
    

    void LoadBanks()
    {
    
        RuntimeManager.LoadBank("Master", true);
        RuntimeManager.LoadBank("Music", true);
        //Debug.Log("FX loaded: " + bankLoaded);
        StartCoroutine(VerifyIsBanksFulled());

    }

    IEnumerator VerifyIsBanksFulled()
    {
        while (!RuntimeManager.HaveAllBanksLoaded)
        {
            yield return null;
            // Wait until all banks are loaded
        }
        FMODUnity.RuntimeManager.CoreSystem.mixerSuspend();
        FMODUnity.RuntimeManager.CoreSystem.mixerResume();
        Debug.Log("AllBanksLoaded");
        // Ensure FMOD is fully initialized before playing sounds

    }
}
