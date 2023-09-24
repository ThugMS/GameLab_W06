//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class PlayerMovement : MonoBehaviour
//{
//    [Header("Movement")]
//    private float moveSpeed;
//    public float walkSpeed;
//    public float sprintSpeed;
//    public float slideSpeed;

//    private float desiredMoveSpeed;
//    private float lastDesiredMoveSpeed;

//    public float speedIncreaseMultiplier;
//    public float slopeIncreaseMultiplier;

//    public float groundDrag;

//    [Header("Jumping")]
//    public float jumpForce;
//    public float jumpCooldown;
//    public float airMultiplier;
//    bool readyToJump;

//    [Header("Crouching")]
//    public float crouchSpeed;
//    public float crouchYScale;
//    private float startYScale;

//    [Header("Keybinds")]
//    public KeyCode jumpKey = KeyCode.Space;
//    public KeyCode sprintKey = KeyCode.LeftShift;
//    public KeyCode crouchKey = KeyCode.LeftControl;

//    [Header("Ground Check")]
//    public float playerHeight;
//    public LayerMask whatIsGround;
//    bool grounded;

//    [Header("Slope Handling")]
//    public float maxSlopeAngle;
//    private RaycastHit slopeHit;
//    private bool exitingSlope;


//    public Transform orientation;

//    float horizontalInput;
//    float verticalInput;

//    Vector3 moveDirection;

//    Rigidbody rb;

//    public MovementState state;
//    public enum MovementState
//    {
//        walking,
//        sprinting,
//        crouching,
//        sliding,
//        air
//    }

//    public bool sliding;

//    private void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        rb.freezeRotation = true;

//        readyToJump = true;

//        startYScale = transform.localScale.y;
//    }

//    private void Update()
//    {
//        // ground check
//        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

//        MyInput();
//        SpeedControl();
//        StateHandler();

//        // handle drag
//        if (grounded)
//            rb.drag = groundDrag;
//        else
//            rb.drag = 0;
//    }

//    private void FixedUpdate()
//    {
//        MovePlayer();
//    }

//    private void MyInput()
//    {
//        horizontalInput = Input.GetAxisRaw("Horizontal");
//        verticalInput = Input.GetAxisRaw("Vertical");

//        // when to jump
//        if (Input.GetKey(jumpKey) && readyToJump && grounded)
//        {
//            readyToJump = false;

//            Jump();

//            Invoke(nameof(ResetJump), jumpCooldown);
//        }

//        // start crouch
//        if (Input.GetKeyDown(crouchKey))
//        {
//            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
//            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
//        }

//        // stop crouch
//        if (Input.GetKeyUp(crouchKey))
//        {
//            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
//        }
//    }

//    private void StateHandler()
//    {
//        // Mode - Sliding
//        if (sliding)
//        {
//            state = MovementState.sliding;

//            if (OnSlope() && rb.velocity.y < 0.1f)
//                desiredMoveSpeed = slideSpeed;

//            else
//                desiredMoveSpeed = sprintSpeed;
//        }

//        // Mode - Crouching
//        else if (Input.GetKey(crouchKey))
//        {
//            state = MovementState.crouching;
//            desiredMoveSpeed = crouchSpeed;
//        }

//        // Mode - Sprinting
//        else if (grounded && Input.GetKey(sprintKey))
//        {
//            state = MovementState.sprinting;
//            desiredMoveSpeed = sprintSpeed;
//        }

//        // Mode - Walking
//        else if (grounded)
//        {
//            state = MovementState.walking;
//            desiredMoveSpeed = walkSpeed;
//        }

//        // Mode - Air
//        else
//        {
//            state = MovementState.air;
//        }

//        // check if desiredMoveSpeed has changed drastically
//        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
//        {
//            StopAllCoroutines();
//            StartCoroutine(SmoothlyLerpMoveSpeed());
//        }
//        else
//        {
//            moveSpeed = desiredMoveSpeed;
//        }

//        lastDesiredMoveSpeed = desiredMoveSpeed;
//    }

//    private IEnumerator SmoothlyLerpMoveSpeed()
//    {
//        // smoothly lerp movementSpeed to desired value
//        float time = 0;
//        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
//        float startValue = moveSpeed;

//        while (time < difference)
//        {
//            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

//            if (OnSlope())
//            {
//                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
//                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

//                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
//            }
//            else
//                time += Time.deltaTime * speedIncreaseMultiplier;

//            yield return null;
//        }

//        moveSpeed = desiredMoveSpeed;
//    }

//    private void MovePlayer()
//    {
//        // calculate movement direction
//        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

//        // on slope
//        if (OnSlope() && !exitingSlope)
//        {
//            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

//            if (rb.velocity.y > 0)
//                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
//        }

//        // on ground
//        else if (grounded)
//            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

//        // in air
//        else if (!grounded)
//            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

//        // turn gravity off while on slope
//        rb.useGravity = !OnSlope();
//    }

//    private void SpeedControl()
//    {
//        // limiting speed on slope
//        if (OnSlope() && !exitingSlope)
//        {
//            if (rb.velocity.magnitude > moveSpeed)
//                rb.velocity = rb.velocity.normalized * moveSpeed;
//        }

//        // limiting speed on ground or in air
//        else
//        {
//            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

//            // limit velocity if needed
//            if (flatVel.magnitude > moveSpeed)
//            {
//                Vector3 limitedVel = flatVel.normalized * moveSpeed;
//                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
//            }
//        }
//    }

//    private void Jump()
//    {
//        exitingSlope = true;

//        // reset y velocity
//        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

//        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
//    }
//    private void ResetJump()
//    {
//        readyToJump = true;

//        exitingSlope = false;
//    }

//    public bool OnSlope()
//    {
//        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
//        {
//            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
//            return angle < maxSlopeAngle && angle != 0;
//        }

//        return false;
//    }

//    public Vector3 GetSlopeMoveDirection(Vector3 direction)
//    {
//        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
//    }
//}

using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public enum MOVEMENT_STATE
    {
        Freeze, Walking, Sprinting, Swinging, Crouching, Sliding, Air
    }

    #region PublicVariables
    public float m_groundDrag;
    public bool m_isSwinging;
    public bool m_isSwingPressed = false;
    public MOVEMENT_STATE m_movementState;
    public bool m_isFreeze = false;
    public bool m_actvieGrapple = false;
    #endregion

    #region PrivateVariables
    [Header("Movement")]
    [SerializeField] private float m_moveSpeed;
    [SerializeField] private float m_walkSpeed;
    [SerializeField] private float m_sprintSpeed;
    [SerializeField] private float m_slideSpeed;
    [SerializeField] private float m_swingSpeed;

    private float m_desiredMoveSpeed;
    private float m_lastDesireMoveSpeed;

    [SerializeField] private Transform m_orientation;
    private Vector3 m_moveDir;

    [Header("Ground Check")]
    [SerializeField] private float m_playerHeight;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private bool m_isGround = false;

    [Header("Jump")]
    [SerializeField] private float m_jumpForce;
    [SerializeField] private float m_jumpCooldown;
    [SerializeField] private float m_airMultiplier;
    [SerializeField] private bool m_readyJump;

    [Header("Crouch")]
    [SerializeField] private float m_crouchSpeed;
    [SerializeField] private float m_crouchYScale;
    [SerializeField] private float m_startYScale;

    [Header("Sliding")]
    [SerializeField] private float m_slidingSpeed;
    [SerializeField] private bool m_isSliding = false;
    [SerializeField] private float m_maxSlideTime;
    [SerializeField] private float m_curSlideTime;

    [Header("SlopeHandling")]
    [SerializeField] public float m_maxSlopeAngle;
    private RaycastHit m_slopeHit;
    private bool exitSlope;

    [Header("KeyBinds")]
    [SerializeField] private KeyCode m_jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode m_sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode m_crouchKey = KeyCode.LeftControl;

    [Header("Grapple")]
    private Vector3 m_velocityToSet;
    private bool m_enableMoveOnNextTouch;

    private float m_horizontalInput;
    private float m_verticalInput;

    private Rigidbody m_rigidbody;
    [SerializeField] private TextMeshProUGUI m_speedText;
    #endregion

    #region PublicMethod
    private void Start()
    {
        TryGetComponent<Rigidbody>(out m_rigidbody);
        m_rigidbody.freezeRotation = true;

        m_readyJump = true;
        m_startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // ground check
        m_isGround = Physics.Raycast(transform.position, Vector3.down, m_playerHeight * 0.5f + 0.2f, m_groundLayer);

        if (m_isGround == true && m_isSwingPressed == false && m_isSwinging == true)
        {
            m_isSwinging = false;
        }

        CallInput();
        ControlSpeed();
        StateHandler();    

        // drag handle
        if (m_isGround == true && !m_actvieGrapple)
        {
            m_rigidbody.drag = m_groundDrag;
        }
        else
        {
            m_rigidbody.drag = 0;
        }

        m_speedText.text = "Speed  :  " + Mathf.Round(m_rigidbody.velocity.magnitude);
      }

    private void FixedUpdate()
    {
        MovePlayer();

        if(m_isSliding == true)
        {
            SlidingMovement();
        }

    }

    public void JumpToPosition(Vector3 _targetPosition, float _trajectoryHeight)
    {
        m_actvieGrapple = true;

        m_velocityToSet = CalculateJumpVelocity(transform.position, _targetPosition, _trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }

    public void ResetRestrictions()
    {
        m_actvieGrapple = false;
    }
    #endregion

    #region PrivateMethod
    private void StateHandler()
    {   
        if(m_isFreeze == true)
        {
            m_movementState = MOVEMENT_STATE.Freeze;
            m_desiredMoveSpeed = 0;
            m_rigidbody.velocity = Vector3.zero;
        }
        else if (m_isSliding == true)
        {
            m_movementState = MOVEMENT_STATE.Sliding;

            if (OnSlope() && m_rigidbody.velocity.y < 0.1f)
            {
                if (m_rigidbody.velocity.magnitude > m_slidingSpeed)
                    m_desiredMoveSpeed = m_slideSpeed;
                else
                    m_desiredMoveSpeed = m_slidingSpeed;
            }
            else
            {
                m_desiredMoveSpeed = m_sprintSpeed;
            }
        }
        // Mode - Swinging
        else if(m_isSwinging == true)
        {
            m_movementState = MOVEMENT_STATE.Swinging;
            m_desiredMoveSpeed = m_swingSpeed;
        }
        // Mode - Crouching
        else if (Input.GetKey(m_crouchKey))
        {
            m_movementState = MOVEMENT_STATE.Crouching;
            m_desiredMoveSpeed = m_crouchSpeed;
        }
        // Mode - Sprinting
        else if (m_isGround == true && Input.GetKey(m_sprintKey))
        {
            m_movementState = MOVEMENT_STATE.Sprinting;
            m_desiredMoveSpeed = m_sprintSpeed;
        }
        // Mode - Walking
        else if (m_isGround == true)
        {
            m_movementState = MOVEMENT_STATE.Walking;
            m_desiredMoveSpeed = m_walkSpeed;
        }
        //Mode - Air
        else
        {
            m_movementState = MOVEMENT_STATE.Air;
        }

        m_moveSpeed = m_desiredMoveSpeed;

        // check if desireMoveSpeed has changed drastically
        //if (Mathf.Abs(m_desiredMoveSpeed - m_lastDesireMoveSpeed) > 4f && m_moveSpeed != 0)
        //{
        //    StopAllCoroutines();
        //    StartCoroutine(SmoothlyLerpMoveSpeed());
        //}
        //else
        //{
        //    m_moveSpeed = m_desiredMoveSpeed;
        //}

        m_lastDesireMoveSpeed = m_desiredMoveSpeed;
}
private void CallInput()
    {
        m_horizontalInput = Input.GetAxisRaw("Horizontal");
        m_verticalInput = Input.GetAxisRaw("Vertical");

        // when jump key pressed
        if (Input.GetKey(m_jumpKey) && m_readyJump && m_isGround)
        {
            m_readyJump = false;

            Jump();

            Invoke(nameof(ResetJump), m_jumpCooldown);
        }

        if (Input.GetKeyDown(m_crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, m_crouchYScale, transform.localScale.z);
            m_rigidbody.AddForce(Vector3.down * 10f, ForceMode.Impulse);

            if (m_rigidbody.velocity.magnitude >= 6)
            {
                StartSlide();
            }
        }

        if (Input.GetKeyUp(m_crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, m_startYScale, transform.localScale.z);

            if(m_isSliding == true)
            {
                StopSlide();
            }
        }

    }

    private void MovePlayer()
    {
        if (m_isSwinging == true)
            return;

        // calculate direction
        m_moveDir = m_orientation.forward * m_verticalInput + m_orientation.right * m_horizontalInput;

        // on slope
        if (OnSlope() == true && !exitSlope)
        {
            m_rigidbody.AddForce(GetSlopeMoveDirection(m_moveDir) * m_moveSpeed * 20f, ForceMode.Force);

            if (m_rigidbody.velocity.y > 0)
            {
                m_rigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        // on ground
        else if (m_isGround == true)
        {
            m_rigidbody.AddForce(m_moveDir.normalized * m_moveSpeed * 10f , ForceMode.Force);
        }
        // in air
        else if (m_isGround == false)
        {
            m_rigidbody.AddForce(m_moveDir.normalized * m_moveSpeed * 10f * m_airMultiplier, ForceMode.Force);
        }

        // turn gravity off while on slope
        m_rigidbody.useGravity = !OnSlope();
    }

    private void StartSlide()
    {
        m_isSliding = true;

        m_curSlideTime = m_maxSlideTime;
    }

    private void StopSlide()
    {
        m_isSliding = false;
    }

    private void SlidingMovement()
    {
        Vector3 inputDir = m_orientation.forward * m_verticalInput + m_orientation.right * m_horizontalInput;

        // sliding normal
        if (!OnSlope() || m_rigidbody.velocity.y > -0.1f)
        {
            m_rigidbody.AddForce(inputDir.normalized * m_slidingSpeed, ForceMode.Force);

            m_curSlideTime -= Time.deltaTime;
        }
        // sliding slope
        else
        {
            m_rigidbody.AddForce(GetSlopeMoveDirection(inputDir) * m_slidingSpeed, ForceMode.Force);
        }

        if(m_curSlideTime < 0)
        {
            StopSlide();
        }
    }

    private void ControlSpeed()
    {
        if (m_actvieGrapple == true)
            return;
        // limiting speed on slope
        if (OnSlope() && !exitSlope)
        {
            if (m_rigidbody.velocity.magnitude > m_moveSpeed)
            {
                m_rigidbody.velocity = m_rigidbody.velocity.normalized * m_moveSpeed;
            }
        }
        // limiting speed on ground or in air
        else
        {
            Vector3 flatVelocity = new Vector3(m_rigidbody.velocity.x, 0f, m_rigidbody.velocity.z);
            if(m_isSliding == true)
            {
                Vector3 limitVelocity = flatVelocity.normalized * m_slidingSpeed;
                m_rigidbody.velocity = new Vector3(limitVelocity.x, m_rigidbody.velocity.y, limitVelocity.z);
            }
            else if (flatVelocity.magnitude > m_moveSpeed)
            {   
                if(m_isSwinging == false)
                {
                    Vector3 limitVelocity = flatVelocity.normalized * m_moveSpeed;
                    m_rigidbody.velocity = new Vector3(limitVelocity.x, m_rigidbody.velocity.y, limitVelocity.z);
                }
            }
        }
    }

    

    private void SetVelocity()
    {
        m_enableMoveOnNextTouch = true;
        m_rigidbody.velocity = m_velocityToSet;
    }

    private void Jump()
    {
        exitSlope = true;

        // reset y velocity
        m_rigidbody.velocity = new Vector3(m_rigidbody.velocity.x, 0f, m_rigidbody.velocity.z);

        m_rigidbody.AddForce(transform.up * m_jumpForce, ForceMode.Impulse);
    }

    private bool OnSlope()
    {
        // if you on slope
        if (Physics.Raycast(transform.position, Vector3.down, out m_slopeHit, m_playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, m_slopeHit.normal);
            return angle < m_maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection(Vector3 _direciton)
    {
        return Vector3.ProjectOnPlane(_direciton, m_slopeHit.normal).normalized;
    }

    private void ResetJump()
    {
        m_readyJump = true;

        exitSlope = false;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float diff = Mathf.Abs(m_desiredMoveSpeed - m_moveSpeed);
        float startValue = m_moveSpeed;

        while (time < diff)
        {
            m_moveSpeed = Mathf.Lerp(startValue, m_desiredMoveSpeed, time / diff  * 3);
            time += Time.deltaTime;
            yield return null;
        }

        m_moveSpeed = m_desiredMoveSpeed;
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(m_enableMoveOnNextTouch == true)
        {
            m_enableMoveOnNextTouch = false;
            ResetRestrictions();

            GetComponent<Grappling>().StopGrapple();
        }
    }
    #endregion
}
