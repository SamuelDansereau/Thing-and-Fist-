//After about 45 minutes of searching I found this tutorial on how to do this https://www.youtube.com/watch?v=dIC4wbUgt5M 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeObjectBlockingObject : MonoBehaviour
{
    [SerializeField]
    private LayerMask layerMask;
    [SerializeField]
    private Transform Player;
    [SerializeField]
    private Camera Camera;
    [SerializeField]
    private float fadedAlpha = 0.33f;
    [SerializeField]
    private FadeMode fadingMode;

    [SerializeField]
    private float ChecksPerSecond = 10;
    [SerializeField]
    private int fadeFPS = 30;
    [SerializeField]
    private int fadeSpeed = 2;

    [Header("Read Only Data")]
    [SerializeField]
    private List<FadingObject> ObjectsBlockingView = new List<FadingObject>();
    private Dictionary<FadingObject, Coroutine> RunningCoroutines = new Dictionary<FadingObject, Coroutine>();

    private RaycastHit[] hits = new RaycastHit[10];

    private void Start()
    {
        StartCoroutine(CheckForObjects());
    }

    private IEnumerator CheckForObjects()
    {
        WaitForSeconds time = new WaitForSeconds(1f / ChecksPerSecond);
        while(true)
        {
            if (Physics.RaycastNonAlloc(Camera.transform.position, (Player.transform.position - Camera.transform.position).normalized, hits, Vector3.Distance(Camera.transform.position, Player.transform.position), layerMask) >  0)
            {
                for (int i = 0; i < hits.Length; i++) 
                { 
                    FadingObject fadingObject = GetFadingObjectsFromHit(hits[i]);
                    if (fadingObject != null && !ObjectsBlockingView.Contains(fadingObject))
                    {
                        if(RunningCoroutines.ContainsKey(fadingObject))
                        {
                            if (RunningCoroutines[fadingObject] != null)
                            {
                                StopCoroutine(RunningCoroutines[fadingObject]);
                            }
                            RunningCoroutines.Remove(fadingObject);
                        }
                        RunningCoroutines.Add(fadingObject, StartCoroutine(FadeObjectOut(fadingObject)));
                        ObjectsBlockingView.Add(fadingObject);
                    }
                }
            }
            FadeObjectsNoLongerBeingHit();

            ClearHits();

            yield return time;
        }
    }

    private void FadeObjectsNoLongerBeingHit()
    {
        for (int i =0; i < ObjectsBlockingView.Count; i++) 
        { 
            bool objectIsBeingHit = false;
            for(int j = 0; j < hits.Length; j++)
            {
                FadingObject fadingObject = GetFadingObjectsFromHit(hits[j]);
                if(fadingObject != null && fadingObject == ObjectsBlockingView[i])
                {
                    objectIsBeingHit = true;
                    break;
                }
            }
            if (!objectIsBeingHit)
            {
                if (RunningCoroutines.ContainsKey(ObjectsBlockingView[i]))
                {
                    if (RunningCoroutines[ObjectsBlockingView[i]] != null)
                    {
                        StopCoroutine(RunningCoroutines[ObjectsBlockingView[i]]);
                    }
                    RunningCoroutines.Remove(ObjectsBlockingView[i]);
                }
                RunningCoroutines.Add(ObjectsBlockingView[i], StartCoroutine(FadeObjectIn(ObjectsBlockingView[i])));
                ObjectsBlockingView.RemoveAt(i);
            }
        }
    }

    private IEnumerator FadeObjectOut(FadingObject fadingObject) 
    {
        float waitTime = 1f / fadeFPS;
        WaitForSeconds time = new WaitForSeconds(waitTime);
        int ticks = 1;
        for(int i = 0; i < fadingObject.materials.Count; i++)
        {
            fadingObject.materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            fadingObject.materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            fadingObject.materials[i].SetInt("_zWrite", 0);
            if(fadingMode == FadeMode.Fade)
            {
                fadingObject.materials[i].EnableKeyword("_ALPHABLEND_ON");
            }
            else
            {
                fadingObject.materials[i].EnableKeyword("_ALPHAPREMULTIPLY_ON");
            }

            fadingObject.materials[i].renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        if (fadingObject.materials[0].HasProperty("_Color"))
        {
            while (fadingObject.materials[0].color.a > fadedAlpha)
            {
                for (int i = 0; i < fadingObject.materials.Count; i++)
                {
                    if (fadingObject.materials[i].HasProperty("_Color"))
                    {
                        fadingObject.materials[i].color = new Color(
                            fadingObject.materials[i].color.r,
                            fadingObject.materials[i].color.g,
                            fadingObject.materials[i].color.b,
                            Mathf.Lerp(fadingObject.InitialAlpha, fadedAlpha, waitTime * ticks * fadeSpeed)
                            );
                    }
                }
                ticks++;
                yield return time;
            }
        }
        if(RunningCoroutines.ContainsKey(fadingObject))
        {
            StopCoroutine(RunningCoroutines[fadingObject]);
            RunningCoroutines.Remove(fadingObject);
        }
    }

    

    private FadingObject GetFadingObjectsFromHit(RaycastHit hit)
    {
        return hit.collider != null ? hit.collider.GetComponent<FadingObject>() : null;
    }

    private IEnumerator FadeObjectIn( FadingObject fadingObject)
    {
        float waitTime = 1f / fadeFPS;
        WaitForSeconds time = new WaitForSeconds(waitTime);
        int ticks = 1;
        if (fadingObject.materials[0].HasProperty("_Color"))
        {
            while (fadingObject.materials[0].color.a < fadingObject.InitialAlpha)
            {
                for (int i = 0; i < fadingObject.materials.Count; i++)
                {
                    if (fadingObject.materials[i].HasProperty("_Color"))
                    {
                        fadingObject.materials[i].color = new Color(
                            fadingObject.materials[i].color.r,
                            fadingObject.materials[i].color.g,
                            fadingObject.materials[i].color.b,
                            Mathf.Lerp( fadedAlpha, fadingObject.InitialAlpha, waitTime * ticks * fadeSpeed)
                            );
                    }
                }
                ticks++;
                yield return time;
            }
        }

        for (int i = 0; i < fadingObject.materials.Count; i++)
        {
            
            if (fadingMode == FadeMode.Fade)
            {
                fadingObject.materials[i].DisableKeyword("_ALPHABLEND_ON");
            }
            else
            {
                fadingObject.materials[i].DisableKeyword("_ALPHAPREMULTIPLY_ON");
            }

            
            fadingObject.materials[i].SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            fadingObject.materials[i].SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            fadingObject.materials[i].SetInt("_zWrite", 1);
            fadingObject.materials[i].renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
        }
        if (RunningCoroutines.ContainsKey(fadingObject))
        {
            StopCoroutine(RunningCoroutines[fadingObject]);
            RunningCoroutines.Remove(fadingObject);
        }
    }

    private void ClearHits()
    {
        RaycastHit hit = new RaycastHit();
        for(int i = 0;i<hits.Length;i++)
        {
            hits[i] = hit;
        }
    }

    public enum FadeMode
    {
        Transparent,
        Fade
    }
}
