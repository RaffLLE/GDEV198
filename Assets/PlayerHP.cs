using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    public float maxHP;
    public float currHP;
    public bool isImmune;

    void Start() {
        currHP = maxHP;
        isImmune = false;
    }

    private IEnumerator immunityFrame(float duration) {
        isImmune = true;
        yield return new WaitForSeconds(duration);
        isImmune = false;
    }

    public void containHP() {
        currHP = Mathf.Clamp(currHP, 0.0f, maxHP);
    }

    public void heal(float value) {
        currHP += value;
        //currHP = Mathf.Clamp(currHP, maxHP, 0.0f);
    }

    public void damage(float value, float immunityDuration) {
        currHP -= value;
        //currHP = Mathf.Clamp(currHP, maxHP, 0.0f);
        StartCoroutine(immunityFrame(immunityDuration));
    }
}
