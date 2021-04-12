using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exercise6_2 : MonoBehaviour
{
    public GameObject vehicle;
    public GameObject target;

    void Start()
    {
        vehicle = Instantiate(vehicle, this.transform);
        target = Instantiate(target, this.transform);

        vehicle.GetComponent<Vehicle6_2>().target = target.GetComponent<Target6_2>();
    }
}
