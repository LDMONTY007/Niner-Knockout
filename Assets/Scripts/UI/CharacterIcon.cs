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

    /// <summary>
    /// The TextMeshProUGUI object that displays
    /// the percent of damage on this character.
    /// </summary>
    public TextMeshProUGUI percentText;

    private void Start()
    {
        //setup color blending for character icon percentage.
        var colors = new GradientColorKey[6];
        colors[0] = new GradientColorKey(Color.white, 0.0f); //white at 0%
        colors[1] = new GradientColorKey(new Color(1f, .36f, 0f), 0.05f); //orange at 50%
        colors[2] = new GradientColorKey(Color.red, 0.1f); //red at 100%
        colors[3] = new GradientColorKey(new Color(0.5f, 0f, 0f), 0.2f); //dark red at 200%
        colors[4] = new GradientColorKey(new Color(0.2f, 0f, 0f), 0.3f); //Super dark red at 300%
        colors[5] = new GradientColorKey(new Color(0.2f, 0f, 0f), 1f); //Super dark red at 1000% (clamped as 999.9%)

        //always be 1 in the alpha for the gradient.
        var alphas = new GradientAlphaKey[2];
        alphas[0] = new GradientAlphaKey(1.0f, 0.0f);
        alphas[1] = new GradientAlphaKey(1.0f, 1.0f);


        percentGradient.SetKeys(colors, alphas);
    }

    public void SetPercent(float p)
    {
        percent = p;
        percentText.text = percent.ToString("F1") + "%";
        percentText.color = percentGradient.Evaluate(percent / 1000);
    }

    public float GetPercent()
    {
        return percent;
    }
}
