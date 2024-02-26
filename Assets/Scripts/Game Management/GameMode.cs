using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

//The intent of the class is as follows:
//Store the # of stocks in this and 
//Store the player index relative
//to the number of stocks they have left
//Also, tell the characterManager
//to respawn the characters from this 
//class instead of from themselves 
//so we can configure their respawn time
//and maybe do some intangibility right 
//when they respawn.

//I made this class but decided I won't implement it yet
//as it'll be easier to let the Player.cs
//handle it's stock count internally relative to the 
//GameManager/CharacterManager.

public class GameMode : MonoBehaviour
{
    int totalStocks = 3;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
