using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

[ExecuteAlways]
public class CharacterIcon : MonoBehaviour
{

    private Gradient percentGradient = new Gradient();



    private float percent;
    private float visualizedPercent = 0f;

    /// <summary>
    /// The TextMeshProUGUI object that displays
    /// the percent of knockback on this character.
    /// </summary>
    public TextMeshProUGUI percentText;

    private void Start()
    {
        // Blend color from red at 0% to blue at 100%
        var colors = new GradientColorKey[6];
        colors[0] = new GradientColorKey(Color.white, 0.0f); //white at 0%
        colors[1] = new GradientColorKey(new Color(1f, .36f, 0f), 0.05f); //orange at 50%
        colors[2] = new GradientColorKey(Color.red, 0.1f); //red at 100%
        colors[3] = new GradientColorKey(new Color(1f, 0f, 0.5f), 0.2f); //dark red at 200%
        colors[4] = new GradientColorKey(new Color(1f, 0f, 0.2f), 0.3f); //Super dark red at 300%
        colors[5] = new GradientColorKey(new Color(1f, 0f, 0.2f), 1f); //Super dark red at 1000% (clamped as 999.9%)

        // Blend alpha from opaque at 0% to transparent at 100%
        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);


        percentGradient.SetKeys(colors, alphas);

        //StartCoroutine(LerpFunction(100, 5f));
    }

/*    float timeElapsed;
    float lerpDuration = 3;
    float startValue = 0;
    float endValue = 10;
    float valueToLerp;
    void Update()
    {
        if (timeElapsed < lerpDuration)
        {
            valueToLerp = Mathf.Lerp(startValue, endValue, timeElapsed / lerpDuration);
            timeElapsed += Time.deltaTime;
        }
    }*/

    public void SetPercent(float p)
    {
        percent = p;
        if (visualizedPercent == percent)
        {
            return;
        }
        StartCoroutine(LerpFunction(percent, 1f));
        //startIncrementing();
        //StartCoroutine(Lerp());
    }

    public float GetPercent()
    {
        return percent;
    }

    IEnumerator LerpFunction(float endValue, float duration)
    {
        float time = 0;
        float startValue = visualizedPercent;
        while (time < duration)
        {
            visualizedPercent = Mathf.Lerp(startValue, endValue, time / duration);
            percentText.text = visualizedPercent.ToString("F1") + "%";
            percentText.color = percentGradient.Evaluate(visualizedPercent / 1000);
            Debug.Log(visualizedPercent);
            time += Time.deltaTime;
            yield return null;
        }
        visualizedPercent = endValue;
        percentText.text = visualizedPercent.ToString("F1") + "%";
        percentText.color = percentGradient.Evaluate(visualizedPercent / 1000);
    }
}
