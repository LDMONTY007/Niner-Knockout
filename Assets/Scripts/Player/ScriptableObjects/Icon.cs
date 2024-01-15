using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Icon", menuName = "Character/Icon", order = 2)]
public class Icon : ScriptableObject
{
    public Sprite characterSprite;
    public string characterName = "Character Name";
    public AudioClip characterAnnouncement;
    public Color backgroundColor = Color.black;

    public float zoom = 1f;

    public GameObject characterPrefab;
}
