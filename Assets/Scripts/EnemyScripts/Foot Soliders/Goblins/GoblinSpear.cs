using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinSpear : FootSoldier
{
    [SerializeField] LayerMask targetLayer;

    [SerializeField] List<Transform> attackPoints;
    [SerializeField] float attackSize;
    [SerializeField] GameObject regStabVfxObj;
    [SerializeField] GameObject runningVfxObj;
    [SerializeField] GameObject dashStabVfxObj;

    [SerializeField] float chargingSpeedMax;
    [SerializeField] float chargingSpeedAccel;
    [SerializeField] int chargingDamage;
    [SerializeField] float chargingDistance;
    bool inChargingRange;
    bool isCharging;

    float defaultMoveSpeed;

    private void Start()
    {
        defaultMoveSpeed = moveSpeed;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        inChargingRange = distanceFromTarget >= chargingDistance;
        SpearChoice();
    }

    private void SpearChoice()
    {
        //don't do shit if mid-attack
        if (isAttacking)
            return;

        //I know nested if statements are bad but honestly it's a student game I really don't think it's a big deal
        if (currentTarget != null && inAggroRange && canMove && !isDead)
        {
            // Store the previous target before updating
            previousTarget = currentTarget;

            // Update the target based on proximity
            UpdateTarget();

            // If the target has changed, print the new target
            if (previousTarget != currentTarget)
            {
                Debug.Log("Goblin (dagger) switched to: " + currentTarget.name);
            }

            if (canAttack && (inAttackRange || inChargingRange) && !isCharging)
            {
                //Do a basic stab if the player is already wicked close
                if (inAttackRange)
                {
                    isAttacking = true;
                    StartCoroutine(BasicStrike());
                }
                else
                {
                    StartCoroutine(ChargingStrike());
                }
            }
            else
                base.NavigateCombat();
        }
    }

    private IEnumerator ChargingStrike()
    {
        AudioManager.instance.PlayRandom(new string[] { "goblin_squeal_1", "goblin_squeal_2"});

        isCharging = true;
        DisableAttackVFX();

        animator.SetBool("isCharging", true);
        animator.SetTrigger("attackStart");

        PlayAttackVFX(direction, runningVfxObj);

        while (!inAttackRange)
        {
            moveSpeed = Mathf.Clamp(moveSpeed + chargingSpeedAccel * Time.deltaTime, defaultMoveSpeed, chargingSpeedMax);
            yield return null;
        }

        yield return new WaitForSeconds(attackStartup);

        DisableAttackVFX();
        if (hitMidAttack || isDead)
        {
            ResetAttack();
            hitMidAttack = false;
            yield break;
        }

        animator.SetTrigger("stab");
        PlayAttackVFX(direction, dashStabVfxObj);
        StabAttack(chargingDamage);

        ResetAttack();
    }

    private IEnumerator BasicStrike()
    {
        isAttacking = true;
        DisableAttackVFX();

        animator.SetTrigger("attackStart");
        yield return new WaitForSeconds(attackStartup);

        if (hitMidAttack || isDead)
        {
            ResetAttack();
            hitMidAttack = false;
            yield break;
        }

        PlayAttackVFX(direction, regStabVfxObj);
        StabAttack(attackPower);

        ResetAttack();
    }

    protected void PlayAttackVFX(Vector3 direction, GameObject vfxObj)
    {
        vfxObj.transform.rotation = Quaternion.LookRotation(new Vector3 (direction.x, 0.0f, direction.z));
        vfxObj.SetActive(true);
    }

    public virtual void DisableAttackVFX()
    {
        regStabVfxObj.SetActive(false);
        dashStabVfxObj.SetActive(false);
        runningVfxObj.SetActive(false);
    }

    private void StabAttack(int damage)
    {
        var ran = Random.Range(0, 5);
        if (ran == 1)
            AudioManager.instance.PlayRandom(new string[] { "goblin_sound_1", "goblin_sound_2", "goblin_sound_3" });

        List<Collider[]> hitTarget = new List<Collider[]>();

        foreach (Transform hitBox in attackPoints)
        {
            hitTarget.Add(Physics.OverlapSphere(hitBox.position, attackSize, targetLayer));
        }

        foreach (Collider[] targetList in hitTarget)
        {
            foreach (Collider c in targetList)
            {
                base.DealDamage(damage, knockback, c.gameObject);
                //return; //pnly damage player once
            }
        }
    }

    public override void TakeDamage(int damage, float knockback, Vector3 direction)
    {
        DisableAttackVFX();

        if (isCharging)
            hitMidAttack = true;

        //start the disablemove so it doesn't start mid combo
        if (disableMoveCoroutine != null)
            StopCoroutine(disableMoveCoroutine);

        disableMoveCoroutine = StartCoroutine(DisableMovementForSeconds(2f));

        if (isAttacking)
            hitMidAttack = true;

        //inflict damage
        base.TakeDamage(damage, knockback, direction);

        //die if dead
        //make sure they're not already dying, prevent from calling "die" twice
        if (curHealthPoints <= 0 && !animator.GetBool("isDead"))
        {
            //Debug.Log(gameObject.name + " Fucking Died");
            canMove = false;
            Die();
            StopCoroutine(disableMoveCoroutine);
        }
        else animator?.SetTrigger("pain");
    }

    private void ResetAttack()
    {
        //add some randomness to the next time he'll attack
        nextDamageTime = Time.time + damageInterval + Random.Range(damageIntRandomMin, damageIntRandomMax);
        canAttack = false;
        isAttacking = false;
        isCharging = false;
        animator.SetBool("isCharging", false);
        moveSpeed = defaultMoveSpeed;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        //Draw the hitbox
        foreach (Transform t in attackPoints)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(t.position, attackSize);
        }
    }
}
