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
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.characterManager = this;
        targetGroup = GetComponent<CinemachineTargetGroup>();
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
        //InputSystem.devices
        /*foreach (InputDevice device in InputSystem.devices)
        {
            device.
        }*/
    }

    //used in the player selection screen
    //and called when a player joins.
    private void CreateSelectionCursor()
    {
        //create the new player as a child of the canvas.
        Instantiate(playerCursorPrefab, selectionCanvas.transform);
    }

    private void OnPlayerJoined(PlayerInput obj)
    {
        //weight is how much they affect the camera,
        //radius is the radius around the transform
        //required to be kept in the camera view.
        targetGroup.AddMember(obj.transform, 1f, 1f);
        //if this is a cursor parent it to the canvas.
/*        if (obj.CompareTag("Cursor"))
        {
            obj.transform.SetParent(selectionCanvas.transform, false);
        }*/
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
}
