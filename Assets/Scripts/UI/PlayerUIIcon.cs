using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIIcon : MonoBehaviour
{
    public CharacterSelectIcon currentlySelectedCharacter;

    //public Image backgroundImage;
    public Image characterImage;
    public TextMeshProUGUI characterName;
    public Image backgroundImage;

    public TextMeshProUGUI playerName;

    Vector2 imageSizeDelta;

    //The default sprite to use if we havent't selected any character.
    public Sprite defaultIfUnselected;

    public int characterIndex;

    public Gradient backgroundGradient = new Gradient();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Called when the player of this PlayerUIIcon selects a character in the character menu.
    public void ReassignCharacterIcon(CharacterSelectIcon icon)
    {
        if (icon == null)
        {
            return;
        }

        currentlySelectedCharacter = icon;

        //set the color to be relative to our player/character index so that it's kind of like smash
        backgroundImage.color = backgroundGradient.Evaluate(characterIndex)/*currentlySelectedCharacter.characterIcon.backgroundColor*/;
        //Set our alpha of the background to be decently transparent.
        backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 100);
        characterImage.sprite = currentlySelectedCharacter.characterIcon.characterSprite;
        characterName.text = currentlySelectedCharacter.characterIcon.characterName;
        if (imageSizeDelta == Vector2.zero)
        imageSizeDelta = characterImage.rectTransform.sizeDelta;

        characterImage.GetComponent<RectTransform>().pivot = uiPivot(characterImage.sprite);
        characterImage.GetComponent<RectTransform>().sizeDelta = imageSizeDelta * 1.75f;
        characterImage.GetComponent<RectTransform>().sizeDelta *= currentlySelectedCharacter.characterIcon.zoom;
    }

    public Vector2 uiPivot(Sprite sprite)
    {
        Vector2 pixelSize = new Vector2(sprite.texture.width, sprite.texture.height);
        Vector2 pixelPivot = sprite.pivot;
        return new Vector2(pixelPivot.x / pixelSize.x, pixelPivot.y / pixelSize.y);
    }
}
