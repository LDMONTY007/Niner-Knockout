using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterManager : MonoBehaviour
{
    CinemachineTargetGroup targetGroup;

    public Transform playerIconPanel;

    

    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.characterManager = this;
        targetGroup = GetComponent<CinemachineTargetGroup>();
        PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;   

    }

    private void OnPlayerJoined(PlayerInput obj)
    {
        //weight is how much they affect the camera,
        //radius is the radius around the transform
        //required to be kept in the camera view.
        targetGroup.AddMember(obj.transform, 1f, 1f);
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
