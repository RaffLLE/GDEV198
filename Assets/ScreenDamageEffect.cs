using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenDamageEffect : MonoBehaviour
{
    public Volume volume;
    private Vignette vignette;

    private GameObject player;
    public float maxVignetteValue;

    void Start() {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null) {
            volume.profile.TryGet(out vignette); {
                if (player.GetComponent<FINALPlayerScript>().isCursed) {
                    vignette.intensity.value += Time.deltaTime * 2.0f;
                    if (vignette.intensity.value > 2.5f) {
                        vignette.intensity.value = 2.5f;
                    }
                    vignette.color.value = Color.black;
                }
                else if (player.GetComponent<FINALPlayerScript>().isDecaying) {
                    vignette.intensity.value = (1.0f - player.GetComponent<FINALPlayerScript>().playerHealth.currHP/player.GetComponent<FINALPlayerScript>().playerHealth.maxHP) * maxVignetteValue;
                    vignette.color.value = new Color(0.1333333f, 0.0f, 0.0f, 1.0f);
                }
                else if (player.GetComponent<FINALPlayerScript>().playerHealth.currHP > 0)
                vignette.intensity.value = (0.5f + Mathf.Clamp((1.0f - player.GetComponent<FINALPlayerScript>().playerHealth.currHP/player.GetComponent<FINALPlayerScript>().playerHealth.maxHP), 0.0f, 0.7f)) * maxVignetteValue;
            }
        }
    }

    public void changeColor(Color color) {
        volume.profile.TryGet(out vignette); {
            vignette.color.value = color;
        }
    }
}
