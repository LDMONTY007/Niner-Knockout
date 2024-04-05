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
    

    //Only make this value editable
    //in the inspector.
    [SerializeField]
    private int totalStocks = 3;

    //The input Device and character prefab the player selected. 
    //Cleared after each match.
    //assigned during the selection screen.
    public List<PlayerInfo> players;

    public List<AudioClip> respawnSounds = new List<AudioClip>();

    private void Awake()
    {
        //make a copy of the original list of players for use
        //during this game mode.
        players = new List<PlayerInfo>(GameManager.instance.players);

        //Set our reference in the GameManager
        GameManager.instance.gameMode = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        //Super inefficient way to check if there's only 1 player left.
        int deadCount = 0;
        foreach (PlayerInfo player in players)
        {
            if (player.stock == 0)
            {
                deadCount++;
            }
        }
        Debug.LogWarning(deadCount);
        if (deadCount == players.Count - 1)
        {
            Debug.Log("IT'S A KNOCKOUT!".Color("Green"));
            GameManager.instance.setScene("CharacterSelectionScene");
        }


/*        if (players.Count == 1)
        {
            Debug.Log("IT'S A KNOCKOUT!".Color("Green"));
            GameManager.instance.setScene("CharacterSelectionScene");
        }*/
    }
}
