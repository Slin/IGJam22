using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildSelectionBubble : MonoBehaviour
{
    public GameObject playerCamera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(-playerCamera.transform.forward, playerCamera.transform.up);//Quaternion.FromToRotation(transform.position, playerCamera.transform.position);
    }
}
