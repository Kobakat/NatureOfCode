using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinTerrain : MonoBehaviour
{
    public Vector3[,] terrain { get; set; }
    [SerializeField] GameObject terrainPiece;
    public float rotationGradient;
    public Vector2 terrainSize;
    public Color color1, color2, color3, color4, color5, color6;

    public float maxHeight;


    public void GenerateTerrain()
    {
        terrain = new Vector3[(int)(terrainSize.x + 0.1f), (int)(terrainSize.y + 0.1f)];

        float xPos = 0;
        for(int i = 0; i < (int)terrainSize.x; i++)
        {
            float yPos = 0;

            for(int j = 0; j < (int)terrainSize.y; j++)
            {
                float position = ExtensionMethods.map(
                    Mathf.PerlinNoise(xPos, yPos), 
                    0f, 
                    1f, 
                    0f, 
                    maxHeight);

                float rotation = ExtensionMethods.map(
                    Mathf.PerlinNoise(xPos, yPos),
                    0f,
                    1f,
                    0f,
                    5f);

                Vector3 perlinRotation = new Vector3(Mathf.Cos(rotation), Mathf.Sin(rotation), 0) * rotationGradient;
                Quaternion quat = Quaternion.Euler(perlinRotation);

                GameObject obj = Instantiate(terrainPiece, new Vector3(i, position, j), quat, this.transform);

                Renderer terrainRender = obj.GetComponent<Renderer>();
                terrainRender.material.SetColor("_Color", colorTerrain(obj.transform.position.y));
                yPos += 0.06f;

                terrain[i, j] = obj.transform.position;
            }

            xPos += 0.06f;
        }
    }

    Color colorTerrain(float height)
    {
        if (height >= maxHeight * .8f)
            return color1;
        else if (height < maxHeight * .8f && height >= maxHeight * .6f)
            return color2;
        else if (height < maxHeight * .6f && height >= maxHeight * .4f)
            return color3;
        else if (height < maxHeight * .4f && height >= maxHeight * .3f)
            return color4;
        else if (height < maxHeight * .3f && height >= maxHeight * .2f)
            return color5;
        else
            return color6;
    }
}
