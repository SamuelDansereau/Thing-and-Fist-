using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using Unity.Netcode;

public class BigGuyMovement : NetworkBehaviour
{
    bool cheats = true;
    public GameObject slimeCop;

    //Movement ==========================================================

    [Header("Movement")]
    Vector2 movementInput;
    private Rigidbody rb;
    [SerializeField]
    public float currentSpeed;
    private Vector3 knockbackEffect;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float maxDownedSpeed;
    [SerializeField]
    private float acceleration;
    [SerializeField]
    private float decceleration;
    [SerializeField]
    private float initialPush;
    public bool moving = false;
    private Vector3 moveDir;
    [SerializeField]
    private float knockbackDecayRate = 1.3f;
    [SerializeField]
    public float bashKnockback;
    public GameObject stunPP;
    public float chargeStun;

    private int stumbling = 0;
    [SerializeField]
    private int stumbleTime = 25;
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

    // ==================================================================

    //Bash ==============================================================

    [Header("Bash")]
    public Material bash;
    public Material bashready;
    GameObject cam;
    public Vector3 cameraPosition;
    public Material normal;
    public Material stun;
    public GameObject bashObject;
    public float bashSpeed, bashForce;
    public bool bashing = false;
    public float stunTime;
    public bool stunned = false;
    public int bashRange;
    public float bashDamage;
    public float bashRotateMod;
    public LayerMask enemiesLayer;
    public GameObject bashPP;
    public float fadePP;

    public int bashIFrames;
    int bashCurrentIFrames;

    public float bashTimer;
    float currentBashTimer = 0;

    //mat changes
    public Material piMat;
    Color col;
    GameObject icon;


    // ==================================================================

    //Sound =============================================================

    [Header("SFX")]
    //FMOD Melee Player Charging SFX
    [SerializeField] FMODUnity.EventReference ChargeStartSFX;
    private FMOD.Studio.EventInstance ChargeStartSFXInstance;
    [SerializeField] FMODUnity.EventReference ChargeLoopSFX;
    private FMOD.Studio.EventInstance ChargeLoopSFXInstance;
    [SerializeField] FMODUnity.EventReference ChargeStopSFX;
    private FMOD.Studio.EventInstance ChargeStopSFXInstance;

    // ==================================================================

    //Input Manager =====================================================

    private Controls controls;

    // ==================================================================

    //Animation =====================================================

    int anim, next, last;
    public float cycleCooldown;
    float cycleMax;
    Animator animator;
    public GameObject model;


    // ==================================================================

    //Controller Rumble ==========================================
    [Header("Rumble")]
    public float chargeRumbleTimer;
    [Header("Still Rumble, These can only be between 0.0 - 1.0")]
    public float chargeLeftRumble; public float chargeRightRumble;
    RumbleManager rumbler;

    //============================================================

    //Camera Lerp ===============================================
    [Header("Camera Lerp")]
    private BigGuyCam camScript;
    //============================================================

    //Camera Shake ===============================================
    [Header("Camera Shake")]
    public Camera playerCam;
    [SerializeField]
    float stunShakeMagnitude;
    [SerializeField]
    float stunShakeTime;
    [SerializeField]
    float chargeHitShakeMagnitude;
    [SerializeField]
    float chargeHitShakeTime;
    CameraShake shaker;

    //============================================================

    [Header(" ")]
    bool slamming;
    PlayerInputManager manager;

    PlayerManager pm;

    private void Awake()
    {
        rumbler = gameObject.GetComponent<RumbleManager>();
        controls = new Controls();
        manager = FindObjectOfType<PlayerInputManager>();

        piMat.SetColor("_EmissionColor", new Color(0.7490196f, 0.5139548f, 0.3466207f));

        col.r = piMat.GetColor("_EmissionColor").r;
        col.g = piMat.GetColor("_EmissionColor").g;
        col.b = piMat.GetColor("_EmissionColor").b;
        bashPP.SetActive(false);

        camScript = playerCam.GetComponent<BigGuyCam>();
        shaker = playerCam.GetComponent<CameraShake>();
        fadePP = 0.5f;
    }

    private void OnEnable()
    {
        controls.Enable();
    }
    private void OnDisable()
    {
        controls.Disable();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        moveDir = Vector3.zero;

        //FMOD Melee Player Charging SFX
        ChargeStartSFXInstance = FMODEngineManager.CreateSound(ChargeStartSFX, 0.6f);
        ChargeLoopSFXInstance = FMODEngineManager.CreateSound(ChargeLoopSFX, 0.6f);
        ChargeStopSFXInstance = FMODEngineManager.CreateSound(ChargeStopSFX, 0.7f);

        bashObject.transform.localScale = new Vector3(bashRange, 1, bashRange);

        cam = GameObject.Find("Main Camera");
        //cameraPosition = GetComponent<Camera>().transform.position;
        cycleMax = cycleCooldown;
        next = 1;
        last = 0;
        animator = model.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(moving)
        {
            currentSpeed = maxSpeed;
        }
        if(!GetComponent<PlayerHealth>().getAlive())
        {
            moving = false;
            currentSpeed = 0;
        }

        if (pm == null)
        {
            pm = FindAnyObjectByType<PlayerManager>();
        }
        if (pm != null)
        {
            if (pm.IsOnline())
            {
                if (!IsOwner)
                    return;
            }
        }
        if(icon == null && GetComponent<PlayerHealth>().getUI())
        {
            icon = GetComponent<PlayerHealth>().getFistCharge();
        }


        slamming = gameObject.GetComponent<BigGuyCombat>().isSlamming();

        if (!bashing)
        {
            ChargeLoopSFXInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        if (slamming)
        {
            moving = false;
            bashing = false;
            currentSpeed = 0;
            rb.velocity = moveDir * currentSpeed;
            anim = 4;
            gameObject.GetComponent<BigGuyAnimation>().setRun(anim);
            bashObject.SetActive(false);
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (moving && !bashing && stumbling == 0)
        {
            moveDir = new Vector3(movementInput.x, 0, movementInput.y);
        }

        if(moving)
        {
            if (currentBashTimer < bashTimer)
            {
                currentBashTimer += Time.deltaTime;
            }
            if (OnSlope())
            {
                rb.AddForce(SlopeMoveDirection() * currentSpeed * 20f, ForceMode.Force);
            }
        }

        if(!moving && currentBashTimer > 0)
        {
            currentBashTimer -= Time.deltaTime;
        }
        icon.GetComponent<Image>().fillAmount = currentBashTimer / bashTimer;

        if (stunned)
        {
            anim = 0;
            currentBashTimer = 0;
            //piMat.SetColor("_EmissionColor", new Color(255, 0, 0));
            icon.GetComponent<Image>().fillAmount = 0;
            currentSpeed = 0;
        }
        else if (bashing || slamming)
            piMat.SetColor("_EmissionColor", col);
        else if (currentSpeed >= bashSpeed)
            piMat.SetColor("_EmissionColor", col);
        else if (currentSpeed > 0)
        {
            float colorMult = currentSpeed / bashSpeed;
            
            piMat.SetColor("_EmissionColor", new Color(0, 0, 0));
        }


        if (!moving && !slamming)
            anim = 0;
        if (!moving)
        {
            piMat.SetColor("_EmissionColor", col * 0.25f);
            animator.SetBool("Moving", false);
        }
        if (bashing && !moving)
        {
            bashObject.SetActive(false);
            bashing = false;
            camScript.LerpCameraSwitch(false);
        }
        if (currentSpeed <= 0)
        {
            currentSpeed = 0;
        }
        if(moving)
        {
            animator.SetBool("Moving", true);
        }
        rb.useGravity = !OnSlope();
    }

    private void FixedUpdate()
    {
        if (pm != null)
        {
            if (pm.IsOnline())
            {
                if (!IsOwner)
                    return;
            }
        }
        if (!slamming)
        {
            Melee();
            if (cycleCooldown > 0)
            {
                cycleCooldown--;
            }
            gameObject.GetComponent<BigGuyAnimation>().setRun(anim);
        }
        if (slamming && bashObject.activeSelf == true)
        {
            bashObject.SetActive(false);
        }

        if (stumbling > 0)
        {
            stumbling--;
        }
        if (bashCurrentIFrames > 0)
        {
            bashCurrentIFrames--;
        }
    }

    //This is the essentially the move function it is where the overarching movement stuff happens 
    private void Melee()
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        if (!stunned)
        {
            if (moving && GetComponent<PlayerHealth>().canMove)
            {
                MeleeMovement();
                MeleeRotation();
                Bash();
                if (cycleCooldown == 0)
                {
                    cycleCooldown = cycleMax;
                    runCycle();
                }

            }
            else if (stumbling == 0)
            {
                if (currentSpeed > 0)
                    currentSpeed -= decceleration * (Time.deltaTime * 2);
            }

            rb.velocity = (moveDir * currentSpeed) + knockbackEffect;
            if (knockbackEffect.magnitude > 0.01f)
            {
                knockbackEffect /= knockbackDecayRate;
            }
            else { knockbackEffect = Vector3.zero; }
        }
    }
    public void Knockback(float intensity, Vector3 dir)
    {
        knockbackEffect = intensity * dir;
    }

    //Animation stuff will be phased out 
    private void runCycle()
    {
        if (next == 1 && last == 0)
        {
            anim = 1;
            next = 2;
            last = 1;
            if (bashing)
            {
                rumbler.Rumble(chargeLeftRumble, 0f, chargeRumbleTimer);
            }
        }
        if (next == 2 && last == 0)
        {
            anim = 2;
            next = 1;
            last = 2;
            if (bashing)
            {
                rumbler.Rumble(0f, chargeRightRumble, chargeRumbleTimer);
            }
        }
        else if (next == 2 && last == 1)
        {
            anim = 2;
            next = 1;
            last = 2;
            if (bashing)
            {
                rumbler.Rumble(0f, chargeRightRumble, chargeRumbleTimer);
            }
        }
        else if (next == 1 && last == 2)
        {
            anim = 1;
            next = 2;
            last = 1;
            if (bashing)
            {
                rumbler.Rumble(chargeLeftRumble, 0f, chargeRumbleTimer);
            }
        }
        else
        {
            anim = 0;
            last = 0;
        }
    }

    //Default movement 
    private void MeleeMovement()
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        if (currentSpeed <= 0 && !bashing)
        {
            currentSpeed = maxSpeed;
        }
        else if (currentSpeed < maxSpeed && rb.velocity.magnitude > 1 && !bashing)
        {
            currentSpeed = maxSpeed;
            //currentSpeed += acceleration * Time.deltaTime;
            //piMat.color = new Color((piMat.color.r + Brightest.r/2), (piMat.color.g + Brightest.g/2), (piMat.color.g + Brightest.g/2));
        }
    }
    private void MeleeRotation()
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        Vector3 playerDir = Vector3.right * moveDir.x + Vector3.forward * moveDir.z;
        if (playerDir.sqrMagnitude > 0)
        {
            Quaternion newRot = Quaternion.LookRotation(playerDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, newRot, Time.deltaTime * 1000);
        }
    }

    //This is the charge 
    private void Bash()
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        //Vector3 newCameraPosition = new Vector3(GetComponent<Camera>().transform.position.x, GetComponent<Camera>().transform.position.y, 100.0f);

        if (currentSpeed > bashSpeed)
        {
            if (bashing)
            {
                bashCurrentIFrames = bashIFrames;

                bashObject.SetActive(true);
                currentSpeed += acceleration * Time.deltaTime;
                //if (GetComponent<PlayerHealth>().getAlive())
                //{
                //    if (currentSpeed > maxSpeed)
                //        currentSpeed = maxSpeed;
                //}
                //else
                //{
                //    if(currentSpeed > maxDownedSpeed)
                //        currentSpeed = maxDownedSpeed;
                //}
                //GetComponent<Camera>().transform.position = newCameraPosition;
            }
            else
            {
                currentSpeed = bashSpeed;
                bashPP.GetComponent<BashPPControl>().EffectEnd(fadePP);
            }
                

        }

        else
        {
            bashObject.SetActive(false);
            bashPP.GetComponent<BashPPControl>().EffectEnd(fadePP);
            bashing = false;
            camScript.LerpCameraSwitch(false);
            //GetComponent<Camera>().transform.position = cameraPosition;

            // Keeps playing after death
            ChargeLoopSFXInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        if (bashing)
        {
            moveDir = Vector3.Slerp(moveDir, new Vector3(movementInput.x, 0, movementInput.y), Time.deltaTime * bashRotateMod);
            bashPP.SetActive(true);
            bashPP.GetComponent<BashPPControl>().EffectStart();
            camScript.LerpCameraSwitch(true);
        }
    }

    // Collision on Charge
    private void OnCollisionEnter(Collision col)
    {
        

        //if (col.gameObject.tag == "Enemy" && col.gameObject.GetComponent<BasicEnemy>() && bashing)
        //{
        //    //col.gameObject.GetComponent<Rigidbody>().velocity = -(gameObject.transform.position - col.gameObject.transform.position) * 20;
        //    col.gameObject.GetComponent<BasicEnemy>().doDamage(2, transform.forward, bashKnockback);
        //    //col.gameObject.GetComponent<BasicEnemy>().Stun(chargeStun);
        //    //StartCoroutine(shaker.shaking(stunShakeTime, stunShakeMagnitude));
        //}

        //if (col.gameObject.tag == "Enemy" && col.gameObject.GetComponent<TutorialEnemies>() && bashing)
        //{
        //    //col.gameObject.GetComponent<Rigidbody>().velocity = -(gameObject.transform.position - col.gameObject.transform.position) * 20;
        //    col.gameObject.GetComponent<BasicEnemy>().doDamage(2, transform.forward, bashKnockback);
        //    //col.gameObject.GetComponent<BasicEnemy>().Stun(chargeStun);
        //    StartCoroutine(shaker.shaking(stunShakeTime, stunShakeMagnitude));

        //}
    }

    public bool getChargeInv()
    {
        return (bashCurrentIFrames > 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7 && bashing)
        {
            bashPP.GetComponent<BashPPControl>().EffectEnd(fadePP);
            ChargeLoopSFXInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            //I'm gonna comment this out for now because Fist gets stunned by everything it most likely won't work until we have actual objects in 
            //Stun();
        }

        if (other.gameObject.tag == "Enemy" && other.gameObject.GetComponent<BasicEnemy>() && bashing)
        {
            //col.gameObject.GetComponent<Rigidbody>().velocity = -(gameObject.transform.position - col.gameObject.transform.position) * 20;
            other.gameObject.GetComponent<BasicEnemy>().doDamage(bashDamage, transform.forward, bashDamage, 0);
            //col.gameObject.GetComponent<BasicEnemy>().Stun(chargeStun);
            StartCoroutine(shaker.shaking(stunShakeTime, stunShakeMagnitude));
        }

        if (other.gameObject.tag == "Enemy" && other.gameObject.GetComponent<TutorialEnemies>() && bashing)
        {
            //col.gameObject.GetComponent<Rigidbody>().velocity = -(gameObject.transform.position - col.gameObject.transform.position) * 20;
            other.gameObject.GetComponent<BasicEnemy>().doDamage(bashDamage, transform.forward, bashKnockback, 0);
            //col.gameObject.GetComponent<BasicEnemy>().Stun(chargeStun);
            StartCoroutine(shaker.shaking(stunShakeTime, stunShakeMagnitude));

        }

        //smash dem gaetss
        if (other.gameObject.tag == "Wall" && other.gameObject.GetComponent<Door>() && bashing)
        {
            other.gameObject.GetComponent<Door>().AddFistHit();
        }


    }
    //This is the self stun when charginf into a wall 
    public void Stun()
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        rb.velocity = new Vector3(0, 0, 0);
        stunPP.SetActive(true);
        currentSpeed = 0;
        stunned = true;
        bashing = false;
        bashObject.SetActive(false);
        Invoke(nameof(ResetMovement), stunTime);
        ChargeStopSFXInstance.start();
        StartCoroutine(shaker.shaking(stunShakeTime, stunShakeMagnitude));
        camScript.LerpCameraSwitch(false);
    }

    void ResetMovement()
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        stunned = false;
        stunPP.SetActive(false);
    }

    // How the input for charge is called, I try to not put things in here but rather in there apporirate function but it can't be avoided sometimes
    public void OnCharge(InputAction.CallbackContext ctx)
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        if (ctx.started && moving && currentBashTimer >= bashTimer)
        {
            if (!bashing)
            {
                ChargeStartSFXInstance.start();
                ChargeLoopSFXInstance.start();
            }

            bashing = true;

            currentSpeed = maxSpeed;
        }
    }

    //Same thing but mvoement input 
    public void OnMove(InputAction.CallbackContext ctx)
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        movementInput = ctx.ReadValue<Vector2>();
        if (GetComponent<PlayerHealth>().canMove)
        {
            moving = true;
        }
        if (ctx.canceled)
        {
            moving = false;
        }
    }

    public bool GetIfCharging()
    {
        return bashing;
    }
    public void Stumble()
    {
        stumbling = stumbleTime;
    }

    //Checks of the player is on a slope
    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, .5f + .3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    Vector3 SlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }

    void OnDestroy()
    {
        ChargeLoopSFXInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void OnDoorPuzzleInteract(InputAction.CallbackContext ctx)
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }

        if(GetComponent<PlayerHealth>().canMove)
        {
            if((GetComponent<PlayerHealth>().isHacking))
            {
                GetComponent<PlayerHealth>().StartHack.Raise(1);
            }
        }

        if(!GetComponent<PlayerHealth>().canMove)
        {
            if (!(GetComponent<PlayerHealth>().isHacking))
            {
                GetComponent<PlayerHealth>().StopHack.Raise(1);
            }
        }
    }
}