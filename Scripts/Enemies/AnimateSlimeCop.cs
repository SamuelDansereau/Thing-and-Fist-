using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AnimateSlimeCop : MonoBehaviour
{
    [SerializeField]
    private GameObject happy, angry;

    [Header("SFX")]
    [SerializeField] FMODUnity.EventReference MeleeSFX;
    private FMOD.Studio.EventInstance MeleeSFXInstance;

    private Vector3 originalScale, scaleTo;
    [SerializeField]
    private float scale, duration;
    private Sequence walk;
    bool started = false;

    // Start is called before the first frame update
    void Start()
    {
        MeleeSFXInstance = FMODEngineManager.CreateSound(MeleeSFX, .35f);
        originalScale = transform.localScale;
        walk = DOTween.Sequence();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Anger()
    {
        angry.SetActive(true);
        happy.SetActive(false);
        FMODUnity.RuntimeManager.PlayOneShotAttached("event:/Enemy/SlimeCop/Melee/MeleeHit",this.gameObject);
    }

    public void Joyous()
    {
        angry.SetActive(false);
        happy.SetActive(true);
    }

    public void MovementLoop()
    {
        if (!started)
        {
            scaleTo = originalScale * scale;
            walk.Append(transform.DOScale(scaleTo, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo));
            started = true;
        }
    }

    public void endMovement()
    {
        walk.Kill();
        started = false;    
        transform.localScale = originalScale;
    }
}
