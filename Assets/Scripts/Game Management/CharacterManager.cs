using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;

public class CharacterManager : MonoBehaviour
{
    CinemachineTargetGroup targetGroup;

    public Transform characterIconPanel;

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
    public CharacterIconList selectableIconList;

    public List<CharacterIcon> characterIcons = new List<CharacterIcon>();

    public List<PlayerUIIcon> playerUIIconList = new List<PlayerUIIcon>();

    public GameObject playerSelectUIIconPrefab;

    public Transform playerIconPanel;

    public bool useSelectableIcons = false;

    public bool manuallyInitCharacters = false;

    //this is the list of cursors currently being used.
    //If we aren't on the Selection Screen then this should be empty. 
    public List<GameObject> cursors = new List<GameObject>();

    //Used in case for some reason we don't select a character.
    public Icon defaultIcon;

    //The inputActionsAsset containing your control schemes.
    public InputActionAsset inputActions;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private string GetCorrespondingControlScheme(InputDevice device)
    {
        if (device is Gamepad)
        {
            return "Gamepad";
        }
        if (device is Keyboard)
        {
            return "Keyboard&Mouse";
        }
        if (device is Mouse)
        {
            return "Keyboard&Mouse";
        }
        return null;
    }

    private void Awake()
    {

        /*        InputSystem.onActionChange += (obj, change) => {
                    if (change == InputActionChange.ActionPerformed)
                    {
                        InputDevice lastDevice = (obj as InputAction).activeControl.device;
                        Debug.Log($"last device->{lastDevice}");

                        if (Application.isPlaying)
                        {
                            //Debug.Log($"Control {control} changed value to {control.ReadValueFromEventAsObject(eventPtr)}");
                            if (GameManager.instance.players.Count > 0)
                            {
                                //InputControlScheme scheme = InputControlScheme.FindControlSchemeForDevice(lastDevice, inputActions.controlSchemes).GetValueOrDefault();
                                string scheme = GetCorrespondingControlScheme(lastDevice);
                                //Make sure this device hasn't already been added.
                                if (GameManager.instance.players.TrueForAll(p => !p.device.deviceId.Equals(lastDevice.deviceId) && !p.controlScheme.Equals(scheme)))
                                {

                                    if (useSelectableIcons) //if we are in the selection scene
                                    {
                                        //Debug.Log(((InputControlScheme)InputControlScheme.FindControlSchemeForDevice(control.device, inputActions.controlSchemes)));
                                        Debug.Log(GameManager.instance.players.ToString() + " " + GameManager.instance.players.Count);
                                        //Create a new cursor for this player.
                                        //PlayerInput.Instantiate(playerCursorPrefab, -1, null, -1, control.device);

                                        PlayerInput curInput = PlayerInput.Instantiate(playerCursorPrefab, -1, scheme, -1, lastDevice);
                                        Cursor c = curInput.GetComponent<Cursor>();
                                        //Create the playerInfo for this player
                                        PlayerInfo playerInfo = new PlayerInfo(lastDevice, curInput.currentControlScheme, defaultIcon, GameManager.instance.stockTotal);
                                        Debug.Log(playerInfo);
                                        //add this new cursor/player to the global list of players
                                        GameManager.instance.players.Add(playerInfo);
                                        //set the character's index.
                                        c.characterIndex = GameManager.instance.players.Count - 1;

                                    }
                                }
                            }
                            else
                            {
                                if (useSelectableIcons) //if we are in the selection scene
                                {
                                    Debug.Log(GameManager.instance.players.ToString() + " " + GameManager.instance.players.Count);
                                    //Create a new cursor for this player.
                                    //PlayerInput.Instantiate(playerCursorPrefab, -1, null, -1, control.device);
                                    //InputControlScheme scheme = InputControlScheme.FindControlSchemeForDevice(lastDevice, inputActions.controlSchemes).GetValueOrDefault();
                                    string scheme = GetCorrespondingControlScheme(lastDevice);
                                    PlayerInput curInput = PlayerInput.Instantiate(playerCursorPrefab, -1, scheme, -1, lastDevice);
                                    Cursor c = curInput.GetComponent<Cursor>();
                                    //Create the playerInfo for this player
                                    PlayerInfo playerInfo = new PlayerInfo(lastDevice, curInput.currentControlScheme, defaultIcon, GameManager.instance.stockTotal);
                                    Debug.Log(playerInfo);
                                    //add this new cursor/player to the global list of players
                                    GameManager.instance.players.Add(playerInfo);
                                    //set the character's index.
                                    c.characterIndex = 0;

                                }
                            }
                        }
                    }
                };*/

        //This is what I'm going to use to monitor if a device has pressed any input and desires to join the game.
        /*InputDevice tempDevice = null;
        if (inputActions.Any((action) => 
        {
            tempDevice = action.activeControl.device;
            return action.WasPressedThisFrame(); 
        })) 
        {
            if (Application.isPlaying)
            {
                //Debug.Log($"Control {control} changed value to {control.ReadValueFromEventAsObject(eventPtr)}");
                if (GameManager.instance.players.Count > 0)
                {
                    //Make sure this device hasn't already been added.
                    if (GameManager.instance.players.TrueForAll(p => !p.device.deviceId.Equals(tempDevice.deviceId)))
                    {

                        if (useSelectableIcons) //if we are in the selection scene
                        {
                            //Debug.Log(((InputControlScheme)InputControlScheme.FindControlSchemeForDevice(control.device, inputActions.controlSchemes)));
                            Debug.Log(GameManager.instance.players.ToString() + " " + GameManager.instance.players.Count);
                            //Create a new cursor for this player.
                            //PlayerInput.Instantiate(playerCursorPrefab, -1, null, -1, control.device);
                            InputControlScheme scheme = InputControlScheme.FindControlSchemeForDevice(tempDevice, inputActions.controlSchemes).GetValueOrDefault();
                            PlayerInput curInput = PlayerInput.Instantiate(playerCursorPrefab, -1, scheme.name, -1, tempDevice);
                            Cursor c = curInput.GetComponent<Cursor>();
                            //Create the playerInfo for this player
                            PlayerInfo playerInfo = new PlayerInfo(tempDevice, curInput.currentControlScheme, defaultIcon*//*null*//*, GameManager.instance.stockTotal)*//*c.CreatePlayerInfo()*//*;
                            Debug.Log(playerInfo);
                            //add this new cursor/player to the global list of players
                            GameManager.instance.players.Add(playerInfo);
                            //set the character's index.
                            c.characterIndex = GameManager.instance.players.Count - 1;

                        }
                    }
                }
                else
                {
                    if (useSelectableIcons) //if we are in the selection scene
                    {
                        Debug.Log(GameManager.instance.players.ToString() + " " + GameManager.instance.players.Count);
                        //Create a new cursor for this player.
                        //PlayerInput.Instantiate(playerCursorPrefab, -1, null, -1, control.device);
                        InputControlScheme scheme = InputControlScheme.FindControlSchemeForDevice(tempDevice, inputActions.controlSchemes).GetValueOrDefault();
                        PlayerInput curInput = PlayerInput.Instantiate(playerCursorPrefab, -1, scheme.name, -1, tempDevice);
                        Cursor c = curInput.GetComponent<Cursor>();
                        //Create the playerInfo for this player
                        PlayerInfo playerInfo = new PlayerInfo(tempDevice, curInput.currentControlScheme, defaultIcon*//*null*//*, GameManager.instance.stockTotal)*//*c.CreatePlayerInfo()*//*;
                        Debug.Log(playerInfo);
                        //add this new cursor/player to the global list of players
                        GameManager.instance.players.Add(playerInfo);
                        //set the character's index.
                        c.characterIndex = 0;

                    }
                }
            }
        };*/

        // Wait for first button press on a gamepad.
        /*InputSystem.onEvent
            .ForDevice<Keyboard>()
            .Where(e => e.HasButtonPress())
            .CallOnce(ctrl => Debug.Log($"Button {ctrl} pressed"));*/

        InputSystem.onEvent.Where(e => e.HasButtonPress()).Call(eventPtr =>
        {
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;
            if (Application.isPlaying)
            {
                foreach (var control in eventPtr.EnumerateChangedControls())
                {

                    string scheme = GetCorrespondingControlScheme(control.device);

                    //Debug info for what changed about each control.
                    //Debug.Log($"Control {control} changed value to {control.ReadValueFromEventAsObject(eventPtr)}");
                    //Debug.Log(eventPtr.type.ToString());
                    if (GameManager.instance.players.Count > 0)
                    {
                        
                        //Make sure this device hasn't already been added.
                        if (GameManager.instance.players.TrueForAll(p => !(p.device == Keyboard.current && control.device == Mouse.current) && !p.device.deviceId.Equals(control.device.deviceId)/* && !p.controlScheme.Equals(scheme)*/))
                        {

                            if (useSelectableIcons) //if we are in the selection scene
                            {
                                //Debug.Log(((InputControlScheme)InputControlScheme.FindControlSchemeForDevice(control.device, inputActions.controlSchemes)));
                                //Debug.Log(GameManager.instance.players.ToString() + " " + GameManager.instance.players.Count);
                                //Create a new cursor for this player.
                                //PlayerInput.Instantiate(playerCursorPrefab, -1, null, -1, control.device);
                                //InputControlScheme scheme = InputControlScheme.FindControlSchemeForDevice(control.device, inputActions.controlSchemes).GetValueOrDefault();
                                PlayerInput curInput = PlayerInput.Instantiate(playerCursorPrefab, -1, scheme, -1, control.device);
                                
                                //Add to the global cursors list.
                                cursors.Add(curInput.gameObject);
                                //REALLY IMPORTANT THAT WE SET THE SCALE HERE.
                                curInput.transform.localScale = Vector3.one;
                                Cursor c = curInput.GetComponent<Cursor>();
                                //Create the playerInfo for this player
                                PlayerInfo playerInfo = new PlayerInfo(control.device, curInput.currentControlScheme, defaultIcon, GameManager.instance.stockTotal);
                                //Debug.Log(playerInfo);
                                //add this new cursor/player to the global list of players
                                GameManager.instance.players.Add(playerInfo);
                                //set the character's index.
                                c.characterIndex = GameManager.instance.players.Count - 1;

                            }
                        }
                    }
                    else
                    {
                        if (useSelectableIcons) //if we are in the selection scene
                        {
                            //string scheme = GetCorrespondingControlScheme(control.device);
                            //Debug.Log(GameManager.instance.players.ToString() + " " + GameManager.instance.players.Count);
                            //Create a new cursor for this player.
                            //PlayerInput.Instantiate(playerCursorPrefab, -1, null, -1, control.device);
                            //InputControlScheme scheme = InputControlScheme.FindControlSchemeForDevice(control.device, inputActions.controlSchemes).GetValueOrDefault();
                            PlayerInput curInput = PlayerInput.Instantiate(playerCursorPrefab, -1, scheme, -1, control.device);
                            //Add to the global cursors list.
                            cursors.Add(curInput.gameObject);
                            Cursor c = curInput.GetComponent<Cursor>();
                            //Create the playerInfo for this player
                            PlayerInfo playerInfo = new PlayerInfo(control.device, curInput.currentControlScheme, defaultIcon, GameManager.instance.stockTotal);
                            //Debug.Log(playerInfo);
                            //add this new cursor/player to the global list of players
                            GameManager.instance.players.Add(playerInfo);
                            //set the character's index.
                            c.characterIndex = 0;

                        }
                    }
                }
            }
        });

        /*InputSystem.onEvent.Call(eventPtr =>
        {
            if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
                return;
            if (Application.isPlaying)
            {
                foreach (var control in eventPtr.EnumerateChangedControls())
                {
                    

                    *//*if (inputActions.Any((action) => )*//*
                    Debug.Log($"Control {control} changed value to {control.ReadValueFromEventAsObject(eventPtr)}");
                    Debug.Log(eventPtr.type.ToString());
                    if (GameManager.instance.players.Count > 0)
                    {
                        //Make sure this device hasn't already been added.
                        if (GameManager.instance.players.TrueForAll(p => !p.device.deviceId.Equals(eventPtr.deviceId)))
                        {

                            if (useSelectableIcons) //if we are in the selection scene
                            {
                                string scheme = GetCorrespondingControlScheme(control.device);
                                //Debug.Log(((InputControlScheme)InputControlScheme.FindControlSchemeForDevice(control.device, inputActions.controlSchemes)));
                                Debug.Log(GameManager.instance.players.ToString() + " " + GameManager.instance.players.Count);
                                //Create a new cursor for this player.
                                //PlayerInput.Instantiate(playerCursorPrefab, -1, null, -1, control.device);
                                //InputControlScheme scheme = InputControlScheme.FindControlSchemeForDevice(control.device, inputActions.controlSchemes).GetValueOrDefault();
                                PlayerInput curInput = PlayerInput.Instantiate(playerCursorPrefab, -1, scheme, -1, control.device);
                                Cursor c = curInput.GetComponent<Cursor>();
                                //Create the playerInfo for this player
                                PlayerInfo playerInfo = new PlayerInfo(control.device, curInput.currentControlScheme, defaultIcon, GameManager.instance.stockTotal);
                                Debug.Log(playerInfo);
                                //add this new cursor/player to the global list of players
                                GameManager.instance.players.Add(playerInfo);
                                //set the character's index.
                                c.characterIndex = GameManager.instance.players.Count - 1;

                            }
                        }
                    }
                    else
                    {
                        if (useSelectableIcons) //if we are in the selection scene
                        {
                            string scheme = GetCorrespondingControlScheme(control.device);
                            Debug.Log(GameManager.instance.players.ToString() + " " + GameManager.instance.players.Count);
                            //Create a new cursor for this player.
                            //PlayerInput.Instantiate(playerCursorPrefab, -1, null, -1, control.device);
                            //InputControlScheme scheme = InputControlScheme.FindControlSchemeForDevice(control.device, inputActions.controlSchemes).GetValueOrDefault();
                            PlayerInput curInput = PlayerInput.Instantiate(playerCursorPrefab, -1, scheme, -1, control.device);
                            Cursor c = curInput.GetComponent<Cursor>();
                            //Create the playerInfo for this player
                            PlayerInfo playerInfo = new PlayerInfo(control.device, curInput.currentControlScheme, defaultIcon, GameManager.instance.stockTotal);
                            Debug.Log(playerInfo);
                            //add this new cursor/player to the global list of players
                            GameManager.instance.players.Add(playerInfo);
                            //set the character's index.
                            c.characterIndex = 0;

                        }
                    }
                }
            }
        });*/

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
                    //On disconnect we want to delete the cursor and their playerInfo
                    //BUT ONLY IN THE SELECTION SCENE
                    //If a controller is disconnected during gameplay it should auto pause
                    //and then let the user reconnect it. THIS IS REALLY IMPORTANT.

                    //we are in the selection scene
                    if (useSelectableIcons)
                    {

                        // Device got unplugged.
                        // Get the player that just unplugged this.
                        PlayerInfo p = GameManager.instance.players.First(p => p.device.Equals(device));
                        // Remove the player from the list.
                        // The cursor will destroy itself and it's other UI objects.
                        GameManager.instance.players.Remove(p);

                        //Does the cursor have an issue with it's controls?
                        GameObject cursorToDestroy = cursors.First(c => c.GetComponent<PlayerInput>().hasMissingRequiredDevices);
                        if (cursorToDestroy != null)
                        {
                            Destroy(cursorToDestroy);
                        }

                    }
                    else //We are in a game
                    {
                        //TODO:
                        //Pause the game and prompt the player to reconnect their controller.
                        //Another player can press a button to close the menu and quit the match
                        //Or the controller is reconnected and gets assigned back to the player that
                        //was using it.
                    }

                    // Debug the disconnect.
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
        foreach (Icon icon in selectableIconList.icons)
        {
            AddSelectableIcon(icon);
        }

        
        if (manuallyInitCharacters)
        {
            //get the devices and the characters they selected and instantiate them.
            int index = 0;
            foreach(PlayerInfo playerInfo in GameManager.instance.gameMode.players)
            {               
                //Create the Character Icons in game.
                characterIcons.Add(AddCharacterIcon(playerInfo.characterIcon));
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
                //Add them to the global cursor list
                cursors.Add(p.gameObject);
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
    public CharacterIcon AddCharacterIcon(Icon icon)
    {
        if (playerIconPrefab != null)
        {
            CharacterIcon obj = Instantiate(playerIconPrefab, characterIconPanel).GetComponent<CharacterIcon>();
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
            CharacterSelectIcon obj = Instantiate(playerSelectIconPrefab, characterIconPanel).GetComponent<CharacterSelectIcon>();
            obj.characterIcon = icon;
        }
    }

    /// <summary>
    /// Adds a character icon to the selectable grid.
    /// </summary>
    /// <param name="prefab">The gameobject icon to be added to the selectable grid UI</param>
    public PlayerUIIcon AddPlayerIcon(CharacterSelectIcon icon)
    {
        if (playerSelectIconPrefab != null)
        {
            PlayerUIIcon obj = Instantiate(playerSelectUIIconPrefab, playerIconPanel).GetComponent<PlayerUIIcon>();
            obj.currentlySelectedCharacter = null;
            return obj;
        }
        return null;
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
        p.GetComponent<Player>().characterIcon = characterIcons[characterIndex]/*AddPlayerIcon(playerInfo.characterIcon)*/;
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
        Debug.Log(playerInfo.characterIcon);
        PlayerInput p = PlayerInput.Instantiate(playerInfo.characterIcon.characterPrefab, -1, playerInfo.controlScheme, -1, playerInfo.device);
        p.GetComponent<Player>().characterIndex = characterIndex;
        //Set the stock of the player.
        p.GetComponent<Player>().stock = playerInfo.stock;
        //Debug.LogWarning(characterIcons[characterIndex]);
        p.GetComponent<Player>().characterIcon = characterIcons[characterIndex]/*AddPlayerIcon(playerInfo.characterIcon)*/;

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
            //Actually don't do that because it can mess some stuff up. Just know that the stock count is zero there now.
            //GameManager.instance.gameMode.players.RemoveAt(characterIndex);

            //Update the info to be modified info.
            GameManager.instance.gameMode.players[characterIndex] = modifiedInfo;
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
