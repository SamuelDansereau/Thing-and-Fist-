using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigGuyCam : MonoBehaviour
{
    //Camera main;
    // Start is called before the first frame update
    //private Quaternion rotation;
    private Quaternion playerRotation;

    public float xDist;
    public float yDist = 46.29f;
    public float zDist = 29.48f;

    //variables for switching between lagging camera and fixed camera
    private bool cameraLag = false;
    public float speed = 1f;
    Vector3 defaultPosition;
    CameraShake shaker;

    void Start()
    {
        //main = Camera.main;
        playerRotation = transform.parent.rotation;
        defaultPosition = transform.position;
        shaker = GetComponent<CameraShake>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.rotation = Quaternion.Inverse(transform.parent.rotation) * playerRotation * transform.rotation;
        playerRotation = transform.parent.localRotation;

        if(!shaker.shakeTriggered)
        {
            if(cameraLag)
            {
                transform.position = defaultPosition;
                Vector3 toPos = transform.parent.position + new Vector3(xDist, yDist, zDist);
                Vector3 currentPos = Vector3.Lerp(transform.position, toPos, speed * Time.deltaTime);
                transform.position = currentPos;
                defaultPosition = currentPos;
                
            }
            else
            {
                transform.position = transform.parent.position + new Vector3(xDist, yDist, zDist);
                defaultPosition = transform.position;
            }
        }
    }

    public void LerpCameraSwitch(bool switchValue)
    {
        cameraLag = switchValue;
    }
}
