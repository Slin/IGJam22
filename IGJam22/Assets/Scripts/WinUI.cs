using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinUI : MonoBehaviour
{
    private TikiSettlers settlers;
    private RawImage image;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<RawImage>();
        settlers = FindObjectOfType<TikiSettlers>();
    }

    // Update is called once per frame
    void Update()
    {
         image.enabled = settlers.didWin;
    }
}
