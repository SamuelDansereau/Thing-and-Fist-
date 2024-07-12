using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigGuyAnimation : MonoBehaviour
{
    bool punching;

    [Header("Punching")]
    public GameObject punchStartLeft;
    public GameObject punchStartRight;
    public GameObject punchEndLeft;
    public GameObject punchEndRight;
    public GameObject punchDefLeft;
    public GameObject punchDefRight;

    [Header("SFX")]
    [SerializeField] FMODUnity.EventReference WalkLoopSFX;
    private FMOD.Studio.EventInstance WalkLoopSFXInstance;
    
    [Header("GroundPound")]
    public GameObject gpChargeBody;
    public GameObject gpChargeLeft, gpChargeRight;
    public GameObject gpSlamBody, gpSlamLeft, gpSlamRight;


    void Start()
    {
        WalkLoopSFXInstance = FMODEngineManager.CreateSound(WalkLoopSFX, 0.25f);

    }

    private void Update()
    {
        punching = gameObject.GetComponent<BigGuyCombat>().isPunching();
    }

    public void setRun(int run)
    {
        if (run == 0) 
        { 
            if(!punching)
            {
                punchStartLeft.SetActive(false);
                punchStartRight.SetActive(false);
                punchEndLeft.SetActive(false);
                punchEndRight.SetActive(false);
            }
            gpChargeLeft.SetActive(false);
            gpChargeRight.SetActive(false);
            gpSlamLeft.SetActive(false);
            gpSlamRight.SetActive(false);
        }
        else if(run == 1)
        {
            if (!punching)
            {
                punchStartLeft.SetActive(false);
                punchStartRight.SetActive(false);
                punchEndLeft.SetActive(false);
                punchEndRight.SetActive(false);
            }
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Melee_Player/Walk/WalkLoop", this.gameObject);
            gpChargeLeft.SetActive(false);
            gpChargeRight.SetActive(false);
            gpSlamLeft.SetActive(false);
            gpSlamRight.SetActive(false);
            
        }
        else if (run == 2)
        {
            if (!punching)
            {
                punchStartLeft.SetActive(false);
                punchStartRight.SetActive(false);
                punchEndLeft.SetActive(false);
                punchEndRight.SetActive(false);
            }
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Melee_Player/Walk/WalkLoop", this.gameObject);
            gpChargeLeft.SetActive(false);
            gpChargeRight.SetActive(false);
            gpSlamLeft.SetActive(false);
            gpSlamRight.SetActive(false);
            
        }
    }

    public void setSlam(int gp)
    {
        if (gp == 1)
        {
            gpChargeLeft.SetActive(true);
            gpChargeRight.SetActive(true);
            gpSlamLeft.SetActive(false);
            gpSlamRight.SetActive(false);
            punchStartLeft.SetActive(false);
            punchStartRight.SetActive(false);
            punchEndLeft.SetActive(false);
            punchEndRight.SetActive(false);
        }
        else if (gp == 2)
        {
            gpChargeLeft.SetActive(false);
            gpChargeRight.SetActive(false);
            gpSlamLeft.SetActive(true);
            gpSlamRight.SetActive(true);
            punchStartLeft.SetActive(false);
            punchStartRight.SetActive(false);
            punchEndLeft.SetActive(false);
            punchEndRight.SetActive(false);
        }
        else
        {
            gpChargeLeft.SetActive(false);
            gpChargeRight.SetActive(false);
            gpSlamLeft.SetActive(false);
            gpSlamRight.SetActive(false);
        }
    }

    public void setPunch(int punch)
    {
        if (punch == 1)
        {
            gpChargeLeft.SetActive(false);
            gpChargeRight.SetActive(false);
            gpSlamLeft.SetActive(false);
            gpSlamRight.SetActive(false);
            punchStartLeft.SetActive(true);
            punchStartRight.SetActive(false);
            punchEndLeft.SetActive(false);
            punchEndRight.SetActive(false);
        }
        else if (punch == 2)
        {
            gpChargeLeft.SetActive(false);
            gpChargeRight.SetActive(false);
            gpSlamLeft.SetActive(false);
            gpSlamRight.SetActive(false);
            punchStartLeft.SetActive(false);
            punchStartRight.SetActive(false);
            punchEndLeft.SetActive(true);
            punchEndRight.SetActive(false);
        }
        else if (punch == 3)
        {
            gpChargeLeft.SetActive(false);
            gpChargeRight.SetActive(false);
            gpSlamLeft.SetActive(false);
            gpSlamRight.SetActive(false);
            punchStartLeft.SetActive(false);
            punchStartRight.SetActive(true);
            punchEndLeft.SetActive(false);
            punchEndRight.SetActive(false);
        } 
        else if (punch == 4) 
        {
            gpChargeLeft.SetActive(false);
            gpChargeRight.SetActive(false);
            gpSlamLeft.SetActive(false);
            gpSlamRight.SetActive(false);
            punchStartLeft.SetActive(false);
            punchStartRight.SetActive(false);
            punchEndLeft.SetActive(false);
            punchEndRight.SetActive(true);
        }
        else if (punch == 5)
        {
            punchStartLeft.SetActive(false);
            punchStartRight.SetActive(false);
            punchEndLeft.SetActive(false);
            punchEndRight.SetActive(false);
        }
    }
}
