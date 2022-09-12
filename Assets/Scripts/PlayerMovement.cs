using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Prime31;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed=10f, jumpPower=10f;
    public SpriteRenderer sprite;
    protected float distanceThreshold = 0.2f;

    protected Collider2D collider;
    protected GravityPoint planet;
    protected Rigidbody2D body;
    public bool isGrounded;
    protected float horizontal;
    protected bool isLongPressed;
    protected TKTapRecognizer TapRecognizer;
    protected TKLongPressRecognizer LongPressRecognizer;
    protected TKSwipeRecognizer SwipeRecognizer;
    protected bool isJump;
    protected int isMoving;
    protected Vector2 swipeDirectionalVector = Vector2.zero;
    protected bool canDash;

    void Awake()
    {
        TapRecognizer = new TKTapRecognizer();
        LongPressRecognizer = new TKLongPressRecognizer();
        SwipeRecognizer = new TKSwipeRecognizer();
    }

    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        

        if (isGrounded)
        {
            canDash = true;
            /*if (Input.GetButtonDown("Jump")) {

                body.AddForce(transform.up * jumpPower, ForceMode2D.Impulse);

            }*/

            if (isJump) {

                body.AddForce(transform.up * jumpPower, ForceMode2D.Impulse);

            }

            /*if (Input.GetAxisRaw("Horizontal") != 0) {

                horizontal = Input.GetAxisRaw("Horizontal");

            } else {
                horizontal = 0;
            }*/
            
            
            if (isMoving != 0) {

                horizontal = isMoving;

            } else {
                horizontal = 0;
            }
            
        //not grounded
        } else {
            if (isMoving != 0) {
                isMoving = 0;
                horizontal = isMoving;
            }

            if (swipeDirectionalVector != Vector2.zero) {
                Dash();
                swipeDirectionalVector = Vector2.zero;
            }


        }

        if (isJump) {
            isJump = false;
        }
    }

    void FixedUpdate()
    {
        if (isGrounded)
        {
            body.AddForce(transform.right * horizontal * moveSpeed);
        }
        sprite.flipX = horizontal > 0 ? false : (horizontal < 0 ? true : sprite.flipX);
    }

    void OnTriggerEnter2D(Collider2D obj)
    {
        if (obj.CompareTag("GravityArea")) {
            planet = obj.GetComponent<GravityPoint>();
        }
        
    }

    void OnTriggerStay2D(Collider2D obj)
    {
        if (obj.CompareTag("GravityArea"))
        {

            if (planet.gameObject.GetInstanceID() != obj.gameObject.GetInstanceID()) {
                planet = obj.GetComponent<GravityPoint>();
            }
            body.drag = 1f;

            float distance = Mathf.Abs(Vector2.Distance(planet.surface.transform.position, transform.position));
            if (distance - distanceThreshold <= planet.planetRadius)
            {
                isGrounded = true;

            } else {
                isGrounded = false;
            }
        }
    }

    void OnTriggerExit2D(Collider2D obj)
    {
        if (obj.CompareTag("GravityArea"))
        {
            body.drag = 0.2f;
        }
    }

    protected void OnLongPressBegin(TKLongPressRecognizer r)
    {
        //isMoving = 0;
    }

    protected void OnLongPressEnd(TKLongPressRecognizer r)
    {
        //isLongPressed = false;
    }

    protected void OnTap(TKTapRecognizer r) 
    {
        isJump = true;
    }

    protected void OnSwipe(TKSwipeRecognizer r)
    {
        switch (r.completedSwipeDirection) {
            case TKSwipeDirection.Up :
                //isJump = true;
                break;
            case TKSwipeDirection.Right :
                isMoving = 1;
                break;
            case TKSwipeDirection.Left :
                isMoving = -1;
                break;
        }

        if (! isGrounded) {
            swipeDirectionalVector = (r.endPoint - r.startPoint).normalized;
        }
    }

    protected void Dash()
    {
        if (canDash) {
            body.AddForce(swipeDirectionalVector * (jumpPower / 2), ForceMode2D.Impulse);
            canDash = false;
        }
    }

    void OnEnable()
    {
        LongPressRecognizer.gestureRecognizedEvent += OnLongPressBegin;
        //LongPressRecognizer.gestureCompleteEvent += OnLongPressEnd;
        LongPressRecognizer.allowableMovementCm = 5;
        TouchKit.addGestureRecognizer(LongPressRecognizer);

        TapRecognizer.gestureRecognizedEvent += OnTap;
        TouchKit.addGestureRecognizer(TapRecognizer);

        SwipeRecognizer.gestureRecognizedEvent += OnSwipe;
        SwipeRecognizer.timeToSwipe = 0.0f;
        TouchKit.addGestureRecognizer(SwipeRecognizer);
    }

    void OnDisable()
    {
        LongPressRecognizer.gestureRecognizedEvent -= OnLongPressBegin;
        //LongPressRecognizer.gestureCompleteEvent -= OnLongPressEnd;
        
        SwipeRecognizer.gestureRecognizedEvent -= OnSwipe;
        
        TapRecognizer.gestureRecognizedEvent -= OnTap;
    }

}