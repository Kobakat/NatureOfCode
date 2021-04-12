using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exercise6_1 : MonoBehaviour
{
    public Camera cam { get; set; }
    public GameObject vehicle;
    public GameObject target;

    void Start()
    {
        Cursor.visible = false;
        cam = Camera.main;
        vehicle = Instantiate(vehicle, this.transform);
        target = Instantiate(target, this.transform);

        vehicle.GetComponent<Vehicle6_1>().target = target.transform;
    }

    void FixedUpdate()
    {
        target.transform.position = MousePosition(cam);
    }
    Vector2 MousePosition(Camera camera)
    {
        return camera.ScreenToWorldPoint(Input.mousePosition);
    }
}
