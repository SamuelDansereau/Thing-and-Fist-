using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carCollide : MonoBehaviour
{
    [SerializeField]
    GameObject front, back;

    // Start is called before the first frame update
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            Debug.Log("crashing");
            front.GetComponent<PushTruck>().Crash();
            back.GetComponent<PushTruck>().Crash();
        }
        else if (collision.gameObject.tag == "Floor" || collision.gameObject.tag == "Untagged" || collision.gameObject.tag == "PushTruck")
        {
            return;
        }
        
    }
}
