using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class CharacterManager : MonoBehaviour
{
    CinemachineTargetGroup targetGroup;

    public Transform playerIconPanel;

    public GameObject playerSelectIconPrefab;

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
            foreach(PlayerInfo playerInfo in GameManager.instance.players)
            {
                //instantiate the prefab, auto assign the playerindex, use X control scheme, auto assign the split screen index, and use X device.
                PlayerInput.Instantiate(playerInfo.prefab, -1, playerInfo.controlScheme, -1, playerInfo.device);
                //THIS IS WHAT LINKS THE CONTROLLER TO THE SPECIFICALLY SPAWNED PLAYER. 
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
    public CharacterIcon AddPlayerIcon(GameObject prefab)
    {
        if (prefab != null)
        {
            return Instantiate(prefab, playerIconPanel).GetComponent<CharacterIcon>();
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
}
