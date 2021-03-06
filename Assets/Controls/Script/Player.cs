﻿using System.Collections;
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
    public int numberOfArrowAtBeginning = 4;
    public float respawnTimer = 1f;

    public float bumpFromCollision = 5f;

    public Animator m_Animator = null;
    public GameObject[] trails = null;

    public AudioClip[] deathSound = null;
    public AudioClip[] pickupSound = null;
    public AudioClip[] deathFromAboveSound = null;
    public AudioClip[] punchSound = null;
    public AudioClip[] fireSound = null;
    public AudioClip[] dashSound = null;

    private AudioSource source;

    public int Score = 0;
    public GameObject Canvas;
    public GameObject UI;



    private Rigidbody   m_Rigidbody = null;
    private Vector3     m_HorizontalDirection = Vector3.zero;
    private Vector3     m_VerticalDirection = Vector3.zero;
    private float       m_DistToGround = 0f;
    private float       m_DistToSide = 0f;
    private bool        m_ShouldBeDragged = false;
    private bool        m_IsGrippingWall = false;
    private bool        m_IsDraggedDown = false;
    private bool        m_IsJumping = false;
    private bool        m_IsDashing = false;
    private bool        m_KeepInAir = false;
    private int         m_AvailableJumps = 1;
    private int         m_availableDashs = 1;
    private bool        m_FacingRight = true;
    private bool        m_IsInAir = false;
    private bool        m_IsCloseEnoughToWall = false;
    private bool        m_CanDashAgain = false;
    private bool        m_IsAiming = false;
    private bool        m_IsInSlowMo = false;
    private Queue<bool> m_Quiver = new Queue<bool>();

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    private void Start()
    {
        foreach(GameObject trail in trails)
        {
            trail.SetActive(false);
        }
        m_Animator = GetComponent<Animator>();
        m_DistToGround = GetComponent<Collider>().bounds.extents.y;
        m_DistToSide = GetComponent<Collider>().bounds.extents.x;
        m_Rigidbody = GetComponent<Rigidbody>();
        for(int i=0; i < numberOfArrowAtBeginning;i++){
            m_Quiver.Enqueue(true);
        }
        UI.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = "x"+m_Quiver.Count;
    }

    private void FixedUpdate()
    {
        m_IsCloseEnoughToWall = HasGripOnWall((m_FacingRight) ? transform.right : - transform.right);
        m_IsInAir = IsInAir();

        bool isFalling = (m_Rigidbody.velocity.y < 0 ? m_IsInAir : false);

        m_Animator.SetBool("Falling", isFalling);
        m_Animator.SetBool("GrabingWall", m_IsGrippingWall || m_IsCloseEnoughToWall);

        if(m_IsInAir)
        {
            if(!m_IsDraggedDown)
            {
                StartCoroutine(DragDownCoroutine());
            }
        }

        //clamp vitesse
        if (m_Rigidbody.velocity.y < -50)
        {
            m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, -50, m_Rigidbody.velocity.z);
        }
        
        if (!m_KeepInAir)
        { 
            if (m_IsInAir && !m_IsDraggedDown)
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
        if (!m_IsDashing)
        {
            //on ne se déplace que si on n'est pas collé à un mur.
            m_HorizontalDirection = iInputValue * Vector3.right * Time.deltaTime;
            if (!HasGripOnWall(m_HorizontalDirection) && !m_IsAiming)
            {
                //m_Rigidbody.velocity += m_HorizontalDirection * speed;
                transform.Translate(m_HorizontalDirection * speed);

                m_Animator.SetBool("Walking", (m_HorizontalDirection * speed) != Vector3.zero);
            }
            else if (!m_IsGrippingWall && !m_IsAiming && m_Rigidbody.velocity.y < 0)
            {
                m_Animator.SetBool("Walking", false);
                StartCoroutine(GrippingWallCoroutine(m_HorizontalDirection.normalized));
            }

            CheckPlayerOrientation(iInputValue);
        }
            
    }

    private void CheckPlayerOrientation(float iInputValue)
    {
        if (iInputValue > 0 && !m_FacingRight)
        {
            Flip();
        }
        else if (iInputValue < 0 && m_FacingRight)
        {
            Flip();
        }
    }

    public void MoveVertical(float iInputValue)
    {
        m_VerticalDirection = iInputValue * Vector3.up;
    }

    public void Fire()
    {
        CancelAim();
        if (m_Quiver.Count > 0){
            Rigidbody arrowInstance;
            if (m_HorizontalDirection + m_VerticalDirection == Vector3.zero)
            {
                if (m_FacingRight)
                {
                    arrowInstance = Instantiate(arrow, new Vector3(transform.position.x + arrowOffset, transform.position.y, transform.position.z), Quaternion.identity) as Rigidbody;
                    arrowInstance.GetComponent<Arrow>().direction = Vector3.right;
                    arrowInstance.velocity = new Vector3(arrowSpeed, 0, 0);
                    arrowInstance.GetComponent<Arrow>().setOwner(this);
                }
                else
                {
                    arrowInstance = Instantiate(arrow, new Vector3(transform.position.x - arrowOffset, transform.position.y, transform.position.z), Quaternion.identity) as Rigidbody;
                    arrowInstance.GetComponent<Arrow>().direction = Vector3.left;
                    arrowInstance.velocity = new Vector3(-arrowSpeed, 0, 0);
                    arrowInstance.GetComponent<Arrow>().setOwner(this);
                }
            }
            else
            {
                Vector3 vel = arrowSpeed * (m_HorizontalDirection.normalized + m_VerticalDirection);
                arrowInstance = Instantiate(arrow, transform.position + (m_HorizontalDirection.normalized + m_VerticalDirection).normalized*arrowOffset, Quaternion.Euler(Arrow.GetRotationFromVelocity(vel))) as Rigidbody;
                arrowInstance.velocity = vel;
                arrowInstance.GetComponent<Arrow>().setOwner(this);
            }
            source.PlayOneShot(fireSound[Random.Range(0, fireSound.Length)], 1.0f);
            m_Quiver.Dequeue();
            updateQuiver();
        }
    }
    
    public void Jump()
    {
        if (!m_IsDashing)
        {
            if (m_AvailableJumps > 0)
            {

                Vector3 direction;

                direction = Vector3.up;
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, Mathf.Max(0, m_Rigidbody.velocity.y), m_Rigidbody.velocity.z);
                m_Rigidbody.velocity = (direction * jumpMultiplier);
                                
                //si on a la place de sauter, on le dépense ce saut de merde.
                if (!Physics.Raycast(transform.position, Vector3.up, m_DistToSide + .01f))
                {
                    m_Animator.SetTrigger("Jump");
                    SpendJump();
                    CancelAim();
                    StartCoroutine(JumpCoroutine());
                }

            }
        }
    }

    private IEnumerator JumpCoroutine()
    {
        m_IsJumping = true;

        while(m_Rigidbody.velocity.y > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        m_IsJumping = false;
    }

    public void Aim()
    {
        if(!m_IsAiming && (m_Quiver.Count != 0))
        {
            Vector3 startVelocity = (m_VerticalDirection.normalized + m_HorizontalDirection.normalized).normalized;
            StartCoroutine(AimCouroutine());
            StartCoroutine(SlowMo(startVelocity));
        }
    }

    private void CancelAim()
    {
        m_IsAiming = false;
        m_IsInSlowMo = false;
    }

    private IEnumerator SlowMo(Vector3 iStartVelocity)
    {
        m_IsInSlowMo = true;
        while(m_IsInSlowMo && m_IsInAir)
        {
            yield return new WaitForEndOfFrame();
            transform.position += iStartVelocity / 40 ;
            iStartVelocity -= new Vector3(0, Time.deltaTime, 0);
        }
    }

    //pour le catch de la flèche
    private IEnumerator SlowMoWithDuration(Vector3 iStartVelocity, float iDuration)
    {
        m_IsInSlowMo = true;
        float count = iDuration;
        while (m_IsInSlowMo && m_IsInAir && count >= 0)
        {
            yield return new WaitForEndOfFrame();
            transform.position += iStartVelocity / 40;
            iStartVelocity -= new Vector3(0, Time.deltaTime, 0);
            iDuration -= Time.deltaTime;
        }
    }

    private IEnumerator AimCouroutine()
    {
        m_Animator.SetBool("Aiming", true);
        m_IsAiming = true;
        //m_Rigidbody.isKinematic = true;
        m_Rigidbody.useGravity = false;
        m_Rigidbody.drag = 1000;

        Vector3 aimDirection = Vector3.zero;
        float aimAnimationNb = 2;
        while (m_IsAiming)
        {
            yield return new WaitForEndOfFrame();
            aimDirection = (m_HorizontalDirection.normalized + m_VerticalDirection.normalized).normalized;
            List<Vector3> possibleAimDirections = new List<Vector3>();
            float unknown2AMMultiplier = (m_FacingRight) ? 1 : -1;
            possibleAimDirections.Add(Vector3.up);
            possibleAimDirections.Add(Vector3.up + Vector3.right * unknown2AMMultiplier);
            possibleAimDirections.Add(Vector3.right * unknown2AMMultiplier);
            possibleAimDirections.Add(Vector3.down + Vector3.right * unknown2AMMultiplier);
            possibleAimDirections.Add(Vector3.down);
            aimAnimationNb = GetAnimIndexFromAim(aimDirection, possibleAimDirections);
            m_Animator.SetInteger("AimingDirection", (int)aimAnimationNb);
        }

        //m_Rigidbody.isKinematic = false;
        m_Rigidbody.useGravity = true;
        m_Rigidbody.drag = 0;
        m_Animator.SetBool("Aiming", false);
    }

    private float GetAnimIndexFromAim(Vector3 iDirection, List<Vector3> iOthers)
    {
        float lowestAngle = 360;
        int animNb = 2;
        if(iDirection == Vector3.zero){
            return 2;
        }

        for(int i = 0; i < iOthers.Count; i++)
        {
            if(Vector3.Angle(iDirection, iOthers[i].normalized) < lowestAngle)
            {
                lowestAngle = Vector3.Angle(iDirection, iOthers[i].normalized);
                animNb = i;
            }
        }

        return animNb;
    }

    public void Dash(float iInputValue)
    {
        if (iInputValue != 0)
        {
            //if (!HasGripOnWall(m_Rigidbody.velocity))
            {
                if (!m_IsDashing && m_availableDashs > 0 && iInputValue != 0 && m_CanDashAgain)
                {
                    source.PlayOneShot(dashSound[Random.Range(0, dashSound.Length)], 1.0f);
                    StartCoroutine(DashCoroutine(iInputValue));
                    StartCoroutine(DashCooldownCoroutine());
                }
            }
            m_CanDashAgain = false;
            /*else if (!m_IsGrippingWall)
            {
                StartCoroutine(GrippingWallCoroutine(Vector3.Normalize(m_Rigidbody.velocity)));
            }*/
        }
        else
        {
            m_CanDashAgain = true;
        }
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

    private IEnumerator DashCoroutine(float iInputValue)
    {
        m_Animator.SetBool("Dashing", true);
        foreach (GameObject trail in trails)
        {
            trail.SetActive(true);
        }
        m_IsDashing = true;
        m_Rigidbody.useGravity = false;
        GiveJump();
        SpendDash();


        Vector3 startVelocity = m_Rigidbody.velocity;
        Vector3 direction = (m_HorizontalDirection.normalized + m_VerticalDirection * 1.5f).normalized;
        if(direction == Vector3.zero)
        {
            direction = (-iInputValue * Vector3.right).normalized;
            CheckPlayerOrientation(iInputValue);
        }
        
        m_Rigidbody.velocity = direction * 30;
        yield return new WaitForSeconds(.1f);
        m_Rigidbody.velocity = direction * 18f;
        yield return new WaitForSeconds(.075f);
        m_Rigidbody.velocity = Vector3.zero;

        m_IsDashing = false;
        m_Rigidbody.useGravity = true;
        foreach (GameObject trail in trails)
        {
            trail.SetActive(false);
        }
        m_Animator.SetBool("Dashing", false);
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
            m_Rigidbody.velocity = Vector3.Lerp(iStartVelocity,
                                                iNewVelocity,
                                                (elapsedTime / iTime));
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

        float sideThreshold = (IsGrounded()) ? .1f : .01f;

        RaycastHit upHit;
        RaycastHit downHit;
        bool hitUp = false;
        bool hitDown = false;

        Vector3 upOffset = new Vector3(0, m_DistToGround - .25f, 0);

        bool hasRightGrip = false;
        bool hasLeftGrip = false;

        Debug.DrawRay(transform.position - upOffset, Vector3.right, Color.red);
        Debug.DrawRay(transform.position + upOffset, Vector3.right, Color.red);
        Debug.DrawRay(transform.position - upOffset, Vector3.left, Color.green);
        Debug.DrawRay(transform.position + upOffset, Vector3.left, Color.green);
        if (isGoingRight)
        {
            hitDown = Physics.Raycast(transform.position - upOffset, Vector3.right, out downHit);
            hitUp = Physics.Raycast(transform.position + upOffset, Vector3.right, out upHit);

            if (hitUp)
            {
                if (upHit.collider.gameObject.layer == LayerMask.NameToLayer("arena"))
                {
                    if (upHit.distance < m_DistToSide + sideThreshold)
                    {
                        hasRightGrip = true;
                    }
                }
            }
            if(hitDown)
            {
                if (downHit.collider.gameObject.layer == LayerMask.NameToLayer("arena"))
                {
                    if (downHit.distance < m_DistToSide + sideThreshold)
                    {
                        hasRightGrip = true;
                    }
                }
            }
        }
        else
        {
            hitDown = Physics.Raycast(transform.position - upOffset, Vector3.left, out downHit);
            hitUp = Physics.Raycast(transform.position + upOffset, Vector3.left, out upHit);

            if (hitUp)
            {
                if (upHit.collider.gameObject.layer == LayerMask.NameToLayer("arena"))
                {
                    if (upHit.distance < m_DistToSide + sideThreshold)
                    {
                        hasLeftGrip = true;
                    }
                }
            }
            if (hitDown)
            {
                if (downHit.collider.gameObject.layer == LayerMask.NameToLayer("arena"))
                {
                    if (downHit.distance < m_DistToSide + sideThreshold)
                    {
                        hasLeftGrip = true;
                    }
                }
            }

        }

        return (isGoingRight) ? hasRightGrip : hasLeftGrip && m_HorizontalDirection != Vector3.zero;
        
    }

    private IEnumerator DragDownCoroutine()
    {
        m_IsDraggedDown = true;
        Vector3 dragVector = Vector3.zero;

        while (m_IsInAir)
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

            m_Rigidbody.velocity += dragVector;
        }

        m_IsDraggedDown = false;
        GiveJump();
    }

    private IEnumerator GrippingWallCoroutine(Vector3 iGrippingDirection)
    {
        if(!m_IsJumping)
        {
            m_IsGrippingWall = true;
            m_Rigidbody.velocity = Vector3.zero;
            m_Rigidbody.useGravity = false;
            m_Rigidbody.drag = 1000;

            //tant que le direction dans laquelle on se déplace va dans le même sens la direction dans laquelle on s'est aggripé,
            //on coupe le joueur du moteur physique.
            while (Vector3.Dot(iGrippingDirection, m_HorizontalDirection) > 0 && !m_IsJumping)
            {
                yield return new WaitForEndOfFrame();
            }

            m_Rigidbody.useGravity = true;
            m_Rigidbody.drag = 0;
            m_ShouldBeDragged = false;
            m_KeepInAir = true;
            //Pour laisser un peu de temps au joueur de sauter après avoir lâché un mur.
            yield return new WaitForSeconds(floattingTimeAfterGrip);
            m_KeepInAir = false;
            m_ShouldBeDragged = true;
            m_IsGrippingWall = false;
        }
        
        yield return StartCoroutine(DragDownCoroutine());
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("SolidEnvironment"))
        {
            m_ShouldBeDragged = false;
            GiveJump();
            
        }

         if(collision.gameObject.layer.Equals(LayerMask.NameToLayer("arrow")))
        {
            bool isInFrontOfArrow = false;
            Vector3 arrowDirection = collision.gameObject.GetComponent<Arrow>().direction;
            Rigidbody arrowRb = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 arrowPos = collision.transform.position;
            Vector3 arrowToPlayerDirection = transform.position - collision.transform.position;
            isInFrontOfArrow = Vector3.Dot(arrowRb.velocity.normalized, arrowToPlayerDirection.normalized) > 0;

            Debug.Log(isInFrontOfArrow);

            if(arrowRb.isKinematic)
            {
                source.PlayOneShot(pickupSound[Random.Range(0, pickupSound.Length)], 1.0f);
                m_Quiver.Enqueue(true);
                updateQuiver();
                Destroy(collision.gameObject);
            }
            else{
                if (m_IsDashing) {
                    StartCoroutine(SlowMoCatchArrow());
                    source.PlayOneShot(pickupSound[Random.Range(0, pickupSound.Length)], 1.0f);
                    m_Quiver.Enqueue(true);
                    updateQuiver();
                    Destroy(collision.gameObject);
                }
                else if(isInFrontOfArrow)
                {
                    Destroy(collision.gameObject);
                    Death();
                    arrowRb.gameObject.GetComponent<Arrow>().getOwner().Score++;
                    foreach(Score s in Canvas.GetComponentsInChildren<Score>()){
                        s.UpdateScore();
                    }
                }
            }
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("player"))
        {
            collision.gameObject.GetComponent<Player>().Bump();
            source.PlayOneShot(punchSound[Random.Range(0, punchSound.Length)], 0.7f);
            DeathFromAbove(collision.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Spike")){
            Death();
            Score--;
            foreach (Score s in Canvas.GetComponentsInChildren<Score>())
            {
                s.UpdateScore();
            }
            return;
        }
    }

    private IEnumerator SlowMoCatchArrow()
    {
        //m_Rigidbody.;

        yield return new WaitForSeconds(2f);

        //m_KeepInAir = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("arena"))
        {
            m_ShouldBeDragged = false;
        }
    }

    private void DeathFromAbove(GameObject iFromPlayer)
    {
        Rigidbody rb = iFromPlayer.GetComponent<Rigidbody>();
        if (Vector3.Dot((iFromPlayer.transform.position - transform.position).normalized, transform.up) >= .85f)
        {
            source.PlayOneShot(deathFromAboveSound[Random.Range(0, deathFromAboveSound.Length)], 0.7f);
            Death();
            iFromPlayer.GetComponent<Player>().Score++;
            foreach(Score s in Canvas.GetComponentsInChildren<Score>()){
                s.UpdateScore();
            }
        }
    }

    public void Bump()
    {
        m_Rigidbody.velocity += Vector3.up * bumpFromCollision;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("arena"))
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
        UI.transform.localScale = new Vector3( UI.transform.localScale.x*-1, UI.transform.localScale.y, UI.transform.localScale.z);
    }

    private void Death()
    {
        source.PlayOneShot(deathSound[Random.Range(0, deathSound.Length)], 1.0f);
        m_Animator.SetBool("Death", true);
        this.enabled = false;
        MeshRenderer[] meshs = this.GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.gameObject.SetActive(false);
        }

        this.transform.position = GameManager.instance.GetUnusedSpawn();

        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnTimer);
        
        this.enabled = true;
        m_Quiver.Enqueue(true);        
        
        MeshRenderer[] meshs = this.GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.gameObject.SetActive(true);
        }
        m_Animator.SetBool("Death", false);
    }

    void updateQuiver(){
        UI.transform.GetChild(0).gameObject.GetComponent<TextMesh>().text = "x"+m_Quiver.Count;
    }
}
