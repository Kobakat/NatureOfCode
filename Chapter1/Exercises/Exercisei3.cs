using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exercisei3 : MonoBehaviour
{
    Exercise3Mover mover;
    Exercise3Food food;

    void Start()
    {
        food = new Exercise3Food();
        mover = new Exercise3Mover(food.mover.transform);           
    }

    void FixedUpdate()
    {
        mover.step();
        mover.CheckEdges();
    }
}

public class Exercise3Mover
{
    Transform prey;
    float followStrength;
    // The basic properties of a mover class
    private Vector3 location;

    // The window limits
    private Vector2 minimumPos, maximumPos;

    // Gives the class a GameObject to draw on the screen
    public GameObject mover = GameObject.CreatePrimitive(PrimitiveType.Sphere);

    public Exercise3Mover(Transform Prey)
    {
        findWindowLimits();
        location = Vector2.zero;
        //We need to create a new material for WebGL
        Renderer r = mover.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Diffuse"));
        followStrength = 15;
        prey = Prey;
    }

    public void step()
    {
        location = mover.transform.position;
        //Each frame choose a new Random number 0,1,2,3, 
        //If the number is equal to one of those values, take a step

        float choice = Random.Range(0, 1f);
        if (choice <= .2f)
        {
            location.y++;
        }
        else if (choice > .2f && choice <= .3f)
        {
            location.y--;
        }
        else if (choice > .4f && choice < .5f)
        {
            location.x--;
        }
        else
        {
            location = Vector3.MoveTowards(location, prey.position, followStrength);
        }

        mover.transform.position += Time.deltaTime * location;
    }

    public void CheckEdges()
    {
        location = mover.transform.position;

        if (location.x > maximumPos.x)
        {
            location = Vector2.zero;
        }
        else if (location.x < minimumPos.x)
        {
            location = Vector2.zero;
        }
        if (location.y > maximumPos.y)
        {
            location = Vector2.zero;
        }
        else if (location.y < minimumPos.y)
        {
            location = Vector2.zero;
        }
        mover.transform.position = location;
    }

    private void findWindowLimits()
    {
        // We want to start by setting the camera's projection to Orthographic mode
        Camera.main.orthographic = true;
        // Next we grab the minimum and maximum position for the screen
        minimumPos = Camera.main.ScreenToWorldPoint(Vector2.zero);
        maximumPos = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
    }
}

public class Exercise3Food
{
    public GameObject mover; 

    public Exercise3Food()
    {
        mover = GameObject.CreatePrimitive(PrimitiveType.Cube);

        float randomPosX = Random.Range(0, 5);
        float randomPosY = Random.Range(0, 5);

        mover.transform.position = new Vector3(randomPosX, randomPosY);
    }
}
