using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    public float maxHP;
    public float currHP;
    private bool canHeal;
    private bool isImmune;

    void Start() {
        currHP = maxHP;
        canHeal = true;
        isImmune = false;
    }

    private IEnumerator immunityFrame(float duration) {
        isImmune = true;
        yield return new WaitForSeconds(duration);
        isImmune = false;
    }

    void heal(float value) {
        currHP += value;
        currHP = Mathf.Clamp(currHP, maxHP, 0.0f);
    }

    void damage(float value, float immunityDuration) {
        currHP -= value;
        currHP = Mathf.Clamp(currHP, maxHP, 0.0f);
        StartCoroutine(immunityFrame(immunityDuration));
    }
}
