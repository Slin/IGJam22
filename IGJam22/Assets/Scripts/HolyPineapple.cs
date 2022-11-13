using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolyPineapple : MonoBehaviour
{
    public float costTotem = 100;
    public float costTree = 20;

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
    private TikiSettlers settlers;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        pineappleInstance = Instantiate(pinapplePrefab, new Vector3(0, 0, 0), Quaternion.identity);
        settlers = FindObjectOfType<TikiSettlers>();
    }

    // Update is called once per frame
    void Update()
    {
        Camera camera = GetComponent<Camera>();
        float cameraDistance = Mathf.Max(Vector3.Distance(camera.transform.position, pineappleInstance.transform.position), 1.0f);
        Vector3 worldMousePosition = Input.mousePosition;
        worldMousePosition.z = cameraDistance;
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
                    pineappleInstance.transform.position = hitPosition;

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

                    float distanceToCamera = Vector3.Distance(newPosition, playerCamera.transform.position) * 0.2f;

                    totemBubbleInstance = Instantiate(totemBubblePrefab);
                    totemBubbleInstance.transform.position = pineappleInstance.transform.position + pineappleInstance.transform.up * distanceToCamera * 2.0f - playerCamera.transform.right * 0.5f * distanceToCamera;
                    BuildSelectionBubble totemBubble = totemBubbleInstance.GetComponent<BuildSelectionBubble>();
                    totemBubble.playerCamera = playerCamera.gameObject;

                    treeBubbleInstance = Instantiate(treeBubblePrefab);
                    treeBubbleInstance.transform.position = pineappleInstance.transform.position + pineappleInstance.transform.up * distanceToCamera * 2.0f + playerCamera.transform.right * 0.5f * distanceToCamera;
                    BuildSelectionBubble treeBubble = treeBubbleInstance.GetComponent<BuildSelectionBubble>();
                    treeBubble.playerCamera = playerCamera.gameObject;
                }
            }
        }
        else
        {
            GameObject objectToCreate = null;
            float cost = 0.0f;

            float distanceToTotem = Vector3.Distance(totemBubbleInstance.transform.position, worldMousePosition);
            float distanceToTree = Vector3.Distance(treeBubbleInstance.transform.position, worldMousePosition);
            if(Mathf.Min(distanceToTotem) < 0.5f * cameraDistance)
            {
                if(distanceToTotem < distanceToTree)
                {
                    cost = costTotem;
                    objectToCreate = totemPrefab;
                    totemBubbleInstance.GetComponent<Renderer>().material.SetColor("_GlowColor", (cost <= settlers.worshipOMeter)? Color.white : Color.red);
                    treeBubbleInstance.GetComponent<Renderer>().material.SetColor("_GlowColor", new Color(1 / 255.0f, 127 / 255.0f, 142 / 255.0f, 1.0f));
                }
                else
                {
                    cost = costTree;
                    objectToCreate = treePrefab;
                    totemBubbleInstance.GetComponent<Renderer>().material.SetColor("_GlowColor", new Color(1 / 255.0f, 127 / 255.0f, 142 / 255.0f, 1.0f));
                    treeBubbleInstance.GetComponent<Renderer>().material.SetColor("_GlowColor", (cost <= settlers.worshipOMeter)? Color.white : Color.red);
                }
            }
            else
            {
                totemBubbleInstance.GetComponent<Renderer>().material.SetColor("_GlowColor", new Color(1 / 255.0f, 127 / 255.0f, 142 / 255.0f, 1.0f));
                treeBubbleInstance.GetComponent<Renderer>().material.SetColor("_GlowColor", new Color(1 / 255.0f, 127 / 255.0f, 142 / 255.0f, 1.0f));
            }

            if(Input.GetKeyDown(KeyCode.Mouse0) && (settlers.worshipOMeter >= cost || !objectToCreate))
            {
                Destroy(totemBubbleInstance);
                Destroy(treeBubbleInstance);

                if(objectToCreate)
                {
                    GameObject totem = Instantiate(objectToCreate);
                    TikiTotemSpawn totemSpawn = totem.GetComponent<TikiTotemSpawn>();
                    totemSpawn.playerCamera = playerCamera;
                    totem.transform.position = pineappleInstance.transform.position + Vector3.up * 500.0f; //Start above the click position to then fall from the sky

                    settlers.worshipOMeter -= cost;

                    if(objectToCreate == totemPrefab) costTotem += costTotem;
                    else if(objectToCreate == treePrefab) costTree += costTree;
                }

                isPlanted = false;
            }
        }
    }
}
