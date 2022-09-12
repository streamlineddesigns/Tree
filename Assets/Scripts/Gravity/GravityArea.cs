using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class GravityArea : MonoBehaviour
{
    //the Priority of our gravity area. Different priorities will affect a gravity body in order
    [SerializeField] private int _priority;
    public int Priority => _priority;
    
    void Start()
    {
        //gravity area is a play that can be entered into by collision. It is a trigger area
        transform.GetComponent<Collider>().isTrigger = true;
    }
    
    //will return the directional vector between a supplied gravity body, and the gravity area game object
    public abstract Vector3 GetGravityDirection(GravityBody _gravityBody);
    
    //if something enteres this area
    private void OnTriggerEnter(Collider other)
    {
        //if it is a gravity body
        if (other.TryGetComponent(out GravityBody gravityBody))
        {
            //tell that gravity body, that its new gravity area, is this gravity area
            gravityBody.AddGravityArea(this);
        }
    }
    
    //if something exists this area
    private void OnTriggerExit(Collider other)
    {
        //see if its a gravity body
        if (other.TryGetComponent(out GravityBody gravityBody))
        {
            //remove this gravity area from that gravity body
            gravityBody.RemoveGravityArea(this);
        }
    }
}