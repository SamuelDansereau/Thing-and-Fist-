using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffectScript : MonoBehaviour
{
    public float effectFade;
    public GameObject hitVol;
    public GameObject player;
    public bool isFading;



    // Start is called before the first frame update
    void Awake()
    {
        isFading = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (effectFade > 0)
        {
            effectFade = effectFade - Time.deltaTime;
            if (isFading)
            {
                hitVol.transform.position += new Vector3(0f, 0.1f, 0f);
            }
        }
        else if (effectFade <= 0)
        {
            Deactivate();

        }
    }

    public void EffectStart()
    {
        hitVol.SetActive(true);
        hitVol.transform.position = player.transform.position;
        isFading = true;
        effectFade = 3.0f;
    }

    public void EffectEnd(float endTimer)
    {
        effectFade = endTimer;
        isFading = true;
    }

    public void Deactivate()
    {
        hitVol.SetActive(false);
        isFading = false;
    }
}
