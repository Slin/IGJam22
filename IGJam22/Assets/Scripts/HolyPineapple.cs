using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolyPineapple : MonoBehaviour
{
    public GameObject pinapplePrefab;
    public GameObject totemPrefab;

    public GameObject islandInstance;
    public PlayerCamera playerCamera;

    private GameObject pineappleInstance;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
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
            Vector3 hitPosition = transform.position + worldMouseDirection * hit.distance;

            pineappleInstance.SetActive(true);
            pineappleInstance.transform.position = hitPosition;

            if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                GameObject totem = Instantiate(totemPrefab);
                TikiTotemSpawn totemSpawn = totem.GetComponent<TikiTotemSpawn>();
                totemSpawn.playerCamera = playerCamera;
                totem.transform.position = hitPosition + Vector3.up * 500.0f; //Start above the click position to then fall from the sky
            }
        }
        else
        {
            pineappleInstance.SetActive(false);
        }
    }
}
