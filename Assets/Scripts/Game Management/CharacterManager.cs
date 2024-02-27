using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

public class CharacterManager : MonoBehaviour
{
    CinemachineTargetGroup targetGroup;

    public Transform playerIconPanel;

    public GameObject playerIconPrefab;

    public GameObject playerSelectIconPrefab;

    public GameObject playerCursorPrefab;

    public Canvas selectionCanvas;

    //each index is the player's number.
    //1-8
    //each device corresponds to the specific index and the
    //character they select will be associated with their slot.
    //we add the device into each dictionary when they select
    //their character in the player selection scene.
    Dictionary<int, InputDevice> devices = new Dictionary<int, InputDevice>();

    Dictionary<int, GameObject> characterSelections = new Dictionary<int, GameObject>();

    /// <summary>
    /// This list is used to create the grid of selectable characters in the player select scene.
    /// </summary>
    public CharacterIconList characterIcons;

    public bool useSelectableIcons = false;

    public bool manuallyInitCharacters = false;

    private void Awake()
    {
        InputSystem.onDeviceChange +=
        (device, change) =>
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    // New Device.
                    Debug.Log(device + " was Added!");
                    break;
                case InputDeviceChange.Disconnected:
                    // Device got unplugged.
                    Debug.Log(device + " was Disconnected!");
                    break;
                case InputDeviceChange.Reconnected:
                    // Plugged back in.
                    Debug.Log(device + " was Reconnected!");
                    break;
                case InputDeviceChange.Removed:
                    // Remove from Input System entirely; by default, Devices stay in the system once discovered.
                    Debug.Log(device + " was Removed!");
                    break;
                default:
                    // See InputDeviceChange reference for other event types.
                    break;
            }
        };
        GameManager.instance.characterManager = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        targetGroup = GetComponent<CinemachineTargetGroup>();
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
        //InputSystem.devices
        /*foreach (InputDevice device in InputSystem.devices)
        {
            device.
        }*/
        if (useSelectableIcons)
        foreach (Icon icon in characterIcons.icons)
        {
            AddSelectableIcon(icon);
        }

        
        if (manuallyInitCharacters)
        {
            //get the devices and the characters they selected and instantiate them.
            int index = 0;
            foreach(PlayerInfo playerInfo in GameManager.instance.gameMode.players)
            {               
                SpawnPlayer(playerInfo, index);

                
                index++;
                //THIS IS WHAT LINKS THE CONTROLLER TO THE SPECIFICALLY SPAWNED PLAYER. 
            }
        }
        //if we are on the character select screen and players have already been 
        //added to the player list we need to spawn the cursor with it's 
        //corresponding device attatched.
        else if (!manuallyInitCharacters && GameManager.instance.players.Count > 0)
        {
            //This is still a horrible way to handle the management of players.
            //it should really be changed to be a dictionary instead of a list
            //so that we can give it the device's name or something and then
            //use that to check if they already have someone selected.
            int index = 0;
            foreach (PlayerInfo pi in GameManager.instance.players)
            {
                //instantiate the player input object using the player cursor prefab.
                PlayerInput p = PlayerInput.Instantiate(playerCursorPrefab, -1, pi.controlScheme, -1, pi.device);
                //tell the cursor we already selected something.
                p.GetComponent<Cursor>().didSelect = true;
                p.GetComponent<Cursor>().characterIndex = index;
                index++;
            }
        }
    }

    private void OnPlayerJoined(PlayerInput obj)
    {
        //weight is how much they affect the camera,
        //radius is the radius around the transform
        //required to be kept in the camera view.
        targetGroup.AddMember(obj.transform, 1f, 1f);
        //if this is a cursor parent it to the canvas.
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Adds a player's icon to the UI.
    /// </summary>
    /// <param name="prefab">The prefab of the player icon</param>
    public CharacterIcon AddPlayerIcon(Icon icon)
    {
        if (playerIconPrefab != null)
        {
            CharacterIcon obj = Instantiate(playerIconPrefab, playerIconPanel).GetComponent<CharacterIcon>();
            obj.characterIcon = icon;
            return obj;
        }
        return null;
    }

    /// <summary>
    /// Adds a character icon to the selectable grid.
    /// </summary>
    /// <param name="prefab">The gameobject icon to be added to the selectable grid UI</param>
    public void AddSelectableIcon(Icon icon)
    {
        if (playerSelectIconPrefab != null)
        {
            CharacterSelectIcon obj = Instantiate(playerSelectIconPrefab, playerIconPanel).GetComponent<CharacterSelectIcon>();
            obj.characterIcon = icon;
        }
    }

    /// <summary>
    /// Spawn a player from the GameManager's player list.
    /// </summary>
    /// <param name="characterIndex">The index of the PlayerInfo in the GameManager PlayerInfo list</param>
    public void SpawnPlayer(int characterIndex)
    {
        //We should not do any checking of win conditions in the characterManager, 
        //all we should do in here is check if a character has no stocks left then
        //don't spawn them.
        if (GameManager.instance.players[characterIndex].stock == 0)
        {
            return;
        }

        PlayerInfo playerInfo = GameManager.instance.gameMode.players[characterIndex];
        //instantiate the prefab, auto assign the playerindex, use X control scheme, auto assign the split screen index, and use X device.
        PlayerInput p = PlayerInput.Instantiate(playerInfo.characterIcon.characterPrefab, -1, playerInfo.controlScheme, -1, playerInfo.device);
        p.GetComponent<Player>().characterIndex = characterIndex;
        //Set the stock of the player.
        p.GetComponent<Player>().stock = playerInfo.stock;
        p.GetComponent<Player>().characterIcon = AddPlayerIcon(playerInfo.characterIcon);
    }

    /// <summary>
    /// Spawn a player given the character's player info and their index.
    /// </summary>
    /// <param name="playerInfo"></param>
    public void SpawnPlayer(PlayerInfo playerInfo, int characterIndex)
    {

        //We should not do any checking of win conditions in the characterManager, 
        //all we should do in here is check if a character has no stocks left then
        //don't spawn them.

        //We may not want to do this check here later, but I am doing it
        //to make sure we set the stock value correctly.
        if (GameManager.instance.players[characterIndex].stock == 0)
        {
            return;
        }

        //instantiate the prefab, auto assign the playerindex, use X control scheme, auto assign the split screen index, and use X device.
        PlayerInput p = PlayerInput.Instantiate(playerInfo.characterIcon.characterPrefab, -1, playerInfo.controlScheme, -1, playerInfo.device);
        p.GetComponent<Player>().characterIndex = characterIndex;
        //Set the stock of the player.
        p.GetComponent<Player>().stock = playerInfo.stock;
        p.GetComponent<Player>().characterIcon = AddPlayerIcon(playerInfo.characterIcon);

        Debug.Log(("Spawn Player: " + characterIndex).ToString().Color("Green"));
    }

    public void PlayerDied(int characterIndex)
    {

        //Decrement character stock count
        //We need to copy the PlayerInfo
        //then decrement it
        //then set change the info stored to the modified info.
        if (GameManager.instance.gameMode == null)
        {
            Debug.LogWarning("LD'S STUPID CODE IS DUMB AND CAUSING ISSUES HERE! LD FIX THIS!");
            return;
        }
        PlayerInfo modifiedInfo = GameManager.instance.gameMode.players[characterIndex];
        modifiedInfo.stock--;
        if (modifiedInfo.stock == 0)
        {
            Debug.Log(modifiedInfo.characterIcon.characterName + " is down for the count!");
            //remove them from the character list.
            GameManager.instance.gameMode.players.RemoveAt(characterIndex);
            //exit this method 
            //because the player can no longer respawn.
            return;
        }
        GameManager.instance.gameMode.players[characterIndex] = modifiedInfo;


        //Really bad way to check if there's only one player left.
        //This will be changed later so that it's 
        //handled in the game mode class.
        /*int i = 0;
        foreach (PlayerInfo p in GameManager.instance.gameMode.players)
        {
            if (p.stock == 0)
            {
                i++;
            }
        }
        if (i == GameManager.instance.gameMode.players.Count - 1)
        {
            //The final Player has won.
            //In the future this'll just be a list of the players
            //that are currently alive in the scene and when 
            //the list only has 1 player we'll just say that 
            //player won and that "IT'S A KNOCKOUT!".
            Debug.Log("IT'S A KNOCKOUT!".Color("Green"));
        }*/

        //Wait X seconds and then respawn character.
        StartCoroutine(LDUtil.Wait(SpawnPlayer, characterIndex, 3f));
    }
}
