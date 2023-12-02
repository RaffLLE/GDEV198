using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSManager : MonoBehaviour
{

    public int desiredFrames;

    // Start is called before the first frame update
    void Start()
    {
        // helps consistent FPS
        Application.targetFrameRate = desiredFrames;
    }
}
