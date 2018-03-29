using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 10f;
    public float jumpMultiplier = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2.5f;
    public float floattingTimeAfterGrip = .1f;
    public float dashCooldown = 1f;

    public float arrowSpeed = 10f;
    public float arrowOffset = 2f;
    public float arrowDisplayTime = 0.2f;
    public Rigidbody arrow;

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
    private bool        m_KeepInAir = false;
    private int         m_AvailableJumps = 1;
    private int         m_availableDashs = 1;
    private bool        m_FacingRight = true;
    private float       m_Dot = 0;
    private List<Transform> m_currentColliders = new List<Transform>();

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_DistToGround = GetComponent<Collider>().bounds.extents.y;
        m_DistToSide = GetComponent<Collider>().bounds.extents.x;
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!m_KeepInAir)
        { 
            if (m_Rigidbody.velocity.y != 0 && !m_IsDraggedDown)
            {
                StartCoroutine(DragDownCoroutine());
            }
        }
        else
        {
            if(m_Rigidbody.velocity.y < 0)
            {
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, 0, m_Rigidbody.velocity.z);
            }
            
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
            else if(!m_IsGrippingWall)
            {
                StartCoroutine(GrippingWallCoroutine(Vector3.Normalize(m_HorizontalDirection)));
            }
        }
        if(iInputValue > 0 && !m_FacingRight){
            print("droite");
            Flip();
        }
        else if(iInputValue < 0 && m_FacingRight){
            print("gauche");
            Flip();
        }
            
    }

    public void MoveRight(float iInputValue)
    {
        //m_VerticalDirection = iInputValue * transform.right * Time.deltaTime;
        //transform.Translate(m_VerticalDirection * speed);
        if (!m_IsDashing && m_availableDashs > 0 && iInputValue != 0)
        {
            StartCoroutine(DashCoroutine());
            StartCoroutine(DashCooldownCoroutine());
        }
    }

    public void Fire()
    {
         if(m_FacingRight)
            {
                // ... instantiate the rocket facing right and set it's velocity to the right. 
                Rigidbody bulletInstance = Instantiate(arrow, new Vector3(transform.position.x + arrowOffset, transform.position.y, transform.position.z), Quaternion.Euler(new Vector3(0,0,0))) as Rigidbody;
                bulletInstance.velocity = new Vector3(arrowSpeed, 0, 0);
            }
            else
            {
                // Otherwise instantiate the rocket facing left and set it's velocity to the left.
                Rigidbody bulletInstance = Instantiate(arrow, new Vector3(transform.position.x-arrowOffset,transform.position.y,transform.position.z), Quaternion.Euler(new Vector3(0,0,180f))) as Rigidbody;
                bulletInstance.velocity = new Vector3(-arrowSpeed, 0, 0);
            }
    }

    public void Jump()
    {
        if (!m_IsDashing)
        {
            if (m_AvailableJumps > 0)
            {
                
                Vector3 direction = m_VerticalDirection + m_HorizontalDirection + Vector3.up;
                m_Rigidbody.velocity = (direction * jumpMultiplier);

                //si on a la place de sauter, on le dépense ce saut de merde.
                if (!Physics.Raycast(transform.position, Vector3.up, m_DistToSide + .01f))
                {
                    SpendJump();
                }

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

    private void SpendDash()
    {
        m_availableDashs = (int)Mathf.Clamp01(m_availableDashs - 1);
    }

    private void GiveDash()
    {
        m_availableDashs = (int)Mathf.Clamp01(m_availableDashs + 1);
    }

    private IEnumerator DashCoroutine()
    {
        m_IsDashing = true;
        //m_Rigidbody.isKinematic = true;
        m_Rigidbody.useGravity = false;
        GiveJump();
        SpendDash();
        Vector3 startVelocity = m_Rigidbody.velocity;
        Vector3 direction = (m_FacingRight) ? Vector3.right : -Vector3.right;
        //yield return StartCoroutine(MoveToPosition(transform.position + transform.right * 1.5f, .05f));
        //yield return StartCoroutine(MoveToPosition(transform.position + transform.right * 3.5f, .1f));
        yield return StartCoroutine(LerpVelocityTo(startVelocity + direction * 50f, startVelocity + direction * 30f, .05f));
        yield return StartCoroutine(LerpVelocityTo(startVelocity + direction * 30f, startVelocity + direction * 0f, .1f));
        //yield return StartCoroutine(LerpVelocityTo(transform.position + transform.right * 3.5f, .1f));

        //m_Rigidbody.AddForce(transform.forward * 10);
        //yield return new WaitForSeconds(.5f);


        m_IsDashing = false;
        //m_Rigidbody.isKinematic = false;
        m_Rigidbody.useGravity = true;
    }

    private IEnumerator DashCooldownCoroutine()
    {
        yield return new WaitForSeconds(dashCooldown);
        GiveDash();
    }

    private IEnumerator LerpVelocityTo(Vector3 iStartVelocity, Vector3 iNewVelocity, float iTime)
    {
        float elapsedTime = 0;
        Vector3 startingPos = iStartVelocity;
        while (elapsedTime < iTime)
        {
            m_Rigidbody.velocity = Vector3.Lerp(new Vector3(iStartVelocity.x, 0, iStartVelocity.z),
                                                new Vector3(iNewVelocity.x, 0, iNewVelocity.z),
                                                (elapsedTime / iTime));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
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
        return Physics.Raycast(transform.position, -transform.up, m_DistToGround + .01f);
    }

    private bool HasGripOnWall(Vector3 iDirection)
    {
        bool isGoingRight = Vector3.Dot(iDirection, Vector3.right) > 0;

        if(isGoingRight)
        {
            bool hasRightGrip = Physics.Raycast(transform.position, Vector3.right, m_DistToSide + .01f);

            return hasRightGrip;
        }
        else
        {
            bool hasLeftGrip = Physics.Raycast(transform.position, -Vector3.right, m_DistToSide + .01f);

            return hasLeftGrip;
        }
        
    }

    private IEnumerator DragDownCoroutine()
    {
        m_IsDraggedDown = true;
        Vector3 dragVector = Vector3.zero;

        while (m_ShouldBeDragged)
        {
            if(!m_Rigidbody.useGravity)
            {
                yield return new WaitForEndOfFrame();
            }

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

            if(m_Rigidbody.velocity.y >= 50)
            {
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, 50, m_Rigidbody.velocity.z);
            }
            else
            {
                m_Rigidbody.velocity += dragVector;
            }
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
        m_KeepInAir = true;
        //Pour laisser un peu de temps au joueur de sauter après avoir lâché un mur.
        yield return new WaitForSeconds(floattingTimeAfterGrip);
        m_KeepInAir = false;
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
         if(collision.gameObject.layer.Equals(LayerMask.NameToLayer("arrow"))){
            if(collision.gameObject.GetComponent<Rigidbody>().isKinematic){
            }
            else{
                Death();
                Destroy(collision.gameObject, arrowDisplayTime);
            }
        }

    }

    private void CheckIfIsGrabbingWall()
    {
        if (m_Dot > .75f || m_Dot < -.75f)
        {
            if (!m_IsGrippingWall)
            {
                StartCoroutine(GrippingWallCoroutine(Vector3.Normalize(m_HorizontalDirection)));
            }
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

     public void Flip()
    {
        Vector3 playerScale = transform.localScale;
        playerScale.x *= -1;
        transform.localScale = playerScale;
        m_FacingRight = !m_FacingRight;
    }

    void Death(){
        print("mort");
    }
}
