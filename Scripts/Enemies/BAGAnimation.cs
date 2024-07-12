using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.ProBuilder;

public class BAGAnimation : MonoBehaviour
{
    private Vector3 originalScale, scaleUp, scaleDown;
    [SerializeField]
    private float scale, duration;
    [SerializeField]
    GameObject model;
    private bool started = false, shrinkTime = false;
    private Vector3 rotationNormal, rotationShoot;
    bool rotated = false;

    // Start is called before the first frame update
    void Start()
    { 
        originalScale = model.transform.localScale;
        scaleUp = new Vector3(model.transform.localScale.x, 1.25f, model.transform.localScale.z);
        scaleDown = new Vector3(model.transform.localScale.x, 0.65f, model.transform.localScale.z);
        rotationNormal = model.transform.rotation.eulerAngles;
        rotationShoot = new Vector3(rotationNormal.x, 90, -20);
    }

    // Update is called once per frame
    void Update()
    {
        rotationNormal.y = transform.rotation.eulerAngles.y + 90;
        rotationNormal.z = 0;
        rotationShoot = new Vector3(rotationNormal.x, rotationNormal.y, -35);
        if (!started)
        {
            started = true;
            StartCoroutine(switchTime());
        }
        if(GetComponent<EnemyNavMesh>().moving)
        {
            if(shrinkTime)
            {
                shrink();
            }
            else if(!shrinkTime)
            {
                grow();
            }
        }
        
    }

    public void grow()
    {
        model.transform.localScale = Vector3.Lerp(model.transform.localScale, scaleUp, Time.deltaTime * 2);
    }

    public void shrink()
    {
        model.transform.localScale = Vector3.Lerp(model.transform.localScale, scaleDown, Time.deltaTime * 2);
    }

    public IEnumerator switchTime()
    {
        yield return new WaitForSeconds(duration);
        started = false;
        shrinkTime = !shrinkTime;
    }

    public IEnumerator Shooting(float shootDuration)
    {
        if (!rotated)
        {
            rotated = true;
            model.transform.eulerAngles = rotationShoot;
            yield return new WaitForSeconds(shootDuration);
            model.transform.eulerAngles = rotationNormal;
            rotated = false;
        }
    }

}
