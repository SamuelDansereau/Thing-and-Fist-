using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    //Camera main;
    // Start is called before the first frame update
    //private Quaternion rotation;
    private Quaternion playerRotation;

    public float xDist;
    public float yDist = 33f;
    public float zDist = 21f;

    
    //variables for switching between lagging camera and fixed camera
    public bool cameraLag = false;
    public float speed = 1f;
    float startSpeed;
    Vector3 defaultPosition;
    CameraShake shaker;

    public float dashEndTime = 0.7f;
    public float dashTimer;

    void Start()
    {
        //main = Camera.main;
        playerRotation = transform.parent.rotation;
        defaultPosition = transform.position;
        shaker = GetComponent<CameraShake>();
        startSpeed = speed;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = Quaternion.Inverse(transform.parent.rotation) * playerRotation * transform.rotation;
        playerRotation = transform.parent.localRotation;
            if(cameraLag)
            {
                transform.position = defaultPosition;
                Vector3 toPos = transform.parent.position + new Vector3(xDist, yDist, zDist);
                Vector3 currentPos = Vector3.Lerp(transform.position, toPos, speed * Time.deltaTime);
                transform.position = currentPos;
                defaultPosition = currentPos;
                speed += 10 * Time.deltaTime;
            }
            else
            {
                if(!shaker.shakeTriggered)
                {
                    transform.position = transform.parent.position + new Vector3(xDist, yDist, zDist);
                    defaultPosition = transform.position;
                }
                speed = startSpeed;
            }
    }

    void Update()
    {
        if(dashTimer > 0)
        {
            dashTimer -= Time.deltaTime;
        }
        if(dashTimer <= 0)
        {
            cameraLag = false;
        }
    }

    public void LerpCameraSwitch(bool switchValue)
    {
        cameraLag = switchValue;
        dashTimer = dashEndTime;
    }
}
