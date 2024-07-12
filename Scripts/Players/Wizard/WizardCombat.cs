using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using Unity.Netcode;

public class WizardCombat : NetworkBehaviour
{

    //public GameObject indicator;
    GameObject scriptObj;

    //Taunt =================================================================
    [Header("Taunt")]
    public GameObject tauntObj;
    public float tauntCooldown;
    public int tauntObjLife;
    float currentTauntCD = 1f;
    bool canTaunt = true;
    [SerializeField]
    private GameObject tauntSpawn;
    //=======================================================================

    //Shooting ==============================================================
    [Header("Shooting")]

    //material vars
    //public Material shotMat;
    float red;
    float blue;
    float green;

    public GameObject bullet;
    public GameObject shootEffect;
    public GameObject bulletSpawn;
    public GameObject reticle;
    public Transform reticlePos;
    /*private NetworkVariable<GameObject> reticleObj = new NetworkVariable<GameObject>(null,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);*/
    float distance;
    public LayerMask wallLayerMask;
    public float bulletCD;
    float currentCD;
    public float shotIntensity;
    bool canFire = true;
    private IEnumerator coroutine;
    bool isShooting = false;
    private int bulletCount = 1;
    //=======================================================================

    //Shield ================================================================

    [Header("Shield")]
    private Transform Fist;
    [SerializeField]
    private GameObject shieldObject;
    [SerializeField]
    private float shieldMaxCD;
    float shieldCD;
    private bool shieldActive = false;
    GameObject shieldIcon;

    //=======================================================================


    //Sound =================================================================
    [Header("SFX")]
    //FMOD Ranged Player Shoot and Teleport SFX
    [SerializeField] FMODUnity.EventReference ShootSFX;
    private FMOD.Studio.EventInstance ShootSFXInstance;
    //[SerializeField] FMODUnity.EventReference TauntSFX;
    //private FMOD.Studio.EventInstance TauntSFXInstance;
    //=======================================================================

    //Controller Rumble ==========================================

    [Header("Rumble")]
    public float rumbleTimer;
    [Header("Still Rumble, These can only be between 0.0 - 1.0")]
    public float leftSideRumble; public float rightSideRumble;
    RumbleManager rumbler;

    //============================================================

    //Camera Shake ===============================================
    [Header("Camera Shake")]
    public Camera playerCam;
    [SerializeField]
    float shootShakeMagnitude;
    [SerializeField]
    float shootShakeTime;
    CameraShake shaker;
    //============================================================


    PlayerHealth hp;
    WizardMovement mov;

    [Header("Rumble")]
    public float coolDownTimer;

    private void Awake()
    {
        shaker = playerCam.GetComponent<CameraShake>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rumbler = gameObject.GetComponent<RumbleManager>();
        mov = gameObject.GetComponent<WizardMovement>();
        currentCD = 0;
        currentTauntCD = 20;
        bulletCount = GameObject.Find("ModifierManager").GetComponent<GameModifiers>().GetBulletHell();

        //shotMat.SetColor("_EmissionColor", new Color(0.522f, 0.392f, 0.69f));

        //red = shotMat.GetColor("_EmissionColor").r;
        //blue = shotMat.GetColor("_EmissionColor").b;
        //green = shotMat.GetColor("_EmissionColor").g;

        hp = gameObject.GetComponent<PlayerHealth>();

        //FMOD Ranged Player Shoot and Teleport SFX
        ShootSFXInstance = FMODEngineManager.CreateSound(ShootSFX, 0.75f);
        //TauntSFXInstance = FMODEngineManager.CreateSound(TauntSFX, 0.8f);
        if(!PlayerManager.Instance.IsOnline())
        {
            reticle = Instantiate(reticle, reticlePos.position, reticlePos.rotation);
        }
        if(PlayerManager.Instance.IsOnline() && IsServer)
        {
            reticle = Instantiate(reticle, reticlePos.position, reticlePos.rotation);
        }
        distance = Vector3.Distance(transform.position, reticlePos.position);
    }

    private void FixedUpdate()
    {
        if(PlayerManager.Instance.IsOnline())
        {
            if (!IsOwner)
                return;
        }

        if(PlayerManager.Instance.IsOnline() && !IsServer)
        {
            reticle = gameObject.transform.GetChild(9).gameObject;
            reticle.SetActive(true);
        }

        if(shieldIcon == null)
        {
            shieldIcon = GetComponent<PlayerHealth>().getUI().ThingShield;
        }
        UpdateReticle();
        if (!PlayerManager.Instance.IsOnline())
        {
            if (currentCD > 0)
                currentCD -= 1;
            else
                canFire = true;
        }


        if (currentTauntCD > 0)
            currentTauntCD -= 1;
        else
            canTaunt = true;

        if (shieldCD > 0 && !shieldActive)
            shieldCD -= 1;
        if (shieldIcon != null)
        {
            shieldIcon.GetComponent<Image>().fillAmount = ((shieldMaxCD - shieldCD) / shieldMaxCD);
        }

        float colorMult = (bulletCD - (currentCD / 1.5f)) / bulletCD;
        //shotMat.SetColor("_EmissionColor", new Color(red * colorMult, green * colorMult, blue * colorMult));

        if(GetComponent<PlayerHealth>().getUI() != null)
            GetComponent<PlayerHealth>().getUI().ThingLure.GetComponent<Image>().fillAmount = ((tauntCooldown - currentTauntCD) / tauntCooldown);

        if(isShooting && hp.getAlive())
        {
            
            Shoot();
        }
    }

    private void UpdateReticle()
    {
        if(PlayerManager.Instance.IsOnline())
        {
            if (!IsOwner)
                return;
        }

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance, wallLayerMask) && !hit.transform.gameObject.GetComponent<PhaseDoor>())
        {
            reticle.transform.position = hit.point;
        }
        else
        {
            reticle.transform.position = reticlePos.position;
        }
        reticle.transform.rotation = gameObject.transform.rotation;
    }

    //Input detection function, should probably be split into a seperate shoot function 
    

    void Shoot()
    {
        if (!PlayerManager.Instance.IsOnline() && currentCD <= 0)
        {
            var rot = bulletSpawn.transform.rotation;
            rot = Quaternion.Euler(0, rot.eulerAngles.y, rot.eulerAngles.z);

            //rotation for multishot; 30 degrees for every 2nd bullet
            if (bulletCount > 1)
            {
                float rotNum = (Mathf.Floor(bulletCount / 2) * 2) - 1;
                rot = Quaternion.Euler(0, rot.eulerAngles.y - (rotNum * 15), rot.eulerAngles.z);
            }

            for (int i = 0; i < bulletCount; i++)
            {
                Instantiate(bullet, bulletSpawn.transform.position, rot);

                //update rotation for bullet hell mode
                rot = Quaternion.Euler(0, rot.eulerAngles.y + 15, rot.eulerAngles.z);

                if (bulletCount % 2 == 0 && i == (bulletCount / 2) + 1)
                {
                    rot = Quaternion.Euler(0, rot.eulerAngles.y + 15, rot.eulerAngles.z);
                }
            }

            currentCD = bulletCD;
            if(hp.isCheating() == false)
                canFire = false;
            Instantiate(shootEffect, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
            //currentCD = coolDownTimer;
            ShootSFXInstance.start();
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Ranged_Player/Shoot/Shoot", this.gameObject);
            rumbler.Rumble(leftSideRumble, rightSideRumble, rumbleTimer);
            //mov.Knockback(shotIntensity, -bulletSpawn.transform.forward);
        }
        else if (PlayerManager.Instance.IsOnline() && canFire)
        {
            if (hp.isCheating() == false)
                canFire = false;
            if (!IsServer)
            {
                SpawnBulletServerRpc();
            }
            if (IsServer)
            {
                if (!IsOwner)
                    return;
                SpawnBulletClientRpc();
            }
            StartCoroutine(ResetFire());
        }
    }

    IEnumerator ResetFire()
    {
        yield return new WaitForSeconds(.5f);
        canFire = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBulletServerRpc()
    {
        GameObject temp = Instantiate(bullet, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        temp.GetComponent<NetworkObject>().Spawn();
        GameObject particle = Instantiate(shootEffect, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        particle.GetComponent<NetworkObject>().Spawn();
    }

    [ClientRpc]
    private void SpawnBulletClientRpc()
    {
        GameObject temp = Instantiate(bullet, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        temp.GetComponent<NetworkObject>().Spawn();
        GameObject particle = Instantiate(shootEffect, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        particle.GetComponent<NetworkObject>().Spawn();
    }

    //Detects if the lure button is pressed should also pronanly be split into a seperate lure function 
    public void OnTaunt(InputAction.CallbackContext ctx)
    {
        if (PlayerManager.Instance.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        if (currentTauntCD > 0 || GetComponent<PlayerHealth>().paused || !canTaunt)
        {
            return;
        }

        if (scriptObj == null && hp.getAlive() && GetComponent<PlayerHealth>().canMove && ctx.started)
        {
            //if (GetComponent<PlayerHealth>().getUI().thingAbility != null)
               //Destroy(GetComponent<PlayerHealth>().getUI().thingAbility);
            if (!PlayerManager.Instance.IsOnline())
            {
                float dist = Vector3.Distance(transform.position, reticlePos.position) / 2;

                scriptObj = Instantiate(tauntObj, tauntSpawn.transform.position, new Quaternion(tauntSpawn.transform.rotation.x, tauntSpawn.transform.rotation.y + 180, tauntSpawn.transform.rotation.z, tauntSpawn.transform.rotation.w));
                scriptObj.GetComponent<Lure>().dist = dist;
                Invoke(nameof(DeleteTaunt), tauntObjLife);
                currentTauntCD = tauntCooldown;
                if (hp.isCheating() == false)
                    canTaunt = false;
            }
            else
            {
                if (!IsServer)
                {
                    SpawnLureServerRpc();
                }
                if (IsServer)
                {
                    if (!IsOwner)
                        return;
                    SpawnLureClientRpc();
                }
                currentTauntCD = tauntCooldown;
                if (hp.isCheating() == false)
                    canTaunt = false;
            }
        }
    }

    void activateShield()
    {
        Fist = GetComponent<PlayerHealth>().getTeamatePos();
        GameObject newShield = Instantiate(shieldObject, Fist.position, Fist.rotation);
        shieldActive = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnShieldServerRpc()
    {
        Fist = FindObjectOfType<BigGuyMovement>().gameObject.transform;
        GameObject newShield = Instantiate(shieldObject, Fist.position, Fist.rotation);
        newShield.GetComponent<NetworkObject>().Spawn();
        shieldActive = true;
    }

    [ClientRpc]
    private void SpawnShieldClientRpc()
    {
        Fist = FindObjectOfType<BigGuyMovement>().gameObject.transform;
        GameObject newShield = Instantiate(shieldObject, Fist.position, Fist.rotation);
        newShield.GetComponent<NetworkObject>().Spawn();
        shieldActive = true;
    }

    public void resetShield()
    {
        if (hp.isCheating() == false)
            shieldCD = shieldMaxCD;
        shieldActive = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnLureServerRpc()
    {
        float dist = Vector3.Distance(transform.position, reticlePos.position) / 2;
        scriptObj = Instantiate(tauntObj, tauntSpawn.transform.position, new Quaternion(tauntSpawn.transform.rotation.x, tauntSpawn.transform.rotation.y + 180, tauntSpawn.transform.rotation.z, tauntSpawn.transform.rotation.w));
        scriptObj.GetComponent<NetworkObject>().Spawn();
        scriptObj.GetComponent<Lure>().dist = dist;
        Invoke(nameof(DeleteTaunt), tauntObjLife);
    }

    [ClientRpc]
    private void SpawnLureClientRpc()
    {
        float dist = Vector3.Distance(transform.position, reticlePos.position) / 2;
        scriptObj = Instantiate(tauntObj, tauntSpawn.transform.position, new Quaternion(tauntSpawn.transform.rotation.x, tauntSpawn.transform.rotation.y + 180, tauntSpawn.transform.rotation.z, tauntSpawn.transform.rotation.w));
        scriptObj.GetComponent<NetworkObject>().Spawn();
        scriptObj.GetComponent<Lure>().dist = dist;
        Invoke(nameof(DeleteTaunt), tauntObjLife);
    }

    IEnumerator ResetTaunt()
    {
        yield return new WaitForSeconds(tauntCooldown);
        canTaunt = true;
    }

    void DeleteTaunt()
    {
        if (PlayerManager.Instance.IsOnline())
        {
            if (!IsServer)
            {
                DeleteLureServerRpc();
            }
            if (IsServer)
            {
                if (!IsOwner)
                    return;
                DeleteLureClientRpc();
            }
        }
        Destroy(scriptObj);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeleteLureServerRpc()
    {
        scriptObj.GetComponent<NetworkObject>().Despawn();
        Destroy(scriptObj);
    }

    [ClientRpc]
    private void DeleteLureClientRpc()
    {
        scriptObj.GetComponent<NetworkObject>().Despawn();
        Destroy(scriptObj);
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (PlayerManager.Instance.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        if (GetComponent<PlayerHealth>().paused)
        {
            return;
        }

        if (hp.getAlive() && gameObject.GetComponent<PlayerHealth>().canMove && ctx.performed)
        {
            isShooting = true;
        }
        if (!hp.getAlive() || !gameObject.GetComponent<PlayerHealth>().canMove || ctx.canceled)
        {
            isShooting = false;
        }

    }

    public void onShield(InputAction.CallbackContext ctx)
    {
        if (shieldCD > 0 || GetComponent<PlayerHealth>().paused || shieldActive)
        {
            return;
        }
        if (hp.getAlive() && GetComponent<PlayerHealth>().canMove && ctx.started && shieldCD <= 0 && !shieldActive)
        {
            shieldIcon.GetComponent<Image>().fillAmount = 0;
            if(!PlayerManager.Instance.IsOnline())
                activateShield();
            if(PlayerManager.Instance.IsOnline())
            {
                if (!IsServer)
                {
                    SpawnShieldServerRpc();
                }
                if (IsServer)
                {
                    if (!IsOwner)
                        return;
                    SpawnShieldClientRpc();
                }
            }
        }
    }
}
