using System.Collections.Generic;
using UnityEngine;

//A Gravity Area
public class GravityAreaCenter : GravityArea
{
    
    public override Vector3 GetGravityDirection(GravityBody _gravityBody)
    {
        //the position of this game object (Vector), minus the supplied gravitiy bodies position (Vector) = directional vector
        return (transform.position - _gravityBody.transform.position).normalized;
    }
}
