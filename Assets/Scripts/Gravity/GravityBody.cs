using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//a gravity body is something that would be attached to a player or a rock
//it will be affected by the gravity areas it is currently inside of
[RequireComponent(typeof(Rigidbody))]
public class GravityBody : MonoBehaviour
{
    private static float GRAVITY_FORCE = 800;
    
    //a getter property for our gravity direction, with some functionality any time it is retrieved
    public Vector3 GravityDirection
    {
        get
        {
            //if we don't have a gravity area, affecting our gravity body, our gravity direction does not exist
            if (_gravityAreas.Count == 0) return Vector3.zero;

            //Sort our gravity areas by level of priority
            _gravityAreas.Sort((area1, area2) => area1.Priority.CompareTo(area2.Priority));

            //return the gravity direction from the gravity area with the highest level of priority
            return _gravityAreas.Last().GetGravityDirection(this).normalized;
        }
    }

    private Rigidbody _rigidbody;
    private List<GravityArea> _gravityAreas;

    void Start()
    {
        _rigidbody = transform.GetComponent<Rigidbody>();
        _gravityAreas = new List<GravityArea>();
    }
    
    //run our physics calculations in fixed update
    void FixedUpdate()
    {
        //add a force to the rigidbody based on the gravity direction
        //remember that the gravity direction is a getter property that returns data based on some operations
        //we take our gravity direction and multiply it by our force, time, and set the force mode type
        //acceleration of course because we are dealing with gravity
        _rigidbody.AddForce(GravityDirection * (GRAVITY_FORCE * Time.fixedDeltaTime), ForceMode.Acceleration);

        //we get the up rotation of our gravity body, which will be the opposite of the gravity direction; naturally
        Quaternion upRotation = Quaternion.FromToRotation(transform.up, -GravityDirection);

        //we create our new rotation based on our current rotation, and the target upRotation
        //Slerp because it interpolates nicely between degrees of rotation, and not lerp because lerp is used for interpolation between distance, not degrees
        Quaternion newRotation = Quaternion.Slerp(_rigidbody.rotation, upRotation * _rigidbody.rotation, Time.fixedDeltaTime * 3f);;
        
        //we set our new rotation to our rigidbody
        _rigidbody.MoveRotation(newRotation);
    }

    //this adds a gravity area to our list of gravity areas affecting this gravity body
    public void AddGravityArea(GravityArea gravityArea)
    {
        _gravityAreas.Add(gravityArea);
    }

    //this removes a gravity area from our list of gravity areas affecting this gravity body
    public void RemoveGravityArea(GravityArea gravityArea)
    {
        _gravityAreas.Remove(gravityArea);
    }
}