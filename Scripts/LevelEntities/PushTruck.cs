using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

public class PushTruck : NetworkBehaviour
{
    [SerializeField]
    Transform moveTo;
    public bool moving = false;
    public bool crashed = false;
    public Vector3 where;
    [SerializeField]
    float moveRate;
    [SerializeField]
    GameObject car, otherSide;
    //This should be based on how far away the moveToForward and moveToBackward are like 10 less or something
    [SerializeField]
    float howFar;
    [SerializeField]
    float carDamage, carKnockback, dampenRate;
    // This is math im doing based on how far it should go howFar is taken into this calculation 
    float desiredDistanceMoved;
    float currentDist;
    private NetworkVariable<bool> networkMove = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    // Start is called before the first frame update
    void Start()
    {
        desiredDistanceMoved = Vector3.Distance(car.transform.position, where);
    }

    // Update is called once per frame
    void Update()
    {
        if (!crashed)
        {
            currentDist = Vector3.Distance(car.transform.position, where);
            if (PlayerManager.Instance.IsOnline())
                moving = networkMove.Value;

            if (moving)
            {
                car.transform.position = Vector3.Lerp(car.transform.position, where, Time.deltaTime * moveRate);
                //otherSide.GetComponent<PushTruck>().setMove(false);
                if (currentDist <= desiredDistanceMoved)
                {
                    setMove(false);
                }
            }
            if (!moving)
            {
                where = moveTo.position;
                desiredDistanceMoved = Vector3.Distance(car.transform.position, moveTo.position) - howFar;
            }
        }
    }

    public void setMove(bool move)
    {
        if (!crashed)
        {
            if (!PlayerManager.Instance.IsOnline())
            {
                if (moving && move)
                {
                    where = moveTo.position;
                    desiredDistanceMoved = Vector3.Distance(car.transform.position, moveTo.position) - howFar;
                }
                moving = move;

            }
            if (PlayerManager.Instance.IsOnline())
            {
                if (!IsServer)
                {
                    if (move == true)
                        SetMovingTrueServerRpc();
                    if (move == false)
                        SetMovingFalseServerRpc();
                }
                if (IsServer)
                {
                    if (!IsOwner)
                        return;
                    if (move == true)
                        SetMovingTrueServerRpc();
                    if (move == false)
                        SetMovingFalseServerRpc();
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetMovingTrueServerRpc()
    {
        networkMove.Value = true;
    }

    [ClientRpc]
    private void SetMovingTrueClientRpc()
    {
        networkMove.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetMovingFalseServerRpc()
    {
        networkMove.Value = false;
    }

    [ClientRpc]
    private void SetMovingFalseClientRpc()
    {
        networkMove.Value = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if(collision.gameObject.tag == "Enemy")
        {
            if (collision.gameObject.GetComponent<BasicEnemy>())
                collision.gameObject.GetComponent<BasicEnemy>().doDamage(carDamage, transform.forward, carKnockback, 0);
            dampenSpeed(dampenRate);
        }
        else if(collision.gameObject.tag == "Wall")
        {
            Crash();
        }
        else if(collision.gameObject.tag == "Floor" || collision.gameObject.tag == "Untagged" || collision.gameObject.tag == "PushTruck" || collision.gameObject.tag == "HeistTruck")
        {
            return;
        }
    }

    public void dampenSpeed(float speed)
    {
        desiredDistanceMoved *= speed;
    }

    public void Crash()
    {
        if (!crashed)
        {
            crashed = true;
            setMove(false);
            otherSide.GetComponent<PushTruck>().Crash();
            otherSide.GetComponent<PushTruck>().setMove(false);
        }
    }
}
