using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shadowHandAnimation : MonoBehaviour
{
    [SerializeField]
    private GameObject baseHand, attackCharge, attackSwing1, attackSwing2;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


    }

    public void startAttack()
    {
        baseHand.SetActive(false);
        attackCharge.SetActive(true);
    }
    public void startSwing()
    {
        baseHand.SetActive(false);
        attackCharge.SetActive(false);
        attackSwing1.SetActive(true);
    }
    public void followThrough()
    {
        baseHand.SetActive(false);
        attackSwing1.SetActive(false);
        attackCharge.SetActive(false);
        attackSwing2.SetActive(true);
    }
    public void endAttack()
    {
        attackSwing1.SetActive(false);
        attackSwing2.SetActive(false);
        attackCharge.SetActive(false);
        baseHand.SetActive(true);
    }
}
