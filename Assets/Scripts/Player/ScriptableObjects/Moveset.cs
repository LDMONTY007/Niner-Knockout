using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Moveset attack data for your character.
/// Name this file "CharacterName_Moveset"
/// when you create it
/// </summary>
[CreateAssetMenu(fileName = "Moveset", menuName = "Character/Moveset", order = 1)]
public class Moveset : ScriptableObject
{
    //TODO:
    //Replace each attack with an array of parameters
    //So that the user can specify which frames
    //Of the attack it each value applies to. 
    //That way there are effectively multiple "hitboxes"
    //like in smash.
    //Also figure out how to implement multiple hitboxes for certain attacks.
    //By that I mean multiple in one singular frame.
    

    [Header("Tilt Attacks")]
    public AttackInfo neutral = new AttackInfo();
    public AttackInfo forwardTilt = new AttackInfo();
    public AttackInfo upTilt = new AttackInfo();
    public AttackInfo downTilt = new AttackInfo();
    [Header("Aerials")]
    public AttackInfo neutralAerial = new AttackInfo();
    public AttackInfo forwardAerial = new AttackInfo();
    public AttackInfo backAerial = new AttackInfo();
    public AttackInfo upAerial = new AttackInfo();
    public AttackInfo downAerial = new AttackInfo();
    [Header("Specials")]
    public AttackInfo neutralSpecial = new AttackInfo();
    public AttackInfo forwardSpecial = new AttackInfo();
    public AttackInfo upSpecial = new AttackInfo();
    public AttackInfo downSpecial = new AttackInfo();
    [Header("Smash Attacks")]
    public AttackInfo forwardSmash = new AttackInfo();
    public AttackInfo upSmash = new AttackInfo();
    public AttackInfo downSmash = new AttackInfo();

    [Header("Misc Attacks")]
    public AttackInfo dashAttack = new AttackInfo();
}
