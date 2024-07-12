using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public bool shakeTriggered;
    void Update()
    {
        
    }

    public IEnumerator shaking(float duration, float strength)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;
        shakeTriggered = true;

        while (elapsed < duration) 
        { 
            elapsed += Time.deltaTime;
            startPos = transform.position;
            transform.position = startPos + Random.insideUnitSphere * strength;
            yield return null;
        }

        transform.position = startPos;
        shakeTriggered = false;
    }
}
