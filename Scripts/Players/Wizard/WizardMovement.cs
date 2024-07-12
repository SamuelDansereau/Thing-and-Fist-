using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.VisualScripting;

public class WizardMovement : NetworkBehaviour
{
    bool cheats = true;
    public GameObject slimeCop;

    //Movement ==============================================================
    [Header("Movement")]
    Vector2 movementInput;
    Vector2 rotInput;
    Vector2 aim;
    private Rigidbody rb;
    [SerializeField]
    private float currentSpeed;
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
    private Controls controls;
    private Vector3 moveDir;
    [SerializeField]
    private float knockbackDecayRate;
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    //=======================================================================


    //Teleport ==============================================================
    [Header("Teleport")]
    public float tpDist;
    public GameObject scorch;
    bool teleport;
    public float tpSpeed;
    GameObject currentShadow;
    public int tpCooldown,dashIFrames;
    int currentTPCD, currentDashIFrames = 0;
    public float dashStart, dashEnd;
    float dashStartMax, dashEndMax;
    bool dashing;
    public float dashExplodeDMG, dashLineDMG, dashExplodRadius;
    public LayerMask enemiesLayer;
    Vector3 reticlePos;
    GameObject zoom;
    public float dashSpeedIncrease, postDashDecay;
    [Header("How Quickly you move along in the dash")]
    public float dashSpeedLine;
    private float dist;

    //=======================================================================

    bool grounded = true;

    //Animation =============================================================
    [Header("Animations")]

    public GameObject model;
    Animator animator;
    public float cycleCooldown;
    float maxCycle;
    public GameObject dashBolt, dashStrike;
    bool firstPass = true;
    GameObject icon;
    //=======================================================================

    // SFX ============================================================
    [Header("SFX")]
    [SerializeField] FMODUnity.EventReference WalkLoopSFX;
    private FMOD.Studio.EventInstance WalkLoopSFXInstance;
    [SerializeField] FMODUnity.EventReference TeleportSFX;
    private FMOD.Studio.EventInstance TeleportSFXInstance;
    //=======================================================================

    //Controller Rumble ==========================================
    [Header("Rumble")]
    public float dashStartTime;
    public float dashFinishTime;
    [Header("Still Rumble, These can only be between 0.0 - 1.0")]
    public float startDashPower; public float endDashPower;
    RumbleManager rumbler;
    //============================================================

    //Camera Shake ===============================================
    [Header("Camera Shake")]
    public Camera playerCam;
    [SerializeField]
    float blinkShakeMagnitude;
    [SerializeField]
    float blinkShakeTime;
    CameraShake shaker;
    //============================================================

    //Camera Lerp ===============================================
    [Header("Camera Lerp")]
    private PlayerCam camScript;
    //============================================================

    PlayerHealth hp;
    PlayerInputManager manager;
    WizardCombat combat;

    PlayerManager pm;
    private void Awake()
    {
        shaker = playerCam.GetComponent<CameraShake>();
        rumbler = gameObject.GetComponent<RumbleManager>();
        controls = new Controls();
        manager = FindObjectOfType<PlayerInputManager>();
        hp = gameObject.GetComponent<PlayerHealth>();
        combat = gameObject.GetComponent<WizardCombat>();
        camScript = playerCam.GetComponent<PlayerCam>();
        zoom = new GameObject();
        animator = model.GetComponent<Animator>();
    }

    private void OnEnable()
    {
        controls.Enable();
    }
    private void OnDisable()
    {
        controls.Disable();
    }

    private void Start()
    {
        reticlePos = GetComponent<WizardCombat>().reticle.transform.position;
        rb = GetComponent<Rigidbody>();
        moveDir = Vector3.zero;
        maxCycle = cycleCooldown;
        WalkLoopSFXInstance = FMODEngineManager.CreateSound(WalkLoopSFX, 0.25f);
        TeleportSFXInstance = FMODEngineManager.CreateSound(TeleportSFX, 0.8f);
        dashEndMax = dashEnd;
        dashEnd = 0;
        dashStartMax = dashStart;
    }
    private void Update()
    {
        if(pm == null)
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
            icon = GetComponent<PlayerHealth>().getUI().ThingBlink;
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (moving)
        {
            moveDir = new Vector3(movementInput.x, 0, movementInput.y);
            animator.SetBool("Moving", true);
            if(OnSlope())
            {
                rb.AddForce(SlopeMoveDirection() * currentSpeed * 20f, ForceMode.Force);
            }
        }
        if(!moving)
        {
            animator.SetBool("Moving", false);
        }

        rb.useGravity = !OnSlope();
    }
    private void FixedUpdate()
    {
        if(pm != null)
        {
            if (pm.IsOnline())
            {
                if (!IsOwner)
                    return;
            }
        }
        if (currentTPCD > 0)
            currentTPCD -= 1;

        if (currentDashIFrames > 0)
            currentDashIFrames -= 1;

        if (currentSpeed < 0.1f)
            currentSpeed = 0;

        if(cycleCooldown > 0)
            cycleCooldown -= 1;
        
        if(teleport)
        {
            icon.GetComponent<Image>().fillAmount = 0;

            if (dashStart > 0)
            {
                dashStart --;
            }
            if (dashEnd > 0)
            {
                dashEnd --;
            }
            Dash();
        }

        else
        {
            if(icon != null)
                icon.GetComponent<Image>().fillAmount = ((tpCooldown - (float)currentTPCD) / tpCooldown);
        }

        Ranged();
    }

    //General movement updates function
    private void Ranged()
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        if (moving &&GetComponent<PlayerHealth>().canMove) 
        { 
            RangedMovement(); 
        }
        else if (currentSpeed > 0)
                currentSpeed -= decceleration * (Time.deltaTime * 2);

        rb.velocity = (moveDir * currentSpeed) + knockbackEffect;
        if (knockbackEffect.magnitude > 0.01f)
        {
            knockbackEffect /= knockbackDecayRate;
        }
        else { knockbackEffect = Vector3.zero; }
    }

    public void Knockback(float intensity, Vector3 dir)
    {
        knockbackEffect = intensity * dir;
    }


    private void RangedMovement()
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        WalkCycle();
        if (currentSpeed <= 0)
        {
            currentSpeed += initialPush;
        }
        else
        {
            if (hp.getAlive())
            {
                if (currentSpeed < maxSpeed)
                    currentSpeed += acceleration * Time.deltaTime;
                if (currentSpeed > maxSpeed)
                    currentSpeed -= decceleration * (Time.deltaTime) * postDashDecay;
            }
            else
            {
                if (currentSpeed < maxDownedSpeed)
                    currentSpeed += acceleration * Time.deltaTime;
                if (currentSpeed > maxDownedSpeed)
                    currentSpeed -= decceleration * (Time.deltaTime) * postDashDecay;
            }
        }
    }

    //Animation stuff don't worry about it will be changed
    private void WalkCycle()
    {
        if (cycleCooldown == 0)
        {
            cycleCooldown = maxCycle;
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Ranged_Player/Walk/WalkLoop", this.gameObject);
        }
        else if (cycleCooldown == 0)
        {

            cycleCooldown = maxCycle;
        }
        else if (cycleCooldown == 0)
        {
            cycleCooldown = maxCycle;
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Ranged_Player/Walk/WalkLoop", this.gameObject);
        }
        else if (cycleCooldown == 0)
        {
            cycleCooldown = maxCycle;
        }
    }

    //Player rotation 
    private void RangedRotation()
    {
        /*GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closestdist = float.MaxValue;
        Transform target = null;*/
        aim = rotInput;
        if (Mathf.Abs(aim.x) > .1 || Mathf.Abs(aim.y) > .1)
        {
            Vector3 playerDir = Vector3.right * aim.x + Vector3.forward * aim.y;
            if (playerDir.sqrMagnitude > 0)
            {
                Quaternion newRot = Quaternion.LookRotation(playerDir, Vector3.up);
                transform.rotation = newRot;

                
                /*for (int i = 0; i < allEnemies.Length; i++)
                {
                    if (Vector3.Distance(allEnemies[i].transform.position, transform.position) < closestdist)
                    {
                        closestdist = Vector3.Distance(allEnemies[i].transform.position, transform.position);
                        target = allEnemies[i].transform;
                    }
                }
                if (target != null)
                {
                    Vector2 direction = (target.position - transform.position);
                    Quaternion targetRotation = Quaternion.Euler(direction);
                    transform.rotation = targetRotation;
                }*/
            }
        }
    }

    
    //Weird since this is both, a movement and conmbat ability. If you need to change anything that has to do with damage, it is in the dash damage script on the dash prefab
    public void Dash()
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        if (dashStart <= 0)
        {
            currentDashIFrames = dashIFrames;

            gameObject.layer = LayerMask.NameToLayer("PhasingWizard");
            if (firstPass)
            {
                firstPass = false;
                if (hp.isCheating() == false)
                    currentTPCD = tpCooldown;
                dashBolt.SetActive(false);
                Quaternion zoomRot = new Quaternion(moveDir.x, moveDir.y, moveDir.z, gameObject.transform.rotation.w);
                zoom = Instantiate(dashStrike, transform.position, zoomRot);
                dashEnd = dashEndMax;
                rumbler.Rumble(0f, endDashPower, dashFinishTime);
                reticlePos = GetComponent<WizardCombat>().reticle.transform.position;
                dist = Vector3.Distance(transform.position, reticlePos);
                GameObject newStrike = Instantiate(scorch, new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z), transform.rotation);
                camScript.LerpCameraSwitch(true);

            }
            if (grounded)
            {
                
                transform.position = Vector3.Lerp(transform.position, reticlePos, Time.deltaTime * dashSpeedLine);

            }
            if (dashEnd <= 0)
            {
                dashBolt.SetActive(false);
                GetComponent<PlayerHealth>().canMove = true;
                teleport = false;
                gameObject.layer = LayerMask.NameToLayer("Player");
                if (hp.isCheating() == false)
                    dashStart = dashStartMax;
                if (hp.isCheating() == false)
                    currentTPCD = tpCooldown;
                firstPass = true;
                if(moving)
                    currentSpeed = maxSpeed + dashSpeedIncrease;
                Destroy(zoom);
            }
        }

    }

    public bool getDashInv()
    {
        return currentDashIFrames > 0;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public float getDashDamage()
    {
        return dashLineDMG;
    }

    //Function that detects input 
    public void OnRotate(InputAction.CallbackContext ctx)
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        rotInput = ctx.ReadValue<Vector2>();
        if (hp.getAlive() && GetComponent<PlayerHealth>().canMove)
        {
            RangedRotation();
        }
        if (ctx.canceled)
        {
        }
    }
    //function that Detects movement input 
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
    //Function that detects teleport button input 
    public void OnTeleport(InputAction.CallbackContext ctx)
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
        if (ctx.started && currentTPCD == 0 && hp.getAlive() && GetComponent<PlayerHealth>().canMove)
        {
            TeleportSFXInstance.start();
            teleport = true;
            dashBolt.SetActive(true);
            rumbler.Rumble(startDashPower, 0f, dashStartTime);
            GetComponent<PlayerHealth>().canMove = false;
            if (hp.isCheating() == false)
                currentTPCD = tpCooldown;
            StartCoroutine(shaker.shaking(blinkShakeTime, blinkShakeMagnitude));

            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, dashExplodRadius, enemiesLayer);
            foreach (Collider enemy in hitEnemies)
            {
                enemy.GetComponent<BasicEnemy>().doDamage(dashExplodeDMG, Vector3.zero, 0, 1);
            }
        }
    }

    //Checks of the player is on a slope
    bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, .5f + .3f))
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

    public void OnDoorPuzzleInteract(InputAction.CallbackContext ctx)
    {
        if (pm != null && pm.IsOnline())
        {
            if (!IsOwner)
                return;
        }
    }
}
