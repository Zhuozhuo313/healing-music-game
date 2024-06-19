using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollidePlayer : MonoBehaviour
{
    // Start is called before the first frame update
    bool activate;
    private PlayerController playerController;
    void Start()
    {
        activate = false;
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController component missing from this game object.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // ShrinkthenDestroy();
    }
    void OnCollisionEnter(Collision collision)
    {
        // originTransform = transform;
        activate = true;
        print("ss");
        // ShrinkthenDestroy();
        
        collision.gameObject.GetComponent<BoxCollider>().enabled = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        if (playerController != null)
        {
            playerController.StartSlowDown();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        // transform.Translate(originTransform.position, Space.World);
    }

    void ShrinkthenDestroy()
    {
        while (activate) 
        {
            activate = false;
            for (int i = 0; i < 20; i++)
            {
                string name = "Fragment" + i;
                GameObject fragment = GameObject.Find(name);
                if (fragment != null)
                {
                    print("Find" + i);
                    activate = true;
                }
            }
        }
    }
}
