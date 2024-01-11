using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;

public class GridGizmos : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        for (int i = 0; i < 100;  i++)
        {
            Handles.color = i % 2 == 0 ? Color.red : Color.green;
            Handles.Label(new Vector3(i, 0f, 0f), (i).ToString());
            Handles.Label(new Vector3(-i, 0f, 0f), (i).ToString());
        }
    }
#endif
}
