using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 10f;

    private Rigidbody m_Rigidbody = null;
    private Vector3 m_HorizontalDirection = Vector3.zero;
    private Vector3 m_VerticalDirection = Vector3.zero;
    private float distToGround = 0f;

    private void Start()
    {
        distToGround = GetComponent<Collider>().bounds.extents.y;
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public void MoveHorizontal(float iInputValue)
    {
        m_HorizontalDirection = iInputValue * transform.forward * Time.deltaTime;
        transform.Translate(m_HorizontalDirection * speed);
    }

    public void MoveRight(float iInputValue)
    {
        //m_VerticalDirection = iInputValue * transform.right * Time.deltaTime;
        //transform.Translate(m_VerticalDirection * speed);
    }

    public void Fire()
    {
        
    }

    public void Jump()
    {
        Debug.Log(IsGrounded());
        if(IsGrounded())
        {
            Vector3 direction = m_VerticalDirection + m_HorizontalDirection + transform.up * 10;
            m_Rigidbody.velocity = (direction);
        }
        
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -transform.up, distToGround + .05f);
    }
}
