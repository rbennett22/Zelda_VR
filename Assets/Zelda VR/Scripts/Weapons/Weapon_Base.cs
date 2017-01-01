using UnityEngine;

// Abstract Base Class

public class Weapon_Base : MonoBehaviour
{
    public enum ControlStyleEnum
    {
        Classic,        // NES
        VR              // Oculus Touch
    }
    [SerializeField]
    protected ControlStyleEnum _controlStyle;
    virtual public ControlStyleEnum ControlStyle {
        get { return _controlStyle; }
        set {
            _controlStyle = value;
        }
    }

    [SerializeField]
    protected float _cooldown = 0;
    [SerializeField]
    protected AudioClip _attackSound;


    float _timeOfLastAttack = float.NegativeInfinity;
    public bool IsCooldownActive { get { return (Time.time - _timeOfLastAttack < _cooldown); } }

    virtual public bool IsAttacking { get { return false; } }

    virtual public bool CanAttack { get { return !IsCooldownActive && !IsAttacking; } }


    virtual public void Attack()
    {
        Attack(Vector3.zero);
    }
    virtual public void AttackTargetPosition(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        Attack(direction);
    }
    virtual public void Attack(Vector3 direction)
    {
        if (!CanAttack)
        {
            return;
        }

        PlayAttackSound();

        _timeOfLastAttack = Time.time;
    }


    protected void PlayAttackSound()
    {
        if(_attackSound == null)
        {
            return;
        }
        SoundFx.Instance.PlayOneShot3D(transform.position, _attackSound);
    }
}