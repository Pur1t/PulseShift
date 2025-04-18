using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clickable : MonoBehaviour
{
    public float alphaThreshold = 0.1f;
    private void Start()
    {
        this.GetComponent<Image>().alphaHitTestMinimumThreshold = alphaThreshold;
    }
}
