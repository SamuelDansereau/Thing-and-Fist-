using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PushHeistTruck : MonoBehaviour
{
    [SerializeField]
    Transform moveTo;
    public bool moving = false;
    public Vector3 where;
    [SerializeField]
    float moveRate;
    [SerializeField]
    GameObject car, otherSide;
    //This should be based on how far away the moveToForward and moveToBackward are like 10 less or something
    [SerializeField]
    float howFar;
    [SerializeField]
    float carDamage, carKnockback;
    // This is math im doing based on how far it should go howFar is taken into this calculation 
    float desiredDistanceMoved;
    float currentDist;
    // Start is called before the first frame update
    void Start()
    {
        desiredDistanceMoved = Vector3.Distance(car.transform.position, where);
    }

    // Update is called once per frame
    void Update()
    {
        currentDist = Vector3.Distance(car.transform.position, where);
        
        if(moving)
        {
            car.transform.position = Vector3.Lerp(car.transform.position, where, Time.deltaTime * moveRate);
            otherSide.GetComponent<PushHeistTruck>().setMove(false);
            if (currentDist <= desiredDistanceMoved)
            {
                setMove(false);
            }
        }
        if(!moving)
        {
            where = moveTo.position;
            desiredDistanceMoved = Vector3.Distance(car.transform.position, moveTo.position) - howFar;
        }
    }

    public void setMove(bool move)
    {
        moving = move;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            if (collision.gameObject.GetComponent<BasicEnemy>())
                collision.gameObject.GetComponent<BasicEnemy>().doDamage(carDamage, transform.forward, carKnockback, 0);
            dampenSpeed(.75f);
        }
        else
        {
            setMove(false);
            otherSide.GetComponent<PushHeistTruck>().setMove(false);
        }
        if (collision.gameObject.tag == "DestructibleDoor")
        {
            Debug.Log("WALL HIT");
            if (collision.gameObject.GetComponent<destructibleDoor>())
                collision.gameObject.GetComponent<destructibleDoor>().Crash();
        }
    }

    public void dampenSpeed(float speed)
    {
        desiredDistanceMoved += speed;
    }
}
