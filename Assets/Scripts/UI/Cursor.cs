using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

//I stole a lot of the code from the VirtualMouseInput script
//but you can't use multiple of those at once so I had to create
//a custom one.
public class Cursor : MonoBehaviour
{
    public PlayerInput playerInput;

    public GameObject coinPrefab;

    InputAction moveAction;
    InputAction selectAction;

    [Header("Motion")]
    [Tooltip("Speed in pixels per second with which to move the cursor. Scaled by the input from 'Stick Action'.")]
    [SerializeField] private float cursorSpeed = 400;

    public RectTransform cursorTransform;

    private Canvas canvas; // Canvas that gives the motion range for the software cursor.
    private double lastTime;
    private Vector2 lastStickValue;

    //Raycasting/selection vars.
    private PointerEventData pointerEventData = new PointerEventData(null);
    public GraphicRaycaster gr;

    private int _characterIndex = -1;

    //used to store the index 
    //where our character was inserted into 
    //the list of characters so we can replace it
    //if they decide to change characters.
    public int characterIndex {  get { return _characterIndex; } set { _characterIndex = value; } }
    //if the user already selected a character.
    public bool didSelect;
    private GameObject coinInstance;

    public PlayerUIIcon playerUIIcon;

    // Start is called before the first frame update
    void Start()
    {
       

/*        //Might want to change this in the future especially when loading the scene again.
        characterIndex = curCharacterIndex;
        curCharacterIndex++;
        //End of area we need to change.*/

        canvas = FindObjectOfType<Canvas>();

        if (canvas == null)
            Debug.LogError("CURSORS SHOULD ONLY BE SPAWNED IN SCENES WITH A CANVAS");

        cursorTransform.SetParent(canvas.transform);
        gr = GetComponentInParent<GraphicRaycaster>();
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["point"];
        selectAction = playerInput.actions["click"];

        playerUIIcon = GameManager.instance.characterManager.AddPlayerIcon(null);
        //Set the icon's character index.
        playerUIIcon.characterIndex = characterIndex;
        playerUIIcon.playerName.text = "Player" + characterIndex.ToString();

        //When the device is disconnected destroy us and our gameobject.
        //For this to get called make sure the "Send Unity Events" mode is selected in the "PlayerInput" component.
        //playerInput.deviceLostEvent.AddListener(_ => Debug.LogWarning("HERE")/*Destroy(this.gameObject)*/);
        //Can't do this here. Nevermind, it causes an error when you destroy this object because the PlayerInput no longer exists but this listener
        //is trying to find it.
    }

    private void Update()
    {
        if (selectAction.WasPressedThisFrame())
        {
            Select();
        }

        if (playerInput.currentControlScheme.Equals("Keyboard&Mouse"))
        {

            // Update position.
            var currentPosition = this.moveAction.ReadValue<Vector2>();

            //can't clamp to canvas for the mouse and also this is taking too long
            //so I'll have to fix this at some point.
/*            // Clamp to canvas.
            if (canvas != null)
            {
                RectTransform clampRect = (RectTransform)canvas.transform;
                Vector3 minPosition = clampRect.rect.min - cursorTransform.rect.min;
                Vector3 maxPosition = clampRect.rect.max - cursorTransform.rect.max;

                // Clamp to canvas.
                currentPosition.x = Mathf.Clamp(currentPosition.x, minPosition.x, maxPosition.x);
                currentPosition.y = Mathf.Clamp(currentPosition.y, minPosition.y, maxPosition.y);

*//*                Vector2 pos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, *//*Input.mousePosition*//*this.moveAction.ReadValue<Vector2>(), canvas.worldCamera, out pos);
                currentPosition = canvas.transform.TransformPoint(pos);*//*
            }*/

            // Update software cursor transform, if any.
            if (cursorTransform != null)
                cursorTransform.position = currentPosition;
        }
        else
        {
            UpdateMotion();
        }
    }

/*    public PlayerInfo CreatePlayerInfo()
    {
        return new PlayerInfo(playerInput.GetDevice<InputDevice>(), playerInput.currentControlScheme, null, GameManager.instance.stockTotal);
    }*/



    private void Select()
    {
        pointerEventData.position = transform.position;
        List<RaycastResult> results = new List<RaycastResult>();
        //gr.Raycast(pointerEventData, results);
        EventSystem.current.RaycastAll(pointerEventData, results);
        if (results.Count > 0)
        {
            if (results[0].gameObject.TryGetComponent(out CharacterSelectIcon selectIcon))
            {
                //assign the device controlling this player and the character prefab they selected to the GameManager before we load the 
                //next scene.
                PlayerInfo playerInfo = new PlayerInfo(playerInput.GetDevice<InputDevice>(), playerInput.currentControlScheme, selectIcon.characterIcon, GameManager.instance.stockTotal);
                //if we already placed our coin, 
                //then when they click again it 
                //means they are trying to 
                //select a different character.
                if (didSelect)
                {
                    //Don't change did select because we are still selecting a character here.

                    //Destroy the coin that was placed before 
                    //and place a new one. 
                    if (coinInstance)
                    Destroy(coinInstance.gameObject);
                    coinInstance = createCoin(coinPrefab, cursorTransform.position, Quaternion.identity, canvas.transform, characterIndex);
                    //replace the reference so that 
                    //the old character they chose is 
                    //removed and we assign the new 
                    //character. 
                    GameManager.instance.players[characterIndex] = playerInfo;

                    //Play the audio of the selection
                    AudioManager.instance.globalSource.PlayOneShot(playerInfo.characterIcon.characterAnnouncement);

                    //Call to reassign the character we've selected.
                    playerUIIcon.ReassignCharacterIcon(selectIcon);
                }
                else
                {
                    //add the playerInfo to the GameManager player list.
                    //GameManager.instance.players.Add(playerInfo);
                    GameManager.instance.players[characterIndex] = playerInfo;
                    //set the character index in case we need to remove it.
                    //characterIndex = GameManager.instance.players.Count - 1;
                    //create the coin where the cursor currently is.
                    coinInstance = createCoin(coinPrefab, cursorTransform.position, Quaternion.identity, canvas.transform, characterIndex);
                    didSelect = true;
                    //print(results[0].gameObject.name);

                    //Play the audio of the selection
                    AudioManager.instance.globalSource.PlayOneShot(playerInfo.characterIcon.characterAnnouncement);

                    //Call to reassign the character we've selected.
                    playerUIIcon.ReassignCharacterIcon(selectIcon);
                }
            }
            if (results[0].gameObject.TryGetComponent(out Button button))
            {
                //tell the button we clicked it with our cursor.
                button.onClick.Invoke();
            }


        }
    }

    public GameObject createCoin(GameObject gameObject, Vector3 position,  Quaternion rotation, Transform parent, int playerIndex)
    {
        GameObject temp = Instantiate(coinPrefab, cursorTransform.position, Quaternion.identity, canvas.transform);
        //Set the color of the coin.
        temp.GetComponent<Image>().color = playerUIIcon.backgroundGradient.Evaluate(playerIndex);
        //Set the number on the coin.
        temp.GetComponentInChildren<TextMeshProUGUI>().text = "P" + playerIndex;
        return temp;
    }


    private void UpdateMotion()
    {
        if (playerInput == null)
            return;

        // Read current stick value.
        var stickAction = this.moveAction;
        if (stickAction == null)
            return;
        var stickValue = stickAction.ReadValue<Vector2>();
        if (Mathf.Approximately(0, stickValue.x) && Mathf.Approximately(0, stickValue.y))
        {
            // Motion has stopped.
            lastTime = default;
            lastStickValue = default;
        }
        else
        {
            var currentTime = InputState.currentTime;
            if (Mathf.Approximately(0, lastStickValue.x) && Mathf.Approximately(0, lastStickValue.y))
            {
                // Motion has started.
                lastTime = currentTime;
            }

            // Compute delta.
            var deltaTime = (float)(currentTime - lastTime);
            var delta = new Vector2(cursorSpeed * stickValue.x * deltaTime, cursorSpeed * stickValue.y * deltaTime);

            // Update position.
            var currentPosition = cursorTransform.anchoredPosition;
            var newPosition = currentPosition + delta;

            // Clamp to canvas.
            if (canvas != null)
            {
                RectTransform clampRect = (RectTransform)canvas.transform;
                Vector3 minPosition = clampRect.rect.min - cursorTransform.rect.min;
                Vector3 maxPosition = clampRect.rect.max - cursorTransform.rect.max;

                // Clamp to canvas.
                newPosition.x = Mathf.Clamp(newPosition.x, minPosition.x, maxPosition.x);
                newPosition.y = Mathf.Clamp(newPosition.y, minPosition.y, maxPosition.y);
            }

            // Update software cursor transform, if any.
            if (cursorTransform != null)
                cursorTransform.anchoredPosition = newPosition;

            lastStickValue = stickValue;
            lastTime = currentTime;
        }
    }

    //We call destroy if a player disconnects.
    //Therefore we should also destroy their icon in the UI 
    //if they disconnect.
    //We also remove them from the player list in CharacterManager.cs 
    //By listening for all the disconnections.

    //Yes, I am aware that this code should be all in one place and only get 
    //called by one thing but I haven't had the time to clean it up yet.
    //I wanted to call it from characterManager.cs but I didn't 
    //set the codebase up properly to store references for that.
    //I'll have to do that in the future.
    private void OnDestroy()
    {         
        Destroy(playerUIIcon.gameObject);
        if (coinInstance)
        Destroy(coinInstance.gameObject);
    }
}
