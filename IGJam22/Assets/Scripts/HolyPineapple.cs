using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolyPineapple : MonoBehaviour
{
    public GameObject pinapplePrefab;

    private GameObject pineappleInstance;

    // Start is called before the first frame update
    void Start()
    {
        pineappleInstance = Instantiate(pinapplePrefab, new Vector3(0, 0, 0), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 worldMousePosition = Input.mousePosition;
        worldMousePosition.z = 100.0f;
        Camera camera = GetComponent<Camera>();
        worldMousePosition = camera.ScreenToWorldPoint(worldMousePosition);
        Vector3 worldMouseDirection = worldMousePosition - transform.position;
        worldMouseDirection.Normalize();

        RaycastHit hit;
        if(Physics.Raycast(transform.position, worldMouseDirection, out hit))
        {
            pineappleInstance.SetActive(true);
            pineappleInstance.transform.position = transform.position + worldMouseDirection * hit.distance;
        }
        else
        {
            pineappleInstance.SetActive(false);
        }
    }
}
