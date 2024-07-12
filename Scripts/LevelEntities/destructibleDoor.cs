using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destructibleDoor : MonoBehaviour
{
    [SerializeField]
    GameObject opened, closed, collapsed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Crash()
    {
        closed.SetActive(false);
        opened.SetActive(true);
        GetComponent<Collider>().enabled = false;
    }

    public void Collapse()
    {
        GameObject cantPass = Instantiate(collapsed, gameObject.transform.position, gameObject.transform.rotation);
        cantPass.transform.localScale = gameObject.transform.localScale;
        Destroy(gameObject);
    }
}
