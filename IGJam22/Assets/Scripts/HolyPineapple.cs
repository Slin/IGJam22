using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolyPineapple : MonoBehaviour
{
    public GameObject pinapplePrefab;

    public GameObject totemPrefab;
    public GameObject totemBubblePrefab;
    public GameObject treePrefab;
    public GameObject treeBubblePrefab;

    public GameObject islandInstance;
    public PlayerCamera playerCamera;

    private GameObject pineappleInstance;
    private float plantTimer = -1.0f;
    private bool isPlanted = false;

    private GameObject totemBubbleInstance;
    private GameObject treeBubbleInstance;

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

        if(!isPlanted)
        {
            if(plantTimer <= 0.0f)
            {
                Vector3 worldMouseDirection = worldMousePosition - transform.position;
                worldMouseDirection.Normalize();

                RaycastHit hit;
                if(Physics.Raycast(transform.position, worldMouseDirection, out hit))
                {
                    Vector3 hitPosition = transform.position + worldMouseDirection * hit.distance;

                    pineappleInstance.SetActive(true);
                    pineappleInstance.transform.position = hitPosition + pineappleInstance.transform.up * 10.0f;

                    if(Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        plantTimer = 0.2f;
                    }
                }
                else
                {
                    pineappleInstance.SetActive(false);
                }
            }

            if(plantTimer > 0.0f)
            {
                float stepSize = Mathf.Min(Time.deltaTime, plantTimer);
                plantTimer -= stepSize;

                Vector3 newPosition = pineappleInstance.transform.position - pineappleInstance.transform.up * stepSize / 0.2f * 10.0f;
                pineappleInstance.transform.position = newPosition;

                if(plantTimer <= 0.0f)
                {
                    isPlanted = true;
                    plantTimer = -1.0f;

                    totemBubbleInstance = Instantiate(totemBubblePrefab);
                    totemBubbleInstance.transform.position = pineappleInstance.transform.position + pineappleInstance.transform.up * 50.0f - pineappleInstance.transform.right * 45.0f;
                    BuildSelectionBubble totemBubble = totemBubbleInstance.GetComponent<BuildSelectionBubble>();
                    totemBubble.playerCamera = playerCamera.gameObject;

                    treeBubbleInstance = Instantiate(treeBubblePrefab);
                    treeBubbleInstance.transform.position = pineappleInstance.transform.position + pineappleInstance.transform.up * 50.0f + pineappleInstance.transform.right * 45.0f;
                    BuildSelectionBubble treeBubble = treeBubbleInstance.GetComponent<BuildSelectionBubble>();
                    treeBubble.playerCamera = playerCamera.gameObject;
                }
            }
        }
        else
        {
            GameObject objectToCreate = null;
            if(Vector3.Distance(totemBubbleInstance.transform.position, worldMousePosition) < Vector3.Distance(treeBubbleInstance.transform.position, worldMousePosition))
            {
                objectToCreate = totemPrefab;
                totemBubbleInstance.GetComponent<Renderer>().material.SetColor("_GlowColor", Color.white);
                treeBubbleInstance.GetComponent<Renderer>().material.SetColor("_GlowColor", new Color(1 / 255.0f, 127 / 255.0f, 142 / 255.0f, 1.0f));
            }
            else
            {
                objectToCreate = treePrefab;
                totemBubbleInstance.GetComponent<Renderer>().material.SetColor("_GlowColor", new Color(1 / 255.0f, 127 / 255.0f, 142 / 255.0f, 1.0f));
                treeBubbleInstance.GetComponent<Renderer>().material.SetColor("_GlowColor", Color.white);
            }

            if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                Destroy(totemBubbleInstance);
                Destroy(treeBubbleInstance);

                if(objectToCreate)
                {
                    GameObject totem = Instantiate(objectToCreate);
                    TikiTotemSpawn totemSpawn = totem.GetComponent<TikiTotemSpawn>();
                    totemSpawn.playerCamera = playerCamera;
                    totem.transform.position = pineappleInstance.transform.position + Vector3.up * 500.0f; //Start above the click position to then fall from the sky
                }

                isPlanted = false;
            }
        }
    }
}
