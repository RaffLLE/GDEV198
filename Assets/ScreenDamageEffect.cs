using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenDamageEffect : MonoBehaviour
{
    public Volume volume;
    private Vignette vignette;

    public PlayerHP health;
    public float maxVignetteValue;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        volume.profile.TryGet(out vignette); {
            vignette.intensity.value = (0.5f + Mathf.Clamp((1.0f - health.currHP/health.maxHP), 0.0f, 0.7f)) * maxVignetteValue;
        }
    }
}
