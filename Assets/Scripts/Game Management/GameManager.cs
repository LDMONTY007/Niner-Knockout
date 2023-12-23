using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Scene = UnityEngine.SceneManagement.Scene;

public class GameManager
{
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

    //so for each list index 1 will be if it is alive or not and index 2 will be the actual prefab.
    //public List<Robot> robotPrefabs = new List<Robot>();


    //robots that will die during gameplay.
    public List<Player> players = new List<Player>();

    public int finalRobotCount = 0;

    private GameManager()
    {

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
        SceneManager.LoadScene(scene);

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



