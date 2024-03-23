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
    //
    public Icon characterIcon;
    public Image backgroundImage;
    public Image characterImage;
    public TextMeshProUGUI characterName;
    //This is a transform for a horizontal layout group
    //for automatically arranging the stock icons.
    public Transform stockTransform;
    //The stock icon prefab to use for the UI
    public GameObject stockIcon;
    //

    public int stockCount = 0;

    Vector2 imageSizeDelta;

    private Gradient percentGradient = new Gradient();

    private float percent;

    /// <summary>
    /// The TextMeshProUGUI object that displays
    /// the percent of damage on this character.
    /// </summary>
    public TextMeshProUGUI percentText;

   

    private void Start()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
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

            backgroundImage.color = characterIcon.backgroundColor;
            characterImage.sprite = characterIcon.characterSprite;
            characterName.text = characterIcon.characterName;
            imageSizeDelta = characterImage.rectTransform.sizeDelta;

            characterImage.GetComponent<RectTransform>().pivot = uiPivot(characterImage.sprite);
            characterImage.GetComponent<RectTransform>().sizeDelta = imageSizeDelta;
            characterImage.GetComponent<RectTransform>().sizeDelta *= characterIcon.zoom;
        }
#else
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

            backgroundImage.color = characterIcon.backgroundColor;
            characterImage.sprite = characterIcon.characterSprite;
            characterName.text = characterIcon.characterName;
            imageSizeDelta = characterImage.rectTransform.sizeDelta;

            characterImage.GetComponent<RectTransform>().pivot = uiPivot(characterImage.sprite);
            characterImage.GetComponent<RectTransform>().sizeDelta = imageSizeDelta;
            characterImage.GetComponent<RectTransform>().sizeDelta *= characterIcon.zoom;
#endif

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

    public Vector2 uiPivot(Sprite sprite)
    {
        Vector2 pixelSize = new Vector2(sprite.texture.width, sprite.texture.height);
        Vector2 pixelPivot = sprite.pivot;
        return new Vector2(pixelPivot.x / pixelSize.x, pixelPivot.y / pixelSize.y);
    }


    private void Update()
    {
        //use preprocessor to change the code depending on if this is the build or not.
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            //Place the stock icons that represent the # of stocks left into the stock panel to be automatically arranged
            if (stockTransform.childCount != stockCount)
            {
                //Destroy the stock icons 
                //These methods aren't the most efficient
                //but honestly it'll work for now.
                if (stockTransform.childCount != 0)
                {
                    for (int i = 0; i < stockTransform.childCount; i++)
                    {
                        Destroy(stockTransform.GetChild(i).gameObject);
                    }
                }
                //Instantiate the stock icons.
                for (int i = stockCount; i > 0; i--)
                {
                    //Instantiate icon as a child of the stockTransform horizontal constrainer.
                    Instantiate(stockIcon, stockTransform);
                }
            }
        }
#else
        //Place the stock icons that represent the # of stocks left into the stock panel to be automatically arranged
            if (stockTransform.childCount != stockCount)
            {
                //Destroy the stock icons 
                //These methods aren't the most efficient
                //but honestly it'll work for now.
                if (stockTransform.childCount != 0)
                {
                    for (int i = 0; i < stockTransform.childCount; i++)
                    {
                        Destroy(stockTransform.GetChild(i).gameObject);
                    }
                }
                //Instantiate the stock icons.
                for (int i = stockCount; i > 0; i--)
                {
                    //Instantiate icon as a child of the stockTransform horizontal constrainer.
                    Instantiate(stockIcon, stockTransform);
                }
            }
#endif
    }
}
