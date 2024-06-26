using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Gui;
using StudioByStorm;
using StudioByStorm.Registries;
using StudioByStorm.Optimizations;
using StudioByStorm.UI;
using StudioByStorm.GestureRecognition;
using StudioByStorm.EventPublishers;
using StudioByStorm.Helpers;
using StudioByStorm.Obstacles;
using StudioByStorm.FX;

namespace StudioByStorm.Gravity.Player {

    public class PlayerController : MonoBehaviour
    {
        public float _movementForce = 0.1f;
        public float mockNodeRadius = 0.6f;
        public GameObject JumpIndicator;
        public GameObject boostIndicator;
        [SerializeField] private GameObject trail;

        [SerializeField] private PlayerPositionHelper PlayerPositionHelper;

        [SerializeField] public Material glowMaterial;
        [SerializeField] private Material lightMaterial;
        [SerializeField] private Material darkMaterial;
        [SerializeField] private Color lightColor;
        [SerializeField] private Color darkColor;
        [SerializeField] private Color hitObstacleColor;
        [SerializeField] private GameObject light2D;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private bool isLightColor;
        private bool isEnforcingLightColor = false;
        private int currentNodeGameID;
        private bool isPositionHelperPlayingBack;
        private Material currentMaterial;
        private Color currentColor;
        private bool isHitAnimationPlaying;
        private bool isHitObstacle;

        private Surface surface;
        private bool isOnSurface;
        private bool isInAtmosphere;
        private float offSurfaceTimer = 0.0f;
        private float currentOffSurfaceTimer;

        private Rigidbody2D rigidbody;
        private ContactPoint2D[] rigidbodyContacts;
        private Vector2 lastContactPoint;    

        private Vector2 gravityDirection = Vector2.zero;
        private float gravityForce = 850;

        private bool isMovementLocked;
        private bool isUsingDirectionalMovement = true;
        private Vector2 previousMovementDirection = Vector2.zero;
        private Vector2 movementDirection = Vector2.zero;
        private Vector2 swipeDirection = Vector2.zero;
        private float movementForce = 7f;
        private int forwardDirection;

        private bool isJumping = false;
        private bool isPowerJumping = false;
        private bool didJump = false;
        private float jumpForce = 7f;
        private float powerJumpForce = 13f;

        private Vector2 dashDirection = Vector2.zero;
        private float spaceDashForce = 9f;
        private Vector2 minimumSpaceVelocity = new Vector2(0.25f, 0.25f);
        private float atmosphereDashForce = 7f;
        private bool didAtmosphereDash = false;
        private bool didSpaceDash = false;
        private bool canDash = true;
        private int extraDashCountAllowed = 0;
        private int dashCount;
        private float originalScale;

        private float JoystickTimerTarget = 0.25f;
        private float currentJoystickTimer = 0.0f;
        private bool lerping;

        private bool isJoystickUp = true;
        private Vector2 joystickDownPoint;
        private Vector2 joystickUpPoint;
        private int playerHeartsCount = 3;
        private bool isLevelLost = false;

        private int playerNodeID = -1;
        private bool isMoving;

        private bool isToggleColorOn = false;
        
        void Awake()
        {

        }

        void Start()
        {
            rigidbody = transform.GetComponent<Rigidbody2D>();
            rigidbodyContacts = new ContactPoint2D[1];
            currentOffSurfaceTimer = offSurfaceTimer;
            originalScale = gameObject.transform.localScale.x;

            bool isInitializingPlayerHearts = true;
            UpdatePlayerHearts(isInitializingPlayerHearts);
        }

        void OnEnable()
        {
            GameEventPublisher.OnJoystickDirectionChange += OnJoystickDirectionChange;
            FTUECheck();
        }

        void OnDisable()
        {
            GameEventPublisher.OnJoystickDirectionChange -= OnJoystickDirectionChange;
        }

        private void FTUECheck()
        {
            if (GameManager.Singleton.LevelManager.currentChapterID == 0 || GameManager.Singleton.LevelManager.currentChapterID == 5) {
                isLightColor = false;
                light2D.SetActive(false);
                spriteRenderer.color = darkColor;
                spriteRenderer.material = darkMaterial;
                currentColor = darkColor;
                currentMaterial = darkMaterial;
            } else if (GameManager.Singleton.LevelManager.currentChapterID == 1) {
                isLightColor = true;
                light2D.SetActive(true);
                spriteRenderer.color = lightColor;
                spriteRenderer.material = lightMaterial;
                currentColor = lightColor;
                currentMaterial = lightMaterial;
            } else {
                isToggleColorOn = true;
            }
        }

        public bool isPlayerGrounded() 
        {
            return isOnSurface;
        }

        public bool isPlayerInAtmosphere()
        {
            return isInAtmosphere;
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
            
            GameEventPublisher.PublishPlayerTravel();

            gameObject.transform.DOMove(GameManager.Singleton.nearbyNode.GetPosition(), 0.1f, false);
            GameManager.Singleton.nearbyNode.GetData<Node>().InnerGraphic.gameObject.transform.DOScale(0.85f, 0.1f);
            yield return new WaitForSeconds(0.1333f);
            GameManager.Singleton.nearbyNode.GetData<Node>().InnerGraphic.gameObject.transform.DOScale(1.0f, 0.1f);

            gameObject.transform.DOScale(1.0f, 0.25f);

            /*for (int i = 0; i < waypoints.Length; i++) {
                if (i % 2 == 0) {
                    gameObject.transform.DOMove(waypoints[i], 0.1f, false);
                } else {
                    gameObject.transform.DOPath(new Vector3[1]{waypoints[i]}, 0.1f, PathType.Linear);
                }
                yield return new WaitUntil(() => gameObject.transform.position == waypoints[i]);
            }*/

            AudioManager.Singleton.Play(SoundType.Travel);
            
            transform.up = waypoints[waypoints.Length - 1];
            gameObject.transform.DOPath(waypoints, 0.75f, PathType.Linear).SetEase(Ease.Linear).OnComplete(() => {
                Move();
            });

            yield return new WaitForSeconds(0.7833f);

            //gameObject.transform.DOPath(waypoints, 1.0f, PathType.Linear);

            gameObject.transform.DOScale(originalScale, 0.25f);
            //gameObject.transform.DOMove(GameManager.Singleton.nearbyNode.GetPosition(), 0.1f, false);
            
            //yield return new WaitUntil(() => (Vector2)gameObject.transform.position == GameManager.Singleton.nearbyNode.GetPosition());
            GameManager.Singleton.nearbyNode.GetData<Node>().InnerGraphic.gameObject.transform.DOScale(0.85f, 0.1f).OnComplete(() => {GameManager.Singleton.nearbyNode.GetData<Node>().InnerGraphic.gameObject.transform.DOScale(1.0f, 0.1f);});
           
            transform.up = Vector3.up;

            yield return null;
            lerping = false;
        }

        void Update()
        {
            movementForce = _movementForce;
            StateCleanUp();
        }

        void FixedUpdate()
        {
            if (isPositionHelperPlayingBack) {
                return;
            }

            if (isOnSurface || isInAtmosphere)
            {
                if (gravityDirection != Vector2.zero && !isOnSurface) {
                    ApplyGravity();
                }
                
                /*if (movementDirection != Vector2.zero) {
                    Move();
                }*/
            }

            if (! isInAtmosphere && ! isOnSurface) {
                ApplyGravityFailSafe();
            }

            //looks like this is handled elsewhere now
            /*if (isOnSurface) {
                if (isJumping) {
                    Jump();
                }
            }*/

            if (! isOnSurface) {
                ResetMovement();
                
                if (dashDirection != Vector2.zero) {
                    Dash();
                }
            }
        }

        IEnumerator ToggleColor(int nodeGameObjectID)
        {
            yield return new WaitUntil(() => !isHitAnimationPlaying);

            if (currentNodeGameID != nodeGameObjectID) {
                currentNodeGameID = nodeGameObjectID;
                isLightColor = !isLightColor;

                if (isLightColor) {
                    light2D.SetActive(true);
                    spriteRenderer.color = lightColor;
                    spriteRenderer.material = lightMaterial;
                    currentColor = lightColor;
                    currentMaterial = lightMaterial;
                } else {
                    light2D.SetActive(false);
                    spriteRenderer.color = darkColor;
                    spriteRenderer.material = darkMaterial;
                    currentColor = darkColor;
                    currentMaterial = darkMaterial;
                }
            }  
        }

        private void HitObstacle()
        {
            //play the hit animation
            if (! isHitObstacle) {
                AudioManager.Singleton.Play(SoundType.WrongObstacleHit);
                GameEventPublisher.PublishPlayerHitWrongObstacle();
                isHitObstacle = true;
                HitObstacleAnimation();

                //check if the player still has hearts left after this
                if ((playerHeartsCount - 1) > 0) {
                    playerHeartsCount--;
                    UpdatePlayerHearts();

                } else if (! isLevelLost) {
                    playerHeartsCount--;
                    UpdatePlayerHearts();
                    StartCoroutine(LevelLostAnimation());
                }
            }
        }

        private void HitObstacleAnimation()
        {
            Sequence hitSequenceAnimation = DOTween.Sequence();
            hitSequenceAnimation.AppendCallback(() => {
                                    isHitAnimationPlaying = true;
                                    spriteRenderer.material = darkMaterial;
                                }).Append(spriteRenderer.DOColor(hitObstacleColor, 0.075f))
                                .Append(spriteRenderer.DOColor(currentColor, 0.075f))
                                .Append(spriteRenderer.DOColor(hitObstacleColor, 0.075f))
                                .Append(spriteRenderer.DOColor(currentColor, 0.075f))
                                .Append(spriteRenderer.DOColor(hitObstacleColor, 0.075f))
                                .Append(spriteRenderer.DOColor(currentColor, 0.075f))
                                .AppendCallback(() => {
                                    isHitAnimationPlaying = false;
                                    spriteRenderer.material = currentMaterial;
                                    isHitObstacle = false;
                                });
        }

        private void UpdatePlayerHearts(bool isInit = false)
        {
            PlayerHeartsView playerHeartsView = GameManager.Singleton.ViewRegistry.TryGetValue(ViewName.PlayerHeartsView) as PlayerHeartsView;

            if (isInit) {
                playerHeartsView.SetPlayerHearts(playerHeartsCount);
            } else {
                playerHeartsView.LoseHeart();
            }
        }

        IEnumerator LevelLostAnimation()
        {
            isLevelLost = true;

            spriteRenderer.enabled = false;
            trail.SetActive(false);
            JumpIndicator.SetActive(false);
            boostIndicator.SetActive(false);

            GameManager.Singleton.FXManager.PlayerLoseFX.transform.position = gameObject.transform.position;
            GameManager.Singleton.FXManager.PlayerLoseFX.GetComponent<PlayerLoseFX>().Play();

            GameEventPublisher.PublishGameStateChange(GameState.LevelLost);

            gameObject.SetActive(false);
            
            yield return null;
        }

        IEnumerator WaitForPositionHelper()
        {
            isPositionHelperPlayingBack = true;
            yield return StartCoroutine(PlayerPositionHelper.WaitUntilFinished());
            isPositionHelperPlayingBack = false;
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.TryGetComponent<Node>(out Node Node)) {
                ActionController ActionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
                if (ActionController != null) ActionController.ManualOnTriggerEnter2D(Node);

                //publish that the players node ID changed
                if (playerNodeID != Node.ID) {
                    playerNodeID = Node.ID;
                    GameEventPublisher.PublishPlayerNodeChange(playerNodeID);
                }
            }

            if (collider.CompareTag("Surface")) {

                currentOffSurfaceTimer = offSurfaceTimer;
                surface = collider.gameObject.GetComponent<Surface>();
                if (isToggleColorOn) StartCoroutine(ToggleColor(collider.gameObject.GetInstanceID()));
                PlayerPositionHelper.SetRecording(false);

                movementDirection = previousMovementDirection;
                Move(true);    
                
                ActionController ActionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
                if (ActionController.ActionModel.CurrentEdge != null && ActionController.ActionModel.CurrentEdge.isOutOfBounds) {
                    StartCoroutine(ActionController.ActionModel.CurrentEdge.StretchTowardsPlayerAnimation());
                }            
                
                //$$jiggle the node
                //Vector3 dir = ((gameObject.transform.position - GameManager.Singleton.nearbyNode.gameObject.transform.position).normalized * 0.015f);
                //collider.gameObject.transform.DOPunchPosition(dir, 0.25f, 0, 0.25f, false);
                //collider.gameObject.transform.DOLocalMove(dir, 0.1f, false).OnComplete(() => {collider.gameObject.transform.DOLocalMove(-dir, 0.1f, false);});

            } else if (collider.CompareTag("Atmosphere")) {
                //CameraController.SetTarget(collider.gameObject.transform);
                PlayerPositionHelper.ResetPositions();
            }
            
            if (collider.CompareTag("Obstacle")) {
                //get the obstacle part
                ColorType obstacleColorType = collider.GetComponent<ObstaclePart>().colorType;
                //if the player is the the light color and so is the obstacle.. or if we're not enforcing light color and they are dark and so is the obstacle
                if ((isLightColor && obstacleColorType == ColorType.Light) 
                     || (!isEnforcingLightColor && (!isLightColor && obstacleColorType == ColorType.Dark))) {
                        
                    if (! lerping) {
                        GameEventPublisher.PublishPlayerHitCorrectObstacle();
                        AudioManager.Singleton.Play(SoundType.CorrectObstacleHit);
                    }

                //otherwise
                } else {
                    //make sure we didn't hit an obstacle while traveling because that doesn't count
                    if (! lerping) {

                        //use position helper to playback to safe point as long as player isn't in atmosphere or surface
                        if (!isLevelLost && ! isInAtmosphere && ! isOnSurface) {
                            PlayerPositionHelper.PlayBack();
                            StartCoroutine(WaitForPositionHelper());
                        }
                        
                        if (! isHitObstacle) {
                            HitObstacle();
                        }
                        
                    }
                }
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
            if (collider.TryGetComponent<Node>(out Node Node)) {
                ActionController ActionController = GameManager.Singleton.ControllerRegistry.TryGetValue(ViewName.ActionView) as ActionController;
                if (ActionController != null) ActionController.ManualOnTriggerExit2D(Node);
            }

            if (collider.CompareTag("Surface")) {
                isOnSurface = false;
                if (! lerping) {
                    PlayerPositionHelper.SetRecording(true);
                }

            } else if (collider.CompareTag("Atmosphere")) {
                isInAtmosphere = false;
                gravityDirection = Vector2.zero;
                surface = null;
            }
        }

        public void JumpOverride(Vector2 dir)
        {
            if (lerping) {
                return;
            }

            if (!isJumping && isOnSurface) {
                isJumping = true;
                isPowerJumping = true;
                Jump(dir);

            } else {
                dashDirection = dir;
            }
        }

        public void OnJoyStickDown()
        {
            isJoystickUp = false;
            joystickDownPoint = Input.mousePosition;
        }

        public void OnJoyStickUp()
        {
            isJoystickUp = true;
            joystickUpPoint  = Input.mousePosition;
        }

        protected void OnJoystickDirectionChange(Vector2 Direction)
        {
            Vector2 joystickDir = Direction;//joystickUpPoint - joystickDownPoint;
            
            if (isOnSurface) {
                movementDirection = -joystickDir;//$$(r.endPoint - r.startPoint).normalized;
            } else {
                previousMovementDirection = joystickDir;
            }

            SetRelativeForwardDirection();
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
            /*
             * Overriding gravity completely for new movement testing
             */
            gravityDirection = (GameManager.Singleton.nearbyNode.GetPosition() - (Vector2) transform.position).normalized;

            transform.up = - gravityDirection;
            rigidbody.AddForce(gravityDirection * (gravityForce * Time.fixedDeltaTime));
        }

        protected void Move(bool ignoreDuringLerp = false)
        {
            if (ignoreDuringLerp && lerping) {
                return;
            }
            //Vector2 nearbyNodePosition = (Vector2)GameManager.Singleton.player.transform.position;//$$Testing could make it like this if we wanted player to be able to just move on flat ground
            Vector2 nearbyNodePosition = (Vector2)GameManager.Singleton.nearbyNode.gameObject.transform.position;
            Vector2 target = nearbyNodePosition + movementDirection.normalized * mockNodeRadius;//$$TODOsget nearby surface gravity point, and get its distance from the nearby node and use that instead of mockNodeRadius.
            
            //rigidbody.DOMove(target, movementForce, false);
            //rigidbody.DOMove(nearbyNodePosition, movementForce, false);
            rigidbody.velocity = Vector2.zero;
            transform.position = nearbyNodePosition;
            
            AudioManager.Singleton.Play(SoundType.CellLand);
        }

        protected void ResetMovement()
        {
            if (currentOffSurfaceTimer > 0.0f) {
                currentOffSurfaceTimer -= Time.deltaTime;
            } else {
                movementDirection = Vector2.zero;
            }
        }

        protected void Jump(Vector2 dir)
        {
            AudioManager.Singleton.Play(SoundType.Jump);

            GameEventPublisher.PublishPlayerJump();

            ActionView actionView = GameManager.Singleton.ViewRegistry.TryGetValue(ViewName.ActionView) as ActionView;
            Vector2 joystickDir = actionView.LeanJoyStick.ScaledValue;

            float force = (isPowerJumping) ? powerJumpForce : jumpForce;
            //force += (!isJoystickUp) ? 2.0f : 0.0f;
            rigidbody.AddForce(dir * force, ForceMode2D.Impulse);
            didJump = true;
            isJumping = false;
            isPowerJumping = false;
        }

        protected void Jump()
        {
            ActionView actionView = GameManager.Singleton.ViewRegistry.TryGetValue(ViewName.ActionView) as ActionView;
            Vector2 joystickDir = actionView.LeanJoyStick.ScaledValue;

            float force = (isPowerJumping) ? powerJumpForce : jumpForce;
            rigidbody.AddForce(- gravityDirection * force, ForceMode2D.Impulse);
            didJump = true;
            isJumping = false;
            isPowerJumping = false;
        }

        protected void Dash()
        {
            if (canDash && currentOffSurfaceTimer <= 0.0f) {
                AudioManager.Singleton.Play(SoundType.Dash);
                GameEventPublisher.PublishPlayerDash();

                float dashForce;

                if (isInAtmosphere) {
                    dashForce = atmosphereDashForce;
                    didAtmosphereDash = true;
                } else {
                    dashForce = spaceDashForce;
                    didSpaceDash = true;
                }
                
                if (dashCount < extraDashCountAllowed) {
                    dashCount++;
                } else {
                    dashCount = 0;
                    canDash = false;
                }

                rigidbody.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
                transform.up = dashDirection;

                //set boost indication
                if (boostIndicator.activeSelf) {
                    //boostIndicator.GetComponent<Animator>().SetTrigger("Scale");
                    //float angle = Mathf.Atan2(-dashDirection.y, -dashDirection.x) * Mathf.Rad2Deg;
                    //boostIndicator.transform.parent.rotation = Quaternion.Euler(0, 0, angle);

                    // Calculate the angle between the direction and the X axis
                    ActionView actionView = GameManager.Singleton.ViewRegistry.TryGetValue(ViewName.ActionView) as ActionView;
                    float angle = Mathf.Atan2(actionView.JumpJoyStick.ScaledValue.y, actionView.JumpJoyStick.ScaledValue.x) * Mathf.Rad2Deg;

                    // Rotate the object around the Z axis to match the direction
                    //boostIndicator.transform.rotation = Quaternion.Euler(0, 0, angle + 90.0f);
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
            } else if (! isInAtmosphere && ! isOnSurface && didSpaceDash && dashCount >= extraDashCountAllowed) {
                canDash = false;
            
            //simply in space
            } else if (! isInAtmosphere && ! isOnSurface) {
                canDash = true;
                didJump = false;
            }
        }
    }

}