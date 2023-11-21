using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHelper : MonoBehaviour
{
    public GameObject Kataw;

    public GameObject proxy;
    public GameObject button;

    public void testing () {
        Kataw.SetActive(true);
        Destroy(proxy);
        button.SetActive(false);
    }
}
