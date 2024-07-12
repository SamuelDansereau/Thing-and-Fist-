using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class ChargeRadius : MonoBehaviour
{
    public float damage;
    public float knockback;
    public GameObject Fist;
    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Enemy" && col.GetComponent<BasicEnemy>())
        {
            col.gameObject.GetComponent<Rigidbody>().velocity = -(gameObject.transform.position - col.gameObject.transform.position) * 20;
            col.gameObject.GetComponent<BasicEnemy>().doDamage(damage, Vector3.forward, knockback, 0);
        }

        if (col.gameObject.tag == "Enemy" && col.GetComponent<TutorialEnemies>())
        {
            col.gameObject.GetComponent<Rigidbody>().velocity = -(gameObject.transform.position - col.gameObject.transform.position) * 20;
            col.gameObject.GetComponent<TutorialEnemies>().doDamage(damage);
        }
        if (col.gameObject.tag == "Wall" || col.gameObject.tag == "PushTruck")
        {
            Fist.GetComponent<BigGuyMovement>().bashPP.GetComponent<BashPPControl>().EffectEnd(Fist.GetComponent<BigGuyMovement>().fadePP);
            Fist.GetComponent<BigGuyMovement>().Stun();
        }
    }
}
