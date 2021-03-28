using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Creature : MonoBehaviour
{
    protected Vector3 velocity;
    protected Vector3 acceleration;

    protected virtual void Awake()
    {
    }
}
