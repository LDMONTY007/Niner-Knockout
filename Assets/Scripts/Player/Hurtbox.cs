using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This should be attatched to a collider 
/// set to "isTrigger"
/// 
/// Keep the launchDir values in a -1 to +1 range on both axes
/// </summary>
public class Hurtbox : MonoBehaviour
{
    /// <summary>
    /// The direction the enemy is sent in if this attack lands. Keep normalized.
    /// </summary>
    public Vector2 launchDir = Vector2.zero;

    /// <summary>
    /// The percentage of damage added to the player's damage meter upon a successful hit.
    /// </summary>
    public float attackDamage = 0f;

    //There should probably be a bool that makes it so that either you use the current damage as knockback for the launch
    //or you use just the damage from this attack as the knockback for the launch.  
}
