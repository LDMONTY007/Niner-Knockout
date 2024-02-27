using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.Scene;

public class GameManager
{
    public bool isPaused = false;

    private static GameManager _instance;

    [HideInInspector]
    public static GameManager instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameManager();
            return _instance;
        }
        set
        {
            _instance = value;
        }
    }

    [HideInInspector]
    public static Scene currentScene;

    public CharacterManager characterManager;

    public GameMode gameMode;

    public List<string> scenes = new List<string>()
    {
        "TitleScene",
        "Level1",
        "Level2",
        "Level3",
        "Level4",
        "Level5"
    };

    public int currentIndex = 1;

    public int stockTotal = 3;

    //so for each list index 1 will be if it is alive or not and index 2 will be the actual prefab.
    //public List<Robot> robotPrefabs = new List<Robot>();


    //The input Device and character prefab the player selected. 
    //Cleared after each match.
    //assigned during the selection screen.
    public List<PlayerInfo> players = new List<PlayerInfo>();

    public int finalRobotCount = 0;

    //The game menu will set this var from within itself.
    public GameMenu gameMenu = null;

    private GameManager()
    {
        //set the target frame rate to be 60, DO NOT GO FASTER.
        Application.targetFrameRate = 60;
        Debug.Log(Application.targetFrameRate);
        //only call update every 1/60th of a second. 
        Time.captureFramerate = 60;
        currentScene = SceneManager.GetActiveScene(); //set current scene.
    }

    public void setScene(string scene)
    {

        //load scene
        if (scene == "TitleScene")
        {
            currentIndex = 1; //reset index;
            players.Clear();
        }
        if (SceneManager.GetSceneByName(scene) != null)
        {
            //before we load the scene we should set the
            //gameMenu to null just in case it was set this scene.
            gameMenu = null;
            gameMode = null;
            SceneManager.LoadScene(scene);
        }
        else
        {
            Debug.LogWarning("Scene : " + scene + " was not found in the build settings");
        }
        

    }


    //use this for slowing down time during things like impacts and right as you die before we switch scenes. Basically it'll do impact frames.
    public IEnumerator slowTime(float scale, float duration)
    {
        Time.timeScale = scale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

}

//class we will use for storing an array of robots and seeing if they are alive or not.
/*public class Robot
{
    public bool alive;
    public GameObject prefab;

    public Robot(bool alive, GameObject prefab)
    {
        this.alive = alive;
        this.prefab = prefab;
    }
}*/

public struct PlayerInfo
{
    public PlayerInfo(InputDevice device, string controlScheme, Icon characterIcon, int stock)
    {
        this.device = device;
        this.controlScheme = controlScheme; 
        this.characterIcon = characterIcon;
        this._stock = stock;
    }

    public InputDevice device;

    public string controlScheme;

    public Icon characterIcon;

    //Property with backing field used so that I can make the stock value minimum be zero
    private int _stock;
    public int stock { get { return _stock; } set { _stock = value < 0 ? 0 : value; } }
}


