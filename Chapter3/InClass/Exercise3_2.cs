using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exercise3_2 : MonoBehaviour
{
    public GameObject Cannonball;

    public float shotForce = 100f;
    public float torque = 10f;
    public float reloadTime = 5f;
   
    float timer;
    void Start()
    {
        timer = reloadTime;
    }

    void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }

        else
        {
            timer = reloadTime;
            FireCannonball();
        }
    }

    void FireCannonball()
    {
        GameObject tempBall = Instantiate(Cannonball, this.transform);
        tempBall.GetComponent<Rigidbody>().AddForce((Vector3.up + Vector3.forward) * shotForce, ForceMode.Impulse);
        tempBall.GetComponent<Rigidbody>().AddTorque(Vector3.right * torque, ForceMode.Impulse);
    }
}
