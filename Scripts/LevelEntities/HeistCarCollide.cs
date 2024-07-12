using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeistCarCollide : MonoBehaviour
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
        if (collision.gameObject.tag != "Enemy")
        {
            front.GetComponent<PushHeistTruck>().setMove(false);
            back.GetComponent<PushHeistTruck>().setMove(false);
        }
        else
        {
            front.GetComponent<PushHeistTruck>().dampenSpeed(.75f);
            back.GetComponent<PushHeistTruck>().dampenSpeed(.75f);
        }    
        if (collision.gameObject.tag == "DestructibleDoor")
        {
            Debug.Log("WALL HIT");
            if (collision.gameObject.GetComponent<destructibleDoor>())
                collision.gameObject.GetComponent<destructibleDoor>().Crash();
        }
    }
}
