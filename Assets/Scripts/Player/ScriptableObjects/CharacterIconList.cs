using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterIconList", menuName = "Character/Icon List", order = 3)]
public class CharacterIconList : ScriptableObject
{
    public List<Icon> icons = new List<Icon>();
}
