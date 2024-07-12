using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScorch : MonoBehaviour
{
    
    public List<Renderer> renderers = new List<Renderer>();
    public Vector3 Position;
    public List<Material> materials = new List<Material>();
    float InitialAlpha;
    [SerializeField]
    private int fadeFPS = 30;
    [SerializeField]
    private int fadeSpeed = 2;
    [SerializeField]
    private FadeMode fadingMode;
    [SerializeField]
    private float upTime;
    private void Awake()
    {
        Position = transform.position;
        if (renderers.Count == 0)
        {
            renderers.AddRange(GetComponentsInChildren<Renderer>());
        }
        for (int i = 0; i < renderers.Count; i++)
        {
            materials.AddRange(renderers[i].materials);
        }

        InitialAlpha = materials[0].color.a;
    }

    // Update is called once per frame
    void Update()
    {
        if (upTime > 0)
        {
            upTime -= Time.deltaTime * 2;
        }
        else
        {
            StartCoroutine(FadeObjectOut());
        }
    }

    void despawn()
    {
        Destroy(gameObject);
    }

    private IEnumerator FadeObjectOut()
    {
        float waitTime = 1f / fadeFPS;
        WaitForSeconds time = new WaitForSeconds(waitTime);
        int ticks = 1;
        for (int i = 0; i < materials.Count; i++)
        {
            materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            materials[i].SetInt("_zWrite", 0);
            if (fadingMode == FadeMode.Fade)
            {
                materials[i].EnableKeyword("_ALPHABLEND_ON");
            }
            else
            {
                materials[i].EnableKeyword("_ALPHAPREMULTIPLY_ON");
            }

            materials[i].renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        if (materials[0].HasProperty("_Color"))
        {
            while (materials[0].color.a > 0)
            {
                for (int i = 0; i < materials.Count; i++)
                {
                    if (materials[i].HasProperty("_Color"))
                    {
                        materials[i].color = new Color(
                            materials[i].color.r,
                            materials[i].color.g,
                            materials[i].color.b,
                            Mathf.Lerp(InitialAlpha, 0, waitTime * ticks * fadeSpeed)
                            );
                    }
                }
                ticks++;
                yield return time;
            }
        }
        despawn();
    }

    public enum FadeMode
    {
        Transparent,
        Fade
    }
}
