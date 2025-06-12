using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoLightManager : MonoBehaviour
{
    public enum LightPattern
    {
        AllFlash,
        AlternateEvenOdd,
        Wave,
        RandomSparkle
    }

    public List<SpriteRenderer> lights = new List<SpriteRenderer>();

    public LightPattern pattern = LightPattern.AllFlash;
    public float stepInterval = 0.2f;
    public float waveDuration = 2f;

    public float patternSwitchInterval = 10f;
    private Coroutine patternCoroutine;

    private Coroutine autoSwitchCoroutine;

    void OnEnable()
    {
        StartPattern();
        StartAutoSwitch();
    }

    void OnDisable()
    {
        StopAutoSwitch();
        StopPattern();
    }

    public void StartPattern()
    {
        StopPattern();
        switch (pattern)
        {
            case LightPattern.AllFlash:
                patternCoroutine = StartCoroutine(AllFlash());
                break;
            case LightPattern.AlternateEvenOdd:
                patternCoroutine = StartCoroutine(AlternateEvenOdd());
                break;
            case LightPattern.Wave:
                patternCoroutine = StartCoroutine(Wave());
                break;
            case LightPattern.RandomSparkle:
                patternCoroutine = StartCoroutine(RandomSparkle());
                break;
        }
    }

    public void StopPattern()
    {
        if (patternCoroutine != null)
            StopCoroutine(patternCoroutine);
        SetAllLights(false);
    }

    public void StartAutoSwitch()
    {
        StopAutoSwitch();
        autoSwitchCoroutine = StartCoroutine(AutoSwitchPattern());
    }
    public void StopAutoSwitch()
    {
        if (autoSwitchCoroutine != null)
            StopCoroutine(autoSwitchCoroutine);
    }
    private IEnumerator AutoSwitchPattern()
    {
        while (true)
        {
            yield return new WaitForSeconds(patternSwitchInterval);
            pattern = (LightPattern)(((int)pattern + 1) % System.Enum.GetValues(typeof(LightPattern)).Length);
            StartPattern();
        }
    }

    private IEnumerator AllFlash()
    {
        bool on = false;
        while (true)
        {
            on = !on;
            SetAllLights(on);
            yield return new WaitForSeconds(stepInterval);
        }
    }

    private IEnumerator AlternateEvenOdd()
    {
        bool showEven = false;
        while (true)
        {
            showEven = !showEven;
            for (int i = 0; i < lights.Count; i++)
                lights[i].enabled = (i % 2 == (showEven ? 0 : 1));
            yield return new WaitForSeconds(stepInterval);
        }
    }

    private IEnumerator Wave()
    {
        int count = lights.Count;
        while (true)
        {
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < count; j++)
                    lights[j].enabled = (j == i);
                yield return new WaitForSeconds(waveDuration / count);
            }
        }
    }

    private IEnumerator RandomSparkle()
    {
        System.Random rnd = new System.Random();
        while (true)
        {
            int idx = rnd.Next(0, lights.Count);
            lights[idx].enabled = !lights[idx].enabled;
            yield return new WaitForSeconds(stepInterval);
        }
    }

    private void SetAllLights(bool on)
    {
        foreach (var lr in lights)
            lr.enabled = on;
    }

#if UNITY_EDITOR
    // cambiar patrón en tiempo real desde el editor
    //muchas veces se bugea pero meh
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            StartAutoSwitch();
            StartPattern();
        }
    }
    #endif
}
