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
    [Header("Tilt Attacks")]
    public AttackInfo Neutral = new AttackInfo();
    public AttackInfo ForwardTilt = new AttackInfo();
    public AttackInfo UpTilt = new AttackInfo();
    public AttackInfo DownTilt = new AttackInfo();
    [Header("Aerials")]
    public AttackInfo NeutralAerial = new AttackInfo();
    public AttackInfo ForwardAerial = new AttackInfo();
    public AttackInfo BackAerial = new AttackInfo();
    public AttackInfo UpAerial = new AttackInfo();
    public AttackInfo DownAerial = new AttackInfo();
    [Header("Specials")]
    public AttackInfo NeutralSpecial = new AttackInfo();
    public AttackInfo ForwardSpecial = new AttackInfo();
    public AttackInfo UpSpecial = new AttackInfo();
    public AttackInfo DownSpecial = new AttackInfo();
    [Header("Smash Attacks")]
    public AttackInfo ForwardSmash = new AttackInfo();
    public AttackInfo UpSmash = new AttackInfo();
    public AttackInfo DownSmash = new AttackInfo();
}
