using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

//No need to redesign the wheel: https://www.youtube.com/watch?v=CNu8PcAmC2c
public class CharacterSelectIcon : MonoBehaviour
{
    public Icon characterIcon;
    public Image backgroundImage;
    public Image characterImage;
    public TextMeshProUGUI characterName;

    Vector2 imageSizeDelta;

    // Start is called before the first frame update
    void Start()
    {
        backgroundImage.color = characterIcon.backgroundColor;
        characterImage.sprite = characterIcon.characterSprite;
        characterName.text = characterIcon.characterName;
        imageSizeDelta = characterImage.rectTransform.sizeDelta;

        characterImage.GetComponent<RectTransform>().pivot = uiPivot(characterImage.sprite);
        characterImage.GetComponent<RectTransform>().sizeDelta = imageSizeDelta;
        characterImage.GetComponent<RectTransform>().sizeDelta *= characterIcon.zoom;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector2 uiPivot(Sprite sprite)
    {
        Vector2 pixelSize = new Vector2(sprite.texture.width, sprite.texture.height);
        Vector2 pixelPivot = sprite.pivot;
        return new Vector2(pixelPivot.x / pixelSize.x, pixelPivot.y / pixelSize.y);
    }
}
