using System;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Tooltip("The speed at which the object rotates")]
    [SerializeField] float speed = 0.2f;
    enum Directions { Clockwise, Counterclockwise }
    [Tooltip("The direction that the object should rotate in")]
    [SerializeField] Directions direction = Directions.Clockwise;

    BoundsInfo bounds; // The object's meshes' bounds script

    void Start()
    {
        bounds = new BoundsInfo(gameObject);
    }

    void FixedUpdate()
    {
        // Rotating clockwise
        if (direction.Equals(Directions.Clockwise))
        {
            transform.RotateAround(bounds.GlobalCentre, Vector3.up, speed);
        }
        // Rotating counterclockwise
        else
        {
            transform.RotateAround(bounds.GlobalCentre, Vector3.up, -speed);
        }
    }
}