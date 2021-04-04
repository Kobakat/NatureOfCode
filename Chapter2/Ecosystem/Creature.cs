using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Creature : MonoBehaviour
{
    public float maxXZSpeed = 10;
    public float maxYSpeed = 15;

    protected Rigidbody body;

    protected Vector2 minBound;
    protected Vector2 maxBound;

    protected virtual void Awake()
    {
        body = GetComponent<Rigidbody>();
        GetPerlinTerrainBounds();
    }

    void GetPerlinTerrainBounds()
    {
        Ecosystem ecosystem = FindObjectOfType<Ecosystem>();

        minBound = new Vector2(-0.5f, -0.5f);
        maxBound = new Vector2(0.5f, 0.5f) + ecosystem.terrainManager.terrainSize;
    }


}
