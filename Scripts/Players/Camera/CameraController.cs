using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(-50, 50)]
    public float camX;
    [Range(00, 50)]
    public float camY;
    [Range(-50, 50)]
    public float camZ;
    [Range(0, 180)]
    public float rotAngleX;
    [Range(0, 180)]
    public float rotAngleY;
    [Range(0, 180)]
    public float rotAngleZ;



    // Start is called before the first frame update
    void Start()
    {
        Vector3 rot = new Vector3(rotAngleX, rotAngleY, rotAngleZ);
        transform.SetPositionAndRotation(new Vector3(camX, camY, camZ), Quaternion.Euler(rot));
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 rot = new Vector3 (rotAngleX, rotAngleY, rotAngleZ);
        //transform.SetPositionAndRotation(new Vector3(camX, camY, camZ), Quaternion.Euler(rot));
    }
}
