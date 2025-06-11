using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public EventReference bumpEvent;
    public EventReference peopleEvent;
    public EventReference musicEvent;
    public EventReference changeMusicEvent;
    public EventReference menuEvent;

    private EventInstance discoMusic;
    private EventInstance bumpInstance;

    private Bank masterBank;

    public bool playMusic;

    public Slider masterSlider;
    public Slider musicSlider;
    public Slider fxSlider;
    public Slider ambienceSlider;
    public Slider paddingSlider;

        // Start is called before the first frame update
    void Start()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        SetSliders();

        RuntimeManager.LoadBank("Master", true);
        RuntimeManager.LoadBank("Music", true);

        bool bankLoaded = RuntimeManager.HasBankLoaded("Music");
        Debug.Log("FX loaded: " + bankLoaded);

        bumpInstance = RuntimeManager.CreateInstance(bumpEvent);
        discoMusic = RuntimeManager.CreateInstance(musicEvent);

        if (!playMusic)
        {
            return;
        }

        discoMusic.setParameterByName("Music", 1);

        discoMusic.start();
    }

    private void SetSliders()
    {
        masterSlider.onValueChanged.AddListener(ChangeMasterValue);
        musicSlider.onValueChanged.AddListener(ChangeMusicValue);
        fxSlider.onValueChanged.AddListener(ChangeFxValue);
        ambienceSlider.onValueChanged.AddListener(ChangeAmbienceValue);
        paddingSlider.onValueChanged.AddListener(ChangePaddingValue);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ChangeMusic();
        }
    }

    public void ChangeMusic()
    {
        RuntimeManager.PlayOneShot(changeMusicEvent);

        discoMusic.setParameterByName("Music", 0);

        discoMusic.setParameterByName("Music", 1);
    }

    public void PlayBump(float collided)
    {
        switch(collided)
        {
            case 0:
                {
                    bumpInstance.setParameterByName("ThingBumped", 0);
                } break;
            case 1:
                {
                    bumpInstance.setParameterByName("ThingBumped", 1);
                }
                break;
            case 2:
                {
                    bumpInstance.setParameterByName("ThingBumped", 2);
                }
                break;
        }
        bumpInstance.start();
    }

    public void ChangeMasterValue(float value)
    {
        masterSlider.value = value;
        RuntimeManager.StudioSystem.setParameterByName("MasterVolume", value * 100);
    }
    public void ChangeMusicValue(float value)
    {
        musicSlider.value = value;
        RuntimeManager.StudioSystem.setParameterByName("MusicVolume", value * 100);
    }
    public void ChangeFxValue(float value)
    {
        fxSlider.value = value;
        RuntimeManager.StudioSystem.setParameterByName("FxVolume", value * 100);
    }
    public void ChangeAmbienceValue(float value)
    {
        ambienceSlider.value = value;
        RuntimeManager.StudioSystem.setParameterByName("AmbienceVolume", value * 100);
    }
    public void ChangePaddingValue(float value)
    {
        paddingSlider.value = value;
        RuntimeManager.StudioSystem.setParameterByName("Padding", value);
    }

}
