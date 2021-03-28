using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Creature : MonoBehaviour
{
    protected Rigidbody body;
    protected Collider col;

    protected virtual void Awake()
    {
        body = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        body.useGravity = false;
    }
}
