using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetalSlime : Slime
{
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Transform attackPoint;
    [SerializeField] float attackSize;
    [SerializeField] GameObject vfxObj;

    protected override void Awake()
    {
        base.Awake();
    }

    void FixedUpdate()
    {
        //SlideTowardsPlayer();

        if (!isMoving)
            SlideTowardsPlayer();

        if (nextDamageTime <= Time.time && !canAttack)
        {
            DisableAttackVFX();
            canAttack = true;
        }
    }

    void Update()
    {
        animator?.SetBool("isMoving", isMoving);
        animator?.SetBool("isAttack", isAttacking);
    }

    private void SlideTowardsPlayer()
    {
        if (isAttacking)
            return;

        distanceFromPlayer = playerDistance();
        inAggroRange = distanceFromPlayer <= aggroDistance;
        inAttackRange = distanceFromPlayer <= attackDistance;

        if (playerObject != null && inAggroRange && canMove)
        {
            if (inAttackRange && canAttack)
            {
                isAttacking = true;
                animator?.SetTrigger("AttackStart");
                StartCoroutine(MetalAttack());
            }
            else
            {
                StartCoroutine(StartMove(moveStartup, moveInterval));
            }
        }
    }

    private IEnumerator MetalAttack()
    {
        yield return new WaitForSeconds(damageStartup);
        PlayAttackVFX(direction);

        Collider[] hitPlayer = Physics.OverlapSphere(attackPoint.position, attackSize, playerLayer);
        foreach (Collider collider in hitPlayer)
            collider.GetComponent<Player>().TakeDamage((int)attackPower);

        nextDamageTime = Time.time + damageInterval;
        canAttack = false;
        isAttacking = false;

    }

    protected void PlayAttackVFX(Vector3 direction)
    {
        vfxObj.transform.rotation = Quaternion.LookRotation(direction);
        vfxObj.SetActive(true);
    }

    public virtual void DisableAttackVFX()
    {
        vfxObj.SetActive(false);
    }
}
