﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 10f;
    public float jumpMultiplier = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2.5f;

    public Animator m_Animator = null;

    private Rigidbody   m_Rigidbody = null;
    private Vector3     m_HorizontalDirection = Vector3.zero;
    private Vector3     m_VerticalDirection = Vector3.zero;
    private float       m_DistToGround = 0f;
    private float       m_DistToSide = 0f;
    private bool        m_ShouldBeDragged = false;
    private bool        m_IsGrippingWall = false;
    private bool        m_IsDraggedDown = false;
    private bool        m_IsDashing = false;
    private int         m_AvailableJumps = 1;

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_DistToGround = GetComponent<Collider>().bounds.extents.y;
        m_DistToSide = GetComponent<Collider>().bounds.extents.x;
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if(m_Rigidbody.velocity.y != 0 && !m_IsDraggedDown)
        {
            StartCoroutine(DragDownCoroutine());
        }
    }

    public void MoveHorizontal(float iInputValue)
    {
        if(!m_IsDashing)
        {
            //on ne se déplace que si on n'est pas collé à un mur.
            m_HorizontalDirection = iInputValue * Vector3.right * Time.deltaTime;
            if (!HasGripOnWall(m_HorizontalDirection))
            {
                transform.Translate(m_HorizontalDirection * speed);
            }
            //sinon, on s'y accroche.
            else if (!m_IsGrippingWall)
            {
                StartCoroutine(GrippingWallCoroutine(Vector3.Normalize(m_HorizontalDirection)));
            }
        }
        
            
    }

    public void MoveRight(float iInputValue)
    {
        //m_VerticalDirection = iInputValue * transform.right * Time.deltaTime;
        //transform.Translate(m_VerticalDirection * speed);
        if (!m_IsDashing && iInputValue != 0)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    public void Fire()
    {
        
    }

    public void Jump()
    {
        if (!m_IsDashing)
        {
            //if (IsGrounded() || m_IsGrippingWall)
            if (m_AvailableJumps > 0)
            {
                Vector3 direction = m_VerticalDirection + m_HorizontalDirection + Vector3.up;
                m_Rigidbody.velocity = (direction * jumpMultiplier);
                SpendJump();
            }
        }
    }

    public void Dash()
    {
        
    }

    private void SpendJump()
    {
        m_AvailableJumps = (int)Mathf.Clamp01(m_AvailableJumps - 1);
    }

    private void GiveJump()
    {
        m_AvailableJumps = (int)Mathf.Clamp01(m_AvailableJumps + 1);
    }

    private IEnumerator DashCoroutine()
    {
        m_IsDashing = true;
        m_Rigidbody.isKinematic = true;
        GiveJump();

        yield return MoveToPosition(transform.position + transform.right, .1f);

        m_IsDashing = false;
        m_Rigidbody.isKinematic = false;
    }

    private IEnumerator MoveToPosition(Vector3 iNewPos, float iTime)
    {
        float elapsedTime = 0;
        Vector3 startingPos = transform.position;
        while (elapsedTime < iTime)
        {
            transform.position = Vector3.Lerp(startingPos, iNewPos, (elapsedTime / iTime));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    private bool IsInAir()
    {
        return !(IsGrounded() || m_IsGrippingWall);
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -transform.up, m_DistToGround + .05f);
    }

    private bool HasGripOnWall(Vector3 iDirection)
    {
        bool isGoingRight = Vector3.Dot(iDirection, Vector3.right) > 0;

        bool hasRightGrip = Physics.Raycast(transform.position, Vector3.right, m_DistToSide + .05f);
        bool hasLeftGrip = Physics.Raycast(transform.position, -Vector3.right, m_DistToSide + .05f);

        return (isGoingRight) ? hasRightGrip : hasLeftGrip;
    }

    private IEnumerator DragDownCoroutine()
    {
        m_IsDraggedDown = true;
        Vector3 dragVector = Vector3.zero;

        while (m_ShouldBeDragged)
        {
            yield return new WaitForEndOfFrame();
            //tweak de l'accélération en chute
            if (m_Rigidbody.velocity.y < 0)
            {
                dragVector = Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            //tweak de la décélération en montée
            else if (m_Rigidbody.velocity.y > 0)
            {
                dragVector = Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            }
            m_Rigidbody.velocity += dragVector;
        }

        m_IsDraggedDown = false;
    }

    private IEnumerator GrippingWallCoroutine(Vector3 iGrippingDirection)
    {
        m_IsGrippingWall = true;
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.isKinematic = true;

        //tant que le direction dans laquelle on se déplace va dans le même sens la direction dans laquelle on s'est aggripé,
        //on coupe le joueur du moteur physique.
        while (Vector3.Dot(iGrippingDirection, m_HorizontalDirection) > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        m_Rigidbody.isKinematic = false;
        m_ShouldBeDragged = false;
        //Pour laisser un peu de temps au joueur de sauter après avoir lâché un mur.
        yield return new WaitForSeconds(.2f);
        m_ShouldBeDragged = true;
        m_IsGrippingWall = false;
        yield return StartCoroutine(DragDownCoroutine());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("SolidEnvironment"))
        {
            m_ShouldBeDragged = false;
            GiveJump();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("SolidEnvironment"))
        {
            m_ShouldBeDragged = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("SolidEnvironment"))
        {
            m_ShouldBeDragged = true;
        }
    }
}
