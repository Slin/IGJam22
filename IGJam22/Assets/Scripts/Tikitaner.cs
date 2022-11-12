using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tikitaner : MonoBehaviour
{
    private float walkTime = -1.0f;
    private Vector3 startPosition;
    private Vector3 randomDirection = new Vector3(0.0f, 0.0f, 0.0f);
    private bool isFalling = false;
    private float fallSpeed = 0.0f;
    private float raycastTimer = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if(isFalling)
        {
            fallSpeed += 9.81f * Time.deltaTime;
            transform.position -= Vector3.up * fallSpeed * Time.deltaTime;
            if(transform.position.y < -20.0f)
            {
                Destroy(gameObject);
            }
            return;
        }

        if(walkTime <= 0.0f)
        {
            walkTime = Random.Range(0.5f, 5.0f);
            randomDirection = new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f));
        }
        walkTime -= Time.deltaTime;
        
        if(Vector3.Distance(transform.localPosition, startPosition) > 10.0f)
        {
            randomDirection = startPosition - transform.localPosition;
        }
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(randomDirection), Time.deltaTime * 3.0f);
        transform.Translate(new Vector3(0.0f, 0.0f, Time.deltaTime * 5.0f));

        raycastTimer -= Time.deltaTime;
        if(raycastTimer <= 0.0f)
        {
            raycastTimer = Random.Range(0.1f, 1.0f);
            RaycastHit hit;
            bool didHit = Physics.Raycast(transform.position + transform.up * 20.0f, -transform.up, out hit);
            if(!didHit || hit.distance > 40.0f)
            {
                //Player walked over the edge and should fall down
                isFalling = true;
                return;
            }
            if(didHit)
            {
                transform.position -= transform.up * (hit.distance - 20.0f);
            }
        }
    }
}
