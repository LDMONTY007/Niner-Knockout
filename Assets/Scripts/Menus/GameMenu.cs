using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameMenu : MonoBehaviour
{
    bool isPaused = false;

    private void Awake()
    {
        //Set our reference in the GameManager
        GameManager.instance.gameMenu = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!isPaused)
        {
            this.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //sub the players pause button to this method.
    public void Pause()
    {
        isPaused = !isPaused;
        //Set the game managers isPaused value to match ours so all coroutines know.
        GameManager.instance.isPaused = isPaused;
        if (isPaused)
        {
            Time.timeScale = 0f;
            gameObject.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }
    }

    //sub the players pause button to this method.
    public void Pause(InputAction.CallbackContext ctx)
    {
        isPaused = !isPaused;
        //Set the game managers isPaused value to match ours so all coroutines know.
        GameManager.instance.isPaused = isPaused;


        if (isPaused)
        {
            Time.timeScale = 0f;
            gameObject.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }
    }

    public void LoadScene(string sceneName)
    {
        if (isPaused == true)
        {
            Pause();
        }
        GameManager.instance.setScene(sceneName);

    }
}
