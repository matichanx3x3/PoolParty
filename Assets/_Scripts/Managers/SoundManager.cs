using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        RuntimeManager.LoadBank("Master", true);
        RuntimeManager.LoadBank("Music", true);

        bool bankLoaded = RuntimeManager.HasBankLoaded("Music");
        Debug.Log("FX loaded: " + bankLoaded);

        bumpInstance = RuntimeManager.CreateInstance(bumpEvent);
        discoMusic = RuntimeManager.CreateInstance(musicEvent);

        discoMusic.setParameterByName("Music", 1);

        discoMusic.start();
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
        }

        bumpInstance.start();

        bumpInstance.release();
    }
}
