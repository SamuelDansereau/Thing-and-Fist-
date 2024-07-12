using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CauldronAnimation : MonoBehaviour
{
    [SerializeField]
    private GameObject model;
    [SerializeField]
    float walkSeconds;
    private Vector3 normal, left, right, normalScale, startShoot, endShoot;
    bool isLeft = true, isRight = false, walkDone = true;
    bool moving;
    // Start is called before the first frame update
    void Start()
    {
        normal = model.transform.rotation.eulerAngles;
        normalScale = model.transform.localScale;
        startShoot = new Vector3(1.15f, .6f, 1.15f);
        endShoot = new Vector3(.5f, 2f, .5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<EnemyNavMesh>().moving)
        {
            startWalk();
        }
        normal.y = transform.rotation.eulerAngles.y - 90;
        normal.z = 0;
        left = new Vector3(normal.x + 20, normal.y, normal.z);
        right = new Vector3(normal.x - 20, normal.y, normal.z);
    }

    public IEnumerator walk(float seconds)
    {
        walkDone = false;
        yield return new WaitForSeconds(seconds);
        if (isLeft)
        {
            model.transform.eulerAngles = right;
            isRight = true;
            isLeft = false;
            walkDone = true;
        }
        else if (isRight)
        {
            model.transform.eulerAngles = left;
            isRight = false;
            isLeft = true;
            walkDone = true;
        }
    }

    public void startWalk()
    {
        if (!walkDone)
        {
            return;
        }
        else
        {
            StartCoroutine(walk(walkSeconds));
        }
    }

    public IEnumerator chargeShot(float seconds)
    {
        GetComponent<EnemyFire>().chargingShot = true;
        model.transform.localScale = Vector3.Lerp(model.transform.localScale, startShoot, Time.deltaTime * 5f);
        yield return new WaitForSeconds(seconds);  
        GetComponent<EnemyFire>().chargingShot = false;
    }

    public IEnumerator shootShot(float seconds)
    {
        model.transform.localScale = Vector3.Lerp(model.transform.localScale, endShoot, Time.deltaTime * 7f);
        yield return new WaitForSeconds(seconds);
        model.transform.localScale = normalScale;
    }

}
