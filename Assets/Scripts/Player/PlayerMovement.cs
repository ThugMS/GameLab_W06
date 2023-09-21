using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{   
    public enum MOVEMENT_STATE
    {
        Walking, Sprinting, Crouching, Air
    }

    #region PublicVariables
    public float m_groundDrag;

    public MOVEMENT_STATE m_movementState;
    #endregion

    #region PrivateVariables
    [Header("Movement")]
    private float m_moveSpeed;
    [SerializeField] private float m_walkSpeed;
    [SerializeField] private float m_sprintSpeed;

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

    [Header("SlopeHandling")]
    [SerializeField] public float m_maxSlopeAngle;
    private RaycastHit m_slopeHit;

    [Header("KeyBinds")]
    [SerializeField] private KeyCode m_jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode m_sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode m_crouchKey = KeyCode.LeftControl;

    private float m_horizontalInput;
    private float m_verticalInput;

    

    private Rigidbody m_rigidbody;
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

        CallInput();
        ControlSpeed();
        StateHandler();

        // drag handle
        if(m_isGround == true)
        {
            m_rigidbody.drag = m_groundDrag;
        }
        else
        {
            m_rigidbody.drag = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }
    #endregion

    #region PrivateMethod
    private void StateHandler()
    {
        // Mode - Sprinting
        if(m_isGround == true && Input.GetKey(m_sprintKey))
        {
            m_movementState = MOVEMENT_STATE.Sprinting;
            m_moveSpeed = m_sprintSpeed;
        }
        // Mode - Walking
        else if (m_isGround == true)
        {
            m_movementState = MOVEMENT_STATE.Walking;
            m_moveSpeed = m_walkSpeed;
        }
        // Mode - Crouching
        else if (Input.GetKey(m_crouchKey))
        {
            m_movementState |= MOVEMENT_STATE.Crouching;
            m_moveSpeed = m_crouchSpeed;

        }
        //Mode - Air
        else
        {
            m_movementState = MOVEMENT_STATE.Air;
        }
    }
    private void CallInput()
    {
        m_horizontalInput = Input.GetAxisRaw("Horizontal");
        m_verticalInput = Input.GetAxisRaw("Vertical");

        // when jump key pressed
        if(Input.GetKey(m_jumpKey) && m_readyJump && m_isGround)
        {
            m_readyJump = false;

            Jump();

            Invoke(nameof(ResetJump), m_jumpCooldown);
        }

        if (Input.GetKeyDown(m_crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, m_crouchYScale, transform.localScale.z);
            m_rigidbody.AddForce(Vector3.down * 10f, ForceMode.Impulse);
        }

        if (Input.GetKeyUp(m_crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, m_startYScale, transform.localScale.z);
        }
    }

    private void MovePlayer()
    {
        // calculate direction
        m_moveDir = m_orientation.forward * m_verticalInput + m_orientation.right * m_horizontalInput;

        if(m_isGround == true)
        {
            m_rigidbody.AddForce(m_moveDir.normalized * m_moveSpeed * 10f, ForceMode.Force);
        }
        else if(m_isGround == false)
        {
            m_rigidbody.AddForce(m_moveDir.normalized * m_moveSpeed * 10f * m_airMultiplier, ForceMode.Force);
        }
    }

    private void ControlSpeed()
    {
        Vector3 flatVelocity = new Vector3(m_rigidbody.velocity.x, 0f, m_rigidbody.velocity.z);

        // limit velocity if needed
        if(flatVelocity.magnitude > m_moveSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * m_moveSpeed;
            m_rigidbody.velocity = new Vector3(limitedVelocity.x, m_rigidbody.velocity.y, limitedVelocity.z);
        }
    }

    private void Jump()
    {
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

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(m_moveDir, m_slopeHit.normal).normalized;
    }

    private void ResetJump()
    {
        m_readyJump = true;
    }
    #endregion
}
