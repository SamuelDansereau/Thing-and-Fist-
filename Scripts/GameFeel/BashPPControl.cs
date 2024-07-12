using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BashPPControl : MonoBehaviour
{
    public float effectFade;
    public GameObject bashVol;
    public GameObject bigGuy;
    public bool isFading;
    /*public Volume bashVolume;
    public Vignette bV;
    public MotionBlur bMB;
    public ChromaticAberration bCA;
    public Bloom bB;
    public LensDistortion bLD;*/



    // Start is called before the first frame update
    void Awake()
    {
        /*Volume bashVolume = bashVol.GetComponent<Volume>();
        bashVolume.profile.TryGet(out bV);
        bashVolume.profile.TryGet(out bMB);
        bashVolume.profile.TryGet(out bCA);
        bashVolume.profile.TryGet(out bB);
        bashVolume.profile.TryGet(out bLD);*/
        isFading = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void FixedUpdate()
    {

        if (effectFade > 0)
        {
            effectFade = effectFade - Time.deltaTime;
            if (isFading)
            {
                bashVol.transform.position += new Vector3(0f, 0.2f, 0f);
            }
        }
        else if (effectFade >= 0)
        {
            Deactivate();

        }
    }

    public void EffectStart()
    {
        bashVol.transform.position = bigGuy.transform.position;
    }

    public void EffectEnd(float endTimer)
    {
        effectFade = endTimer;
        isFading = true;
    }

    public void Deactivate()
    {
        bashVol.SetActive(false);
        isFading = false;
    }
}
