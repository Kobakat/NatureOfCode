using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Flower : MonoBehaviour, IResource
{
    [SerializeField] GameObject branch;
    public Ecosystem ecosystem { get; set; }
    public int branchCount = 3;
    public int xCoord { get; private set; }
    public int zCoord { get; private set; }
    
    void Start()
    {
        if(Time.time < 3f)
            SnapToTerrain();
        StartCoroutine(GenerateBranchesAfterPhysicsUpdate());
    }

    public void SnapToTerrain()
    {
        xCoord= Random.Range(0, (int)(ecosystem.terrainManager.terrainSize.x + .1f));
        zCoord= Random.Range(0, (int)(ecosystem.terrainManager.terrainSize.y + .1f));

        transform.position = ecosystem.terrainManager.terrain[xCoord, zCoord] + Vector3.up * 0.5f;       
    }

    public void SnapToTerrain(int x, int z)
    {
        xCoord = x;
        zCoord = z;
        transform.position = ecosystem.terrainManager.terrain[xCoord, zCoord] + Vector3.up * 0.5f;
    }

    IEnumerator GenerateBranchesAfterPhysicsUpdate()
    {
        yield return new WaitForFixedUpdate();
        GenerateBranches();
    }

    void GenerateBranches()
    {
        for(int i = 0; i < branchCount; i++)
        {
            float branchHeight = Random.Range(0f, 0.3f);
            int sideX = Random.Range(-1, 2);
            int sideZ = 0;
            if (sideX == 0)
                sideZ = (Random.Range(0, 2) * 2) - 1; 

        
            Vector3 end = transform.position + transform.up * branchHeight;
            end.x += sideX * .2f;
            end.z += sideZ * .2f;

            Vector3 rayDir = new Vector3(-sideX, 0, -sideZ);
            GameObject newBranch = Instantiate(branch, end, Quaternion.identity, this.transform);
            SnapToStem(newBranch, rayDir);
        }
    }

    void SnapToStem(GameObject newBranch, Vector3 rayDir)
    {
        RaycastHit hit;
        int layermask = 1 << 11;
        Debug.DrawRay(newBranch.transform.position, rayDir, Color.red);
        Physics.Raycast(newBranch.transform.position, rayDir, out hit, 1, layermask);

        newBranch.transform.position = hit.point;
        newBranch.transform.rotation = Quaternion.LookRotation(Vector3.up - hit.normal);
    }
}
