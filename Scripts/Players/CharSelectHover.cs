using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharSelectHover : MonoBehaviour
{
    [SerializeField]
    public GameObject sitting, standing, other1, other2;

    [Header("SFX")]
    [SerializeField] FMODUnity.EventReference HoverSoundSFX;
    private FMOD.Studio.EventInstance HoverSoundSFXInstance;

    public GameObject parent;

    void start()
    {
        HoverSoundSFXInstance = FMODEngineManager.CreateSound(HoverSoundSFX, 0.7f);
    }

    public void hovered()
    {
        if (!parent.GetComponent<PlayerSelectHolder>().ready && standing.activeSelf == false && other2.activeSelf == false)
        {
            sitting.SetActive(true);
            //HoverSoundSFXInstance.start();
            FMODUnity.RuntimeManager.PlayOneShotAttached("event:/UI/CharSelect/CharSelected", this.gameObject);
        }
    }

    public void unHovered()
    {
        sitting.SetActive(false);
    }

    public void selected()
    {
        standing.SetActive(true);
        sitting.SetActive(false);
        other1.SetActive(false);
        other2.SetActive(false);
    }

}
