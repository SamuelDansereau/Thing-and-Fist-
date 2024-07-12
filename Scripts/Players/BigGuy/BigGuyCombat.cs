using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using Unity.VisualScripting;
using Unity.Netcode;

public class BigGuyCombat : NetworkBehaviour
{
    [SerializeField]
    private LayerMask enemiesLayer;
    [SerializeField]
    private LayerMask bulletLayer;
    [SerializeField]
    private LayerMask levelLayer;
    [SerializeField]
    private Transform attackPoint;

    //Punch ======================================================
    [Header("Punch")]
    [SerializeField]
    private float punchRadius;
    [SerializeField]
    private float punchCooldown;
    [SerializeField]
    private float punchKnockback;
    [SerializeField]
    private float punchKnockbackPlayer;
    float punchMaxCooldown;
    [SerializeField]
    private bool canAttack = true;
    [SerializeField]
    private float punchCharge, punchPause;
    float punchMaxCharge, punchMaxPause;
    [SerializeField]
    private bool punching = false;
    [SerializeField]
    private int punchDamage;
    bool firstPassPunch = true;
    char next = 'r';
    public float stunTime;
    [Header("HeavyPunch")]
    bool isHeavyPunch, punchReady;
    private int heavyBuildup;
    [SerializeField]
    private float heavyPunchCharge;
    [SerializeField]
    private float heavyPunchRadius;
    [SerializeField]
    private int heavyPunchDamage;
    [SerializeField]
    private float heavyPunchKnockback;
    [SerializeField]
    private bool noHeavy = true; // disables heavy punch
    public NetworkVariable<bool> networkPunching = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    //GroundPound ================================================

    [Header("Ground Pound")]
    [SerializeField]
    private float gpCooldown;
    float gpMaxCooldown;
    [SerializeField]
    private int gpRadius, gpDamage;
    [SerializeField]
    private float gpCharge, gpPause, gpCrackDis;
    float gpMaxCharge, gpMaxPause, gpMaxCrackDis;
    [SerializeField]
    private bool slamming = false;
    [SerializeField]
    private Transform gpLocation;
    [SerializeField]
    private GameObject slam;
    bool firstPassCharge = true;
    [SerializeField]
    private GameObject crack;

    
    
    GameObject icon;

    //============================================================

    //Block ======================================================

    [Header("Block")]
    [SerializeField]
    private float blockCooldown;
    private float blockMaxCooldown;
    [SerializeField]
    private GameObject wall;
    [SerializeField]
    private Transform wallSpawn;
    GameObject wallIcon;


    //============================================================

    //Controller Rumble ==========================================
    [Header("Rumble")]
    [SerializeField]
    private float punchRumbleTimer;
    [SerializeField]
    private float gpRumbleTimer;
    [Header("Still Rumble, These can only be between 0.0 - 1.0")]
    [SerializeField]
    private float punchRumbleLeft; 
    [SerializeField]
    private float punchRumbleRight;
    [SerializeField]
    private float gpRumbleLeft; 
    [SerializeField]
    private float gpRumbleRight;
    RumbleManager rumbler;

    //============================================================
    
    //Animation ==================================================
    int chargeAnim = 4;
    int punchAnim = 4;
    Animator animator;
    bool leftPunch = true;
    bool rightPunch = false;

    public GameObject model;

    public GameObject rightPunchPrefab, leftPunchPrefab;
    Vector3 rightFistRot = new Vector3(0, 0, -180);

    //============================================================

    //Camera Shake ===============================================
    [Header("Camera Shake")]
    [SerializeField]
    private Camera playerCam;
    [SerializeField]
    float gpShakeMagnitude;
    [SerializeField]
    float punchShakeMagnitude;
    [SerializeField]
    float gpShakeTime;
    [SerializeField]
    float punchShakeTime;
    CameraShake shaker;

    [Header("Camera Hitstop")]
    Camera_Hitstop stopper;
    [SerializeField]
    float freezeDuration;

    //============================================================

    //SFX
    [Header("SFX")]
    [SerializeField] FMODUnity.EventReference PunchEnemySFX;
    private FMOD.Studio.EventInstance PunchEnemySFXInstance;
    [SerializeField] FMODUnity.EventReference PunchWallSFX;
    private FMOD.Studio.EventInstance PunchWallSFXInstance;
    [SerializeField] FMODUnity.EventReference PunchAirSFX;
    private FMOD.Studio.EventInstance PunchAirSFXInstance;
    [SerializeField] FMODUnity.EventReference SlamRaiseSFX;
    private FMOD.Studio.EventInstance SlamRaiseSFXInstance;
    [SerializeField] FMODUnity.EventReference SlamHitSFX;
    private FMOD.Studio.EventInstance SlamHitSFXInstance;
    [SerializeField] FMODUnity.EventReference ShieldSpawnSFX;
    private FMOD.Studio.EventInstance ShieldSpawnSFXInstance;
    [SerializeField] FMODUnity.EventReference TruckSFX;
    private FMOD.Studio.EventInstance TruckSFXInstance;

    BigGuyMovement mov;

    PlayerManager pm;


    void Start()
    {
        shaker = playerCam.GetComponent<CameraShake>();
        stopper = playerCam.GetComponent<Camera_Hitstop>();
        animator = model.GetComponent<Animator>();
        rumbler = gameObject.GetComponent<RumbleManager>();
        mov = gameObject.GetComponent<BigGuyMovement>();

        //SFX
        PunchEnemySFXInstance = FMODEngineManager.CreateSound(PunchEnemySFX, 0.7f);
        PunchWallSFXInstance = FMODEngineManager.CreateSound(PunchWallSFX, 0.6f);
        PunchAirSFXInstance = FMODEngineManager.CreateSound(PunchAirSFX, 0.7f);
        SlamRaiseSFXInstance = FMODEngineManager.CreateSound(SlamRaiseSFX, 0.7f);
        SlamHitSFXInstance = FMODEngineManager.CreateSound(SlamHitSFX, 0.9f);
        ShieldSpawnSFXInstance = FMODEngineManager.CreateSound(ShieldSpawnSFX, 0.7f);
        TruckSFXInstance = FMODEngineManager.CreateSound(TruckSFX, 0.75f);

        gpMaxCooldown = gpCooldown;
        gpMaxCharge = gpCharge;
        gpMaxPause = gpPause;
        gpMaxCrackDis = gpCrackDis;
        gpCooldown = 0;
        gpPause = 0;
        gpCrackDis = 0;

        punchMaxCooldown = punchCooldown;
        punchMaxCharge = punchCharge;
        punchMaxPause = punchPause;
        punchPause = 0;

        punchMaxCooldown = punchCooldown;
        punchCooldown = 0;

        blockMaxCooldown = blockCooldown;
        blockCooldown = 0;

        slam.transform.localScale = new Vector3(2, 2, 2);
    }

    void FixedUpdate()
    {
        if (PlayerManager.Instance.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        if(icon == null)
        {
            icon = GetComponent<PlayerHealth>().getFistGround();
        }
        if (wallIcon == null)
        {
            wallIcon = GetComponent<PlayerHealth>().getUI().FistWall;
        }
        // Need to change these to coroutines when I have time theres so many makeshift ones 
        if (gpCooldown > 0) 
        {
            gpCooldown--; 
        }
        else if (gpCooldown < 0) 
        { 
            gpCooldown = 0; 
        }

        if (punchCooldown > 0) { punchCooldown--; }
        else if (punchCooldown < 0) { punchCooldown = 0; }
        else { canAttack = true; }

        if (blockCooldown > 0) { blockCooldown--; }
        else if (blockCooldown < 0) { blockCooldown = 0; }
        if (wallIcon != null)
        {
            wallIcon.GetComponent<Image>().fillAmount = ((blockMaxCooldown - blockCooldown) / blockMaxCooldown);
        }

        networkPunching.Value = punching;

        if (slamming)
        {
            icon.GetComponent<Image>().fillAmount = 0;

            if (gpCharge > 0)
            {
                gpCharge--;
            }
            if (gpCrackDis > 0)
            {
                gpCrackDis--;
            }
            if (gpPause > 0)
            {
                gpPause--;
            }
            groundPound();
        }
        if (punching)
        {
            icon.GetComponent<Image>().fillAmount = 0;
            
            if (punchCharge > 0)
            {
                punchCharge--;
            }
            if (punchPause > 0)
            {
                punchPause--;
            }

            //only charge heavy while building up strength
            if (punchPause == 0) { heavyBuildup++; }
            //attacks w both heavy and non
            isHeavyPunch = (heavyBuildup > heavyPunchCharge) && !noHeavy;

            Attack(isHeavyPunch);
        }

        else
        {
            icon.GetComponent<Image>().fillAmount = ((gpMaxCooldown - gpCooldown) / gpMaxCooldown);
        }


    }

    //This function looks so long because of my fucky animations so don't raellu worry about it, I'll highlight where important stuff happens 
    public void Attack(bool heavy)
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        if (!GetComponent<BigGuyMovement>().bashing && punchCharge <= 0 && (noHeavy || punchReady))
        {
            if (firstPassPunch)
            {
                firstPassPunch = false;

                //variable swaps if heavy
                float rad;
                rad = punchRadius;

                int dmg;
                dmg = punchDamage; 

                float kbk;
                kbk = punchKnockback; 

                //This is the punch right here
                Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, rad, enemiesLayer);
                foreach (Collider enemy in hitEnemies)
                {
                    //enemy.GetComponent<Rigidbody>().velocity *= -.080f;
                    if(enemy.GetComponent<BasicEnemy>())
                        enemy.GetComponent<BasicEnemy>().doDamage(dmg, transform.forward, kbk, 2);
                }
                Collider[] hitEntities = Physics.OverlapSphere(attackPoint.position, rad, levelLayer);
                foreach (Collider entity in hitEntities)
                {
                    if (entity.GetComponent<PunchedTrigger>())
                        entity.GetComponent<PunchedTrigger>().getPunched(transform.position);
                    if (entity.GetComponent<PushTruck>())
                        entity.GetComponent<PushTruck>().setMove(true);
                        TruckSFXInstance.start();
                    if (entity.GetComponent<PushHeistTruck>())
                        entity.GetComponent<PushHeistTruck>().setMove(true);
                        TruckSFXInstance.start();
                }

                //if (hitEnemies.Length > 0)
                //{
                //    mov.Knockback(punchKnockbackPlayer, -transform.forward);
                //    StartCoroutine(shaker.shaking(punchShakeTime, punchShakeMagnitude));
                //}
                //else
                //{
                //    mov.Stumble();
                //}

                //smash dem booletss (not currently working) - The bulletLayer variable wasn't set to anything in Fist -Sam
                Collider[] hitBullets = Physics.OverlapSphere(attackPoint.position, rad, bulletLayer);
                foreach (Collider bullet in hitBullets)
                {
                    Destroy(bullet.gameObject);
                }

                //smash dem gaetss
                Collider[] hitGates = Physics.OverlapSphere(attackPoint.position, rad, LayerMask.NameToLayer("Wall"));
                foreach (Collider gate in hitGates)
                {
                    if (gate.gameObject.GetComponent<PunchableDoor>() != null)
                    {
                        if (!PlayerManager.Instance.IsOnline())
                        {
                            PunchWallSFXInstance.start();
                            gate.gameObject.GetComponent<PunchableDoor>().DestroyWall();
                        }
                    }

                    if (gate.gameObject.GetComponent<Door>() != null)
                    {
                        gate.gameObject.GetComponent<Door>().AddFistHit();
                        break;
                    }
                }

                rumbler.Rumble(punchRumbleLeft, punchRumbleRight, punchRumbleTimer);
                animator.SetBool("Punching", true);
                punchPause = punchMaxPause;
                if (leftPunch)
                {
                    punchAnim = 2;
                    Instantiate(leftPunchPrefab, transform.position, gameObject.transform.rotation);
                    
                    PunchEnemySFXInstance.start();
                }

                else if (rightPunch)
                {
                    punchAnim = 4;
                    Instantiate(rightPunchPrefab, transform.position, Quaternion.AngleAxis(-90, Vector3.up) * gameObject.transform.rotation);
                    
                    PunchEnemySFXInstance.start();
                }
            }
            if (punchPause <= 0)
            {
                animator.SetBool("Punching", false);

                if (leftPunch)
                {
                    leftPunch = false;
                    next  = 'r';

                }
                else if (rightPunch)
                {
                    rightPunch = false;
                    next = 'l';
                }
                if(next == 'l')
                {
                    leftPunch = true;
                }
                else if (next == 'r')
                {
                    rightPunch = true;
                }
                punchAnim = 5;
                //animator.setPunch(chargeAnim);
                punching = false;
                punchCharge = punchMaxCharge;
                if (GetComponent<PlayerHealth>().isCheating() == false)
                    canAttack = false;
                punchCooldown = punchMaxCooldown;
                firstPassPunch = true;
            }
            if (GetComponent<PlayerHealth>().isCheating() == false)
                canAttack = false;
            punchCooldown = punchMaxCooldown;
        }
    }

    //Actual groundpound function
    public void groundPound()
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        if (gpCharge <= 0)
        {
            if (firstPassCharge)
            {
                chargeAnim = 2;
                
                firstPassCharge = false;
                gpPause = gpMaxPause;
                slam.SetActive(true);
                gpCrackDis = gpMaxCrackDis;
                rumbler.Rumble(gpRumbleLeft, gpRumbleRight, gpRumbleTimer);
                SlamHitSFXInstance.start();
                //Maybe rotate at random 
                GameObject newCrack = Instantiate(crack, new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), gameObject.transform.rotation);
                StartCoroutine(shaker.shaking(gpShakeTime, gpShakeMagnitude));
                Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, gpRadius, enemiesLayer);
                foreach (Collider enemy in hitEnemies)
                {
                    enemy.GetComponent<BasicEnemy>().doDamage(gpDamage, transform.forward, 0, 0);
                    enemy.GetComponent<BasicEnemy>().Stun(stunTime);
                }

                //smash dem gaetss
                Collider[] hitGates = Physics.OverlapSphere(attackPoint.position, punchRadius, LayerMask.NameToLayer("Wall"));
                foreach (Collider gate in hitGates)
                {
                    if (gate.gameObject.GetComponent<Door>() != null)
                    {
                        gate.gameObject.GetComponent<Door>().AddFistHit();
                        break;
                    }
                }
            }
            if (gpCrackDis <= 0)
            {
                slam.SetActive(false);
                slam.SetActive(false);
                animator.SetBool("GroundPound", false);

            }

            if (gpPause <= 0)
            {
                chargeAnim = 4;
                animator.SetBool("GroundPound", false);
                gameObject.GetComponent<BigGuyAnimation>().setSlam(chargeAnim);
                slamming = false;
                gpCharge = gpMaxCharge;
                if(GetComponent<PlayerHealth>().isCheating() == false)
                    gpCooldown = gpMaxCooldown;
                firstPassCharge = true;
            }

        }

    }

    private void Block()
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
        wallIcon.GetComponent<Image>().fillAmount = 0;
        if (!PlayerManager.Instance.IsOnline())
        {
            GameObject newWall = Instantiate(wall, wallSpawn.position, gameObject.transform.rotation);
        }
        if(PlayerManager.Instance.IsOnline())
        {
            if (!IsServer)
            {
                WallSpawnServerRpc();
            }
            if (IsServer)
            {
                if (!IsOwner)
                    return;
                WallSpawnClientRpc();
            }
        }
        ShieldSpawnSFXInstance.start();
        blockCooldown = blockMaxCooldown;
    }

    [ServerRpc(RequireOwnership = false)]
    private void WallSpawnServerRpc()
    {
        GameObject newWall = Instantiate(wall, wallSpawn.position, gameObject.transform.rotation);
        newWall.GetComponent<NetworkObject>().Spawn();
    }

    [ClientRpc]
    private void WallSpawnClientRpc()
    {
        GameObject newWall = Instantiate(wall, wallSpawn.position, gameObject.transform.rotation);
        newWall.GetComponent<NetworkObject>().Spawn();
    }

    //This is what detects input 
    public void onAttack(InputAction.CallbackContext ctx)
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        if (GetComponent<PlayerHealth>().getAlive() && GetComponent<PlayerHealth>().canMove && canAttack && punchCooldown == 0)
        {
            if (ctx.started)
            {
                heavyBuildup = 0;
                punchReady = false;
                punching = true;
                if (leftPunch)
                {
                    punchAnim = 1;
                }
                else if (rightPunch)
                {
                    punchAnim = 3;
                }
                //animator.setPunch(punchAnim);
            }
            
            else if (ctx.canceled)
            {
                punchReady = true;
            }
        }
    }

    //This detects input 
    public void onGroundpound(InputAction.CallbackContext ctx)
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        if (!GetComponent<PlayerHealth>().paused)
        {
            if (ctx.started && GetComponent<PlayerHealth>().getAlive() && gpCooldown == 0 && firstPassCharge && GetComponent<PlayerHealth>().canMove)
            {
                slamming = true;
                chargeAnim = 1;
                //animator.setSlam(chargeAnim);
                animator.SetBool("GroundPound", true);
                SlamRaiseSFXInstance.start();

            }
        }
    }

    public void onBlock(InputAction.CallbackContext ctx)
    {
        /*if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }*/
        if (ctx.started && GetComponent<PlayerHealth>().getAlive() && blockCooldown == 0)
        {
            Block();
        }  
    }


    public bool isPunching()
    {
        return punching;
    }

    public bool isSlamming()
    {
        return slamming;
    }

   
    
}

