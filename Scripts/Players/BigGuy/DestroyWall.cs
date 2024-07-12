using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;
using Unity.Netcode;
using System.Net;

public class DestroyWall : NetworkBehaviour
{
    [SerializeField]
    private float health, iFrames;
    [SerializeField]
    GameObject[] blocks;

    bool beDamaged;

    [SerializeField]
    private float despawnTime = 30;
    private int timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        //timer; not currently in use but not to be removed yet
        //StartCoroutine(despawnObject());

        beDamaged = true;
    }

    private IEnumerator despawnObject()
    {
        yield return new WaitForSeconds(despawnTime);

        // don't understand why all this is necessary?
        /*while (timer < despawnTime)
        {
            float waitTime = 1f / despawnTime;
            WaitForSeconds time = new WaitForSeconds(waitTime);
            timer++;
            yield return time;
        }*/
        if(PlayerManager.Instance.IsOnline())
        {
            if (!IsServer)
            {
                WallDestroyServerRpc();
            }
            if (IsServer)
            {
                if (IsOwner)
                    WallDestroyClientRpc();
            }
        }
        Destroy(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void WallDestroyServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }

    [ClientRpc]
    private void WallDestroyClientRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }

    public void doDamage(float damageDone)
    {
        if (!beDamaged) { return; }

        health -= damageDone;

        for (int i = 0; i < damageDone; i++)
        {
            if (health >= 0 && (int)health + i < blocks.Length)
            {
                blocks[(int)health + i].SetActive(false);
            }
        }

        if (health <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            beDamaged = false;
            Invoke(nameof(ResetDamage), iFrames);
        }
    }
    void ResetDamage()
    {
        beDamaged = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            doDamage(1);
        }
        else if(other.gameObject.tag == "Bullet")
        {
            if (other.gameObject.GetComponent<Molotov>() != null)
            {
                doDamage(1);
            }
        }
        else if (other.gameObject.tag == "AOE")
        {
            UnityEngine.Debug.Log("AOE");
            doDamage(1);
        }
        else
        {
            UnityEngine.Debug.Log(other.gameObject);
        }
    }
}
