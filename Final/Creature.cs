using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Creature : MonoBehaviour
{
    public float maxXZSpeed = 10;
    public float maxYSpeed = 15;

    public Ecosystem ecosystem { get; set; }
    
    protected Rigidbody body;
    protected Vector3 moveForce;
    protected Vector2 minBound;
    protected Vector2 maxBound;

    protected virtual void Start()
    {
        body = GetComponent<Rigidbody>();
        GetPerlinTerrainBounds();
    }

    protected virtual void FixedUpdate()
    {
        moveForce = Vector3.zero;
    }

    void GetPerlinTerrainBounds()
    {
        minBound = new Vector2(0, 0);
        maxBound = new Vector2((int)(ecosystem.terrainManager.terrainSize.x + .1f), (int)(ecosystem.terrainManager.terrainSize.y + .1f));
    }

    protected void SpawnInRandomLocation(float heightPos, bool randomRot)
    {
        int posX = Random.Range(0, (int)(ecosystem.terrainManager.terrainSize.x + 0.1f));
        int posY = Random.Range(0, (int)(ecosystem.terrainManager.terrainSize.y + 0.1f));
        float rotY = randomRot ? Random.Range(-180f, 180f) : 0;

        transform.position = ecosystem.terrainManager.terrain[posX, posY] + Vector3.up * heightPos;
        transform.rotation = Quaternion.Euler(new Vector3(0, rotY, 0));
    }
}
