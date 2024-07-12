using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Lure_Explosion : NetworkBehaviour
{
    public float duration;

    [Header("SFX")]
    [SerializeField] FMODUnity.EventReference ExplodeSFX;
    private FMOD.Studio.EventInstance ExplodeSFXInstance;


    // Start is called before the first frame update
    void Start()
    {
        ExplodeSFXInstance = FMODEngineManager.CreateSound(ExplodeSFX, 0.85f);
        ExplodeSFXInstance.start();
    }

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;
        if(duration <= 0)
        {
            if (PlayerManager.Instance.IsOnline())
            {
                if (!IsServer)
                {
                    blowUpServerRpc();
                }
                if (IsServer)
                {
                    if (!IsOwner)
                        return;
                    blowUpClientRpc();
                }
            }
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void blowUpServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }

    [ClientRpc]
    private void blowUpClientRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
