using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UIElements;


/* duration is how long the attack will last
 * impact is the momentum the attack carries
 * vfxIndex corresponds to the VFX animation that will get called by VFX controller
 */

[System.Serializable]
public class PlayerAttack
{
    //This is the visual stuff
    [SerializeField] private string attackName;
    [SerializeField] private string animTrigger;

    //Sound effect stuff
    [SerializeField] protected string audioName;
    protected static string[] contactAudioNames;
        public string[] combatSounds
    {  get { return contactAudioNames; } set { contactAudioNames = value; } }

    //This is stuff that affects game states
    [SerializeField] private float duration;
    [SerializeField] private float impact;
    [SerializeField] private float delay;
    [SerializeField] private float damage;
    [SerializeField] private float knockBack;

    [SerializeField] protected bool independentKnockback = false;

    [SerializeField] private List<HitBox> hitBoxes;
    [SerializeField] private GameObject vfxObj;

    //collection of getter methods
    public string GetName() { return attackName; }
    public string GetAnim() { return animTrigger; }
    public float GetDuration() { return duration; }
    public float GetImpact() { return impact; }
    public float GetDelay() { return delay; }
    public float GetDamage() { return damage; }
    public float GetKnockBack() { return knockBack; }

    public GameObject GetVfxObj() { return vfxObj; }

    public List<HitBox> GetHitBoxes() { return hitBoxes; }

    public virtual IEnumerator ActivateAttack(Player player, float dmgMultiplier, float meterGain, LayerMask enemyLayers, UnityEngine.Vector3 direction)
    {
        yield return new WaitForSeconds(delay);
        DisableAttackVFX();
        PlayAttackVFX(direction);

        //play sound
        AudioManager.instance.Play(audioName);

        List<Collider[]> hitEnemies = new List<Collider[]>();

        foreach (HitBox hitBox in hitBoxes)
        {
            hitEnemies.Add(Physics.OverlapSphere(hitBox.GetPosition(), hitBox.GetSize(), enemyLayers));
        }

        //this is mostly for any attack that surrounds the player, so that when takedamage is called,
        //the knockback is exclusively in the direction away from the player
        UnityEngine.Vector3 knockbackDirection;
        if (independentKnockback)
            knockbackDirection = UnityEngine.Vector3.zero;
        else
            knockbackDirection = direction;

        //This is to prevent enemies from Getting hit twice if they're in range of 2 or more hitboxes
        HashSet<Collider> loggedEnemies = new HashSet<Collider>();

        foreach (Collider[] enemyList in hitEnemies)
        {
            foreach (Collider enemy in enemyList)
            {
                if (!loggedEnemies.Contains(enemy))
                {
                    AudioManager.instance.PlayRandom(contactAudioNames);

                    //Main meter per enemy hit
                    player.GainMeter(meterGain);
                    Enemy thisEnemy = enemy.GetComponent<Enemy>();

                    //this is the main attack shit
                    thisEnemy.TakeDamage((int)(damage * player.GetAttackScale() * dmgMultiplier), knockBack * player.GetKnockBScale(), knockbackDirection);
                    loggedEnemies.Add(enemy);
                }
            }
        }
    }

    protected void PlayAttackVFX(UnityEngine.Vector3 direction)
    {
        vfxObj.transform.rotation = UnityEngine.Quaternion.LookRotation(direction);
        vfxObj.SetActive(true);
    }

    public virtual void DisableAttackVFX()
    {
        vfxObj.SetActive(false);
    }
}

[System.Serializable]
public class HitBox
{
    [SerializeField] private Transform center;
    [SerializeField] private float size;

    public Transform GetTransform() { return center; }

    public UnityEngine.Vector3 GetPosition() { return center.position; }
    public float GetSize() { return size; }
}
