using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightpost : MonoBehaviour
{
    bool used = false;
    [SerializeField]
    GameObject off, on, explosion;
    public Transform effectPoint;
    [SerializeField]
    LayerMask enemyLayer;
    [SerializeField]
    float damage, radius, stunTime, explosionWaitTime;

    [Header("SFX")]
    [SerializeField] FMODUnity.EventReference ExplodeSFX;
    private FMOD.Studio.EventInstance ExplodeSFXInstance;


    // Start is called before the first frame update
    void Start()
    {
        ExplodeSFXInstance = FMODEngineManager.CreateSound(ExplodeSFX, 0.85f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shock()
    {
        if(!used)
        {
            
            used = true;
            GetComponent<LamppostScaler>().scaleExplosion(radius);
            Instantiate(explosion, effectPoint.position, gameObject.transform.rotation);
            ExplodeSFXInstance.start();
            Collider[] hitEnemies = Physics.OverlapSphere(gameObject.transform.position, radius, enemyLayer);
            foreach (Collider enemy in hitEnemies)
            {
                enemy.GetComponent<BasicEnemy>().Stun(stunTime);
                enemy.GetComponent<BasicEnemy>().doDamage(damage, transform.forward, 0, 0);
            }
            StartCoroutine(turnOff());
        }
    }

    IEnumerator turnOff()
    {
        yield return new WaitForSeconds(explosionWaitTime);
        off.SetActive(true);
        on.SetActive(false);

    }
}
