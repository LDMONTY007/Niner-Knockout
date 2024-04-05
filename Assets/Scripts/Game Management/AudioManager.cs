using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public AudioSource globalSource;

    //This is not a signleton pattern because I need this to be a monobehavior.
    public static AudioManager instance;

    private void Awake()
    {
        instance = this;
        globalSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
