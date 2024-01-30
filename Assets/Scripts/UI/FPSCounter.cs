using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//this code is from the old Unity Standard Assets package that is now deprecated.
//this code is the accurate FPS the game is running at, not what the editor 
//is running at. The "stats" window in the editor is notoriously innacurate
//as it also accounts for the editors fps. This only calculates FPS for the game.

[RequireComponent(typeof(TextMeshProUGUI))]
public class FPSCounter : MonoBehaviour
{
    const float fpsMeasurePeriod = 0.5f;
    private int m_FpsAccumulator = 0;
    private float m_FpsNextPeriod = 0;
    private int m_CurrentFps;
    const string display = "{0} FPS";
    private TextMeshProUGUI m_Text;


    private void Start()
    {
        m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        m_Text = GetComponent<TextMeshProUGUI>();
    }


    private void Update()
    {
        // measure average frames per second
        m_FpsAccumulator++;
        if (Time.realtimeSinceStartup > m_FpsNextPeriod)
        {
            m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
            m_FpsAccumulator = 0;
            m_FpsNextPeriod += fpsMeasurePeriod;
            m_Text.text = string.Format(display, m_CurrentFps);
        }
    }
}
