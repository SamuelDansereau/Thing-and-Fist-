using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorPassthrough : MonoBehaviour
{
    GameObject firstPlayer, secondPlayer, Car;
    [SerializeField]
    GameObject Door;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (firstPlayer != null && secondPlayer != null && Car != null) 
        {
            Door.GetComponent<destructibleDoor>().Collapse();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player") 
        { 
            if (firstPlayer == null) 
            {
                firstPlayer = other.gameObject;
            }
            else if(secondPlayer == null && other.gameObject != firstPlayer)
            {
                secondPlayer = other.gameObject;
            }
        }
        if(other.gameObject.tag == "HeistTruck")
        {
            Car = other.gameObject;
        }
    }



}
