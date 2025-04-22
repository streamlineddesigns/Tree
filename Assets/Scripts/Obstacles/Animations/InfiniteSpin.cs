using UnityEngine;

namespace StudioByStorm.Obstacles.Animations {

    [DisallowMultipleComponent]
    public class InfiniteSpin : MonoBehaviour
    {
        [Tooltip("Degrees per second around the chosen axis.")]
        public Vector3 rotationSpeed = new Vector3(0f, 0f, 10f);

        void Update()
        {
            // Rotate around the object's local axes
            transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

}