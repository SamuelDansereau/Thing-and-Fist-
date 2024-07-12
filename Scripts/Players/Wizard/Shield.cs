using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Netcode;

public class Shield : NetworkBehaviour
{
    [SerializeField]
    private float health, iFrames;
    private Transform fist;
    private GameObject thing;
    private bool active;
    [SerializeField]
    GameObject[] blocks;
    bool beDamaged;

    [Header("SFX")]
    [SerializeField] FMODUnity.EventReference ShieldSFX;
    private FMOD.Studio.EventInstance ShieldSFXInstance;

    // Start is called before the first frame update
    void Start()
    {
        active = true;
        beDamaged = true;
        ShieldSFXInstance = FMODEngineManager.CreateSound(ShieldSFX, 0.85f);
        ShieldSFXInstance.start();
    }

    // Update is called once per frame
    void Update()
    {
        fist = FindObjectOfType<BigGuyMovement>().transform;
        transform.position = fist.position;
        if(health <= 0)
        {
            active = false;
            thing = FindObjectOfType<WizardMovement>().gameObject;
            thing.GetComponent<WizardCombat>().resetShield();
            if(PlayerManager.Instance.IsOnline())
            {
                if (!IsServer)
                {
                    DestroyServerRpc();
                }
                if (IsServer)
                {
                    if (!IsOwner)
                        return;
                    DestroyClientRpc();
                }
            }
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }

    [ClientRpc]
    private void DestroyClientRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }

    /*public void setFist(Transform him)
    {
        fist = him;
    }*/

    /*public void setThing(GameObject them)
    {
        thing = them;
    }*/

    public bool checkActive()
    {
        return active;
    }

    public void doDamage(float damageDone)
    {
        if (beDamaged)
        {
            health -= damageDone;

            for (int i = 0; i < damageDone; i++)
            {
                if (health >= 0 && (int)health + i < blocks.Length)
                {
                    blocks[(int)health + i].SetActive(false);
                }
            }
            beDamaged = false;
            Invoke(nameof(ResetDamage), iFrames);
        }
    }

    void ResetDamage()
    {
        beDamaged = true;
    }
}
