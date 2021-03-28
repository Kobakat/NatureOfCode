using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : Creature
{
    public float speed = 1;
    public float intelligence = .5f;

    List<Food> foods;
    Food targetFood;

    #region Mono
    protected sealed override void Awake()
    {
        base.Awake();
        FindAllFood();
    }

    void FixedUpdate()
    {
        FindNearestPieceOfFood();
        SelectMoveType();

        Debug.DrawLine(transform.position, targetFood.transform.position, Color.red);
    }

    #endregion

    #region Logic Functions
    void FindAllFood()
    {
        foods = new List<Food>();

        foreach (Food f in FindObjectOfType<Ecosystem>().resources)
        {
            foods.Add(f);
        }
    }

    void FindNearestPieceOfFood()
    {
        foreach (Food f in foods)
        {
            if (f != targetFood)
            {
                float distance = Vector3.Distance(this.transform.position, f.transform.position);
                float currentTargetDistance =
                    targetFood ?
                    Vector3.Distance(this.transform.position, targetFood.transform.position)
                    : 10000;

                if (distance < currentTargetDistance)
                {
                    targetFood = f;
                }
            }
        }
    }

    /// <summary>
    /// Determine if the fly should move randomly or towards the nearest piece of food
    /// </summary>
    void SelectMoveType()
    {
        float selection = Random.Range(0f, 1f);

        if (selection > intelligence)
            MoveRandomly();

        else
            MoveTowardsNearestPieceOfFood();

    }

    void MoveTowardsNearestPieceOfFood()
    {
        Vector3 directionalVector = targetFood.transform.position - this.transform.position;
        directionalVector.Normalize();

        body.AddForce(directionalVector * speed, ForceMode.Acceleration);
    }

    void MoveRandomly()
    {
        Vector3 randomVector = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            0);

        randomVector.Normalize();

        body.AddForce(randomVector * speed, ForceMode.Acceleration);
    }

    

    #endregion

}
