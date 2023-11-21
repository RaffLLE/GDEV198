using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    // Game Values
    bool paused = false;

    public GameObject pauseMenu;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown((KeyCode.P))) {
            paused = !paused;
        }

        if (paused) {
            pauseMenu.SetActive(true);
            Time.timeScale = 0;
        }
        else {
            pauseMenu.SetActive(false);
            Time.timeScale = 1;
        }
    }
}
