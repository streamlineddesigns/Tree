using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using StudioByStorm.Registries;
using StudioByStorm.Optimizations;
using StudioByStorm.UI;
using Lean.Gui;
using StudioByStorm.GestureRecognition;
using StudioByStorm.EventPublishers;

namespace StudioByStorm.Gravity.Player {

    public class PlayerController : MonoBehaviour
    {
        private Surface surface;
        private bool isOnSurface = false;
        private bool isInAtmosphere = false;
        private float offSurfaceTimer = 0.0f;
        private float currentOffSurfaceTimer;

        private Rigidbody2D rigidbody;
        private ContactPoint2D[] rigidbodyContacts;
        private Vector2 lastContactPoint;    

        private Vector2 gravityDirection = Vector2.zero;
        private float gravityForce = 850;

        private bool isMovementLocked;
        private bool isUsingDirectionalMovement = true;
        private Vector2 movementDirection = Vector2.zero;
        private Vector2 swipeDirection = Vector2.zero;
        private float movementForce = 7f;
        private int forwardDirection;

        private bool isJumping = false;
        private bool isPowerJumping = false;
        private bool didJump = false;
        private float jumpForce = 7f;
        private float powerJumpForce = 11f;

        private Vector2 dashDirection = Vector2.zero;
        private float spaceDashForce = 7f;
        private Vector2 minimumSpaceVelocity = new Vector2(0.25f, 0.25f);
        private float atmosphereDashForce = 5f;
        private bool didAtmosphereDash = false;
        private bool didSpaceDash = false;
        private bool canDash = true;
        private int dashCountAllowed = 1;
        private int dashCount;
        private float originalScale;

        protected float JoystickTimerTarget = 0.25f;
        protected float currentJoystickTimer = 0.0f;
        protected bool lerping;

        
        void Awake()
        {

        }

        void Start()
        {
            rigidbody = transform.GetComponent<Rigidbody2D>();
            rigidbodyContacts = new ContactPoint2D[1];
            currentOffSurfaceTimer = offSurfaceTimer;
            originalScale = gameObject.transform.localScale.x;
        }

        void OnEnable()
        {
            GameEventPublisher.OnJoystickDirectionChange += OnJoystickDirectionChange;
            GameEventPublisher.OnTap += OnTap;
            GameEventPublisher.OnLongTap += OnLongTap;
        }

        void OnDisable()
        {
            GameEventPublisher.OnJoystickDirectionChange += OnJoystickDirectionChange;
            GameEventPublisher.OnTap -= OnTap;
            GameEventPublisher.OnLongTap -= OnLongTap;
        }

        public Vector2 GetSwipeDirection()
        {
            return swipeDirection;
        }

        public void LockMovement(bool isLock)
        {
            isMovementLocked = isLock;
        }

        public void DoPathMovement(Vector3[] waypoints)
        {
            if (lerping) {
                return;
            }
            StartCoroutine(TravelMovement(waypoints));
        }

        protected IEnumerator TravelMovement(Vector3[] waypoints)
        {
            lerping = true;
            gameObject.transform.DOMove(GameManager.Singleton.nearbyNode.GetPosition(), 0.1f, false);
            GameManager.Singleton.nearbyNode.GetData<Node>().InnerGraphic.gameObject.transform.DOScale(0.85f, 0.1f);
            yield return new WaitUntil(() => (Vector2)gameObject.transform.position == GameManager.Singleton.nearbyNode.GetPosition());
            GameManager.Singleton.nearbyNode.GetData<Node>().InnerGraphic.gameObject.transform.DOScale(1.0f, 0.1f);

            gameObject.transform.DOScale(1.0f, 0.25f);

            for (int i = 0; i < waypoints.Length; i++) {
                if (i % 2 == 0) {
                    gameObject.transform.DOMove(waypoints[i], 0.1f, false);
                } else {
                    gameObject.transform.DOPath(new Vector3[1]{waypoints[i]}, 0.1f, PathType.Linear);
                }
                yield return new WaitUntil(() => gameObject.transform.position == waypoints[i]);
            }

            //gameObject.transform.DOPath(waypoints, 1.0f, PathType.Linear);
            //yield return new WaitUntil(() => gameObject.transform.position == waypoints[waypoints.Length - 1]);

            gameObject.transform.DOScale(originalScale, 0.25f);
            gameObject.transform.DOMove(GameManager.Singleton.nearbyNode.GetPosition(), 0.1f, false);
            yield return new WaitUntil(() => (Vector2)gameObject.transform.position == GameManager.Singleton.nearbyNode.GetPosition());
            lerping = false;
        }

        void Update()
        {
            StateCleanUp();
        }

        void FixedUpdate()
        {
            if (isOnSurface || isInAtmosphere)
            {
                if (gravityDirection != Vector2.zero) {
                    ApplyGravity();
                }
                
                if (movementDirection != Vector2.zero) {
                    Move();
                }
            }

            if (! isInAtmosphere && ! isOnSurface) {
                ApplyGravityFailSafe();
            }

            if (isOnSurface) {
                if (isJumping) {
                    Jump();
                }
            }

            if (! isOnSurface) {
                ResetMovement();
                
                if (dashDirection != Vector2.zero) {
                    Dash();
                }
            }
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.CompareTag("Surface")) {

                currentOffSurfaceTimer = offSurfaceTimer;
                surface = collider.gameObject.GetComponent<Surface>();

            } else if (collider.CompareTag("Atmosphere")) {
                //CameraController.SetTarget(collider.gameObject.transform);
            }
        }
        
        void OnTriggerStay2D(Collider2D collider)
        {
            if (collider.CompareTag("Surface") && surface == null) {

                isOnSurface = true;
                rigidbody.GetContacts(rigidbodyContacts);
                lastContactPoint = rigidbodyContacts[0].point;
                gravityDirection = (lastContactPoint - (Vector2) transform.position).normalized;
            
            } else if (collider.CompareTag("Surface") && surface != null) {

                isOnSurface = true;
                Vector2 nearestGravityPoint = getNearestGravityPoint();
                lastContactPoint = getNearestGravityPoint();
                gravityDirection = (nearestGravityPoint - (Vector2) transform.position).normalized;

            } else if (collider.CompareTag("Atmosphere")) {
                
                isInAtmosphere = true;

                if (surface != null) {
                    Vector2 nearestGravityPoint = getNearestGravityPoint();
                    gravityDirection = (nearestGravityPoint - (Vector2) transform.position).normalized;

                } else {
                    gravityDirection = ((Vector2) collider.transform.position - (Vector2) transform.position).normalized;
                }
            }
        }

        void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.CompareTag("Surface")) {
                isOnSurface = false;

            } else if (collider.CompareTag("Atmosphere")) {
                isInAtmosphere = false;
                gravityDirection = Vector2.zero;
                surface = null;
            }
        }

        protected void OnJoystickDirectionChange(Vector2 Direction)
        {
            Vector2 joystickDir = Direction;

            if (isOnSurface) {
                movementDirection = joystickDir;//$$(r.endPoint - r.startPoint).normalized;
            } else {
                dashDirection = joystickDir;//$$(r.endPoint - r.startPoint).normalized;
            }

            SetRelativeForwardDirection();
        }

        protected void OnTap(Vector2 tap) 
        {
            if (isMovementLocked) {
                return; 
            }

            if (!isJumping && isOnSurface) {
                isJumping = true;
                isPowerJumping = false;
            }
        }

        protected void OnLongTap(Vector2 tap)
        {
            if (isMovementLocked) {
                return; 
            }

            if (!isJumping && isOnSurface) {
                isJumping = true;
                isPowerJumping = true;
            }
        }

        protected void ApplyGravityFailSafe()
        {
            if (GameManager.Singleton == null || GameManager.Singleton.nearbyNode == null) {
                return;
            }

            if (Mathf.Abs(rigidbody.velocity.x) <= minimumSpaceVelocity.x && Mathf.Abs(rigidbody.velocity.y) <= minimumSpaceVelocity.y) {
                gravityDirection = (GameManager.Singleton.nearbyNode.GetPosition() - (Vector2) transform.position).normalized;
                rigidbody.AddForce(gravityDirection * (gravityForce/2 * Time.fixedDeltaTime), ForceMode2D.Impulse);
            }
        }

        protected void ApplyGravity()
        {
            transform.up = - gravityDirection;
            rigidbody.AddForce(gravityDirection * (gravityForce * Time.fixedDeltaTime));
        }

        protected void Move()
        {
            rigidbody.AddForce(transform.right * forwardDirection * movementForce);
        }

        protected void ResetMovement()
        {
            if (currentOffSurfaceTimer > 0.0f) {
                currentOffSurfaceTimer -= Time.deltaTime;
            } else {
                movementDirection = Vector2.zero;
            }
        }

        protected void Jump()
        {
            ActionView actionView = GameManager.Singleton.ViewRegistry.TryGetValue(ViewName.ActionView) as ActionView;
            Vector2 joystickDir = actionView.LeanJoyStick.ScaledValue;
            if (joystickDir.x != 0.0f || joystickDir.y != 0.0f) {
                return;
            }

            float force = (isPowerJumping) ? powerJumpForce : jumpForce;
            rigidbody.AddForce(- gravityDirection * force, ForceMode2D.Impulse);
            didJump = true;
            isJumping = false;
            isPowerJumping = false;
        }

        protected void Dash()
        {
            if (canDash && currentOffSurfaceTimer <= 0.0f) {
                float dashForce;

                if (isInAtmosphere) {
                    dashForce = atmosphereDashForce;
                    didAtmosphereDash = true;
                } else {
                    dashForce = spaceDashForce;
                    didSpaceDash = true;
                }

                rigidbody.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
                
                if (dashCount < dashCountAllowed) {
                    dashCount++;
                } else {
                    dashCount = 0;
                    canDash = false;
                }
            }

            dashDirection = Vector2.zero;
        }

        protected Vector2 getNearestGravityPoint()
        {

            Vector3 nearestGravityPoint = surface.gravityPoints[0].position;

            for (int i = 1; i < surface.gravityPoints.Length; i++) {
                nearestGravityPoint = (getDistance(nearestGravityPoint, transform.position) <
                                getDistance(surface.gravityPoints[i].position, transform.position) ) 
                                ? nearestGravityPoint : surface.gravityPoints[i].position;
            }

            return (Vector2) nearestGravityPoint;
        }

        protected void SetRelativeForwardDirection()
        {
            forwardDirection = (getDistance(transform.right, movementDirection) <
                                getDistance(- transform.right, movementDirection) ) 
                                ? 1 : -1;
        }

        protected float getDistance(Vector3 p1, Vector3 p2)
        {
            return (Mathf.Pow(p1.x - p2.x, 2) + Mathf.Pow(p1.y - p2.y, 2));
        }

        protected void StateCleanUp()
        {
            //is on surface
            if (isOnSurface) {
                canDash = true;
                didJump = false;
                didAtmosphereDash = false;
                didSpaceDash = false;
            }

            //in atmosphere and used dash
            if (isInAtmosphere && didAtmosphereDash) {
                canDash = false;

            //simply in atmosphere
            } else if (isInAtmosphere) {
                canDash = true;
            
            //in space & used dash
            } else if (! isInAtmosphere && ! isOnSurface && didSpaceDash && dashCount >= dashCountAllowed) {
                canDash = false;
            
            //simply in space
            } else if (! isInAtmosphere && ! isOnSurface) {
                canDash = true;
                didJump = false;
            }
        }
    }

}