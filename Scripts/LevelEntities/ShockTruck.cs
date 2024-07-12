using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class ShockTruck : MonoBehaviour
{
    bool used = false;
    [SerializeField]
    GameObject explosion;
    [SerializeField]
    LayerMask enemyLayer;
    [SerializeField]
    float damage, radius, stunTime, explosionWaitTime, truckCooldown;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Shock()
    {
        if (!used)
        {

            used = true;
            explosion.SetActive(true);
            Collider[] hitEnemies = Physics.OverlapSphere(gameObject.transform.position, radius, enemyLayer);
            foreach (Collider enemy in hitEnemies)
            {
                enemy.GetComponent<BasicEnemy>().Stun(stunTime);
                enemy.GetComponent<BasicEnemy>().doDamage(damage, transform.forward, 0, 0);
            }
            StartCoroutine(turnOff());
            StartCoroutine(shockCooldown());
        }
    }

    IEnumerator turnOff()
    {
        yield return new WaitForSeconds(explosionWaitTime);
        explosion.SetActive(false);

    }

    IEnumerator shockCooldown()
    {
        yield return new WaitForSeconds(truckCooldown);
        explosion.SetActive(false);
        used = false;

    }
}
