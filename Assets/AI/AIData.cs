using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIData : MonoBehaviour
{
    // list of possible targets in case multiple targets wanted
    public List<Transform> targets = null;
    public Collider2D[] obstacles = null;

    public Transform currentTarget;

    // get how many targets, if there are none then the count is 0
    public int GetTargetsCount() => targets == null ? 0 : targets.Count;
}