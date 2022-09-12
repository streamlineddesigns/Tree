using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StudioByStorm.Registries;

namespace StudioByStorm.Gravity {

    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private Transform _cam;
        [SerializeField] private Animator _animator;
        
        private float _groundCheckRadius = 0.3f;
        private float _speed = 8;
        private float _turnSpeed = 1500f;
        private float _jumpForce = 500f;

        private Rigidbody _rigidbody;
        private Vector3 _direction;

        private GravityBody _gravityBody;
        
        void Start()
        {
            //players physics rigidbody
            _rigidbody = transform.GetComponent<Rigidbody>();

            //players gravity body
            _gravityBody = transform.GetComponent<GravityBody>();
        }

        void Update()
        {
            //the direction the player is moving
            _direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;

            //if the player is grounded using a physics check
            bool isGrounded = Physics.CheckSphere(_groundCheck.position, _groundCheckRadius, _groundMask);

            //play animation if player is jumping
            _animator.SetBool("isJumping", !isGrounded);

            //if player presses jump button
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {

                //add force to players rigidbody in the direction of their gravity body
                _rigidbody.AddForce(-_gravityBody.GravityDirection * _jumpForce, ForceMode.Impulse);
            }
        }
        
        void FixedUpdate()
        {
            //player is running if their direction is larger than a threshold
            bool isRunning = _direction.magnitude > 0.1f;
            
            
            if (isRunning)
            {
                //if player is running, set their direction to their transforms forward direction
                Vector3 direction = transform.forward * _direction.z;

                //move the rigidbody based on their direction
                _rigidbody.MovePosition(_rigidbody.position + direction * (_speed * Time.fixedDeltaTime));
                
                //generate players right direction in the form of a quaternion, based on the direction vector
                Quaternion rightDirection = Quaternion.Euler(0f, _direction.x * (_turnSpeed * Time.fixedDeltaTime), 0f);

                //set players rotation based on the right direction, and their current rotation
                Quaternion newRotation = Quaternion.Slerp(_rigidbody.rotation, _rigidbody.rotation * rightDirection, Time.fixedDeltaTime * 3f);;
                
                //rotate their rigidbody based on the new rotation
                _rigidbody.MoveRotation(newRotation);
            }

            //set the running animation to whether they are running or not
            _animator.SetBool("isRunning", isRunning);
        }
    }

}