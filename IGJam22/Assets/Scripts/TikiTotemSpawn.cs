using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TikiTotemSpawn : MonoBehaviour
{
    public PlayerCamera playerCamera;
    private float fallSpeed = 100.0f;
    private bool isFalling = true;

    // Start is called before the first frame update
    void Start()
    {
        transform.eulerAngles = new Vector3(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if(isFalling)
        {
            fallSpeed += 9.81f * Time.deltaTime;
            float fallDistance = fallSpeed * Time.deltaTime;

            RaycastHit hit;
            bool didHit = Physics.Raycast(transform.position, -Vector3.up, out hit);
            if(didHit && hit.distance <= fallDistance)
            {
                transform.position -= Vector3.up * hit.distance;
                transform.parent = hit.collider.gameObject.transform.parent;
                playerCamera.DoTotemShake();
                isFalling = false;
            }
            else if(transform.position.y < 0.0f)
            {
                Destroy(gameObject);
            }
            else
            {
                Vector3 position = transform.position;
                position.y -= fallDistance;
                transform.position = position;
            }
        }
    }
}
