using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HumanoidUnit : Unit
{
    protected AI ai => GetComponent<AI>();
    protected Animator anim => GetComponentInChildren<Animator>();

    private float m_Velocity;
    private Vector3 m_LastPosition;
    private bool IsFacingRight = true;

    [Header("Detection Radius")]
    public float ObjectCheckRadius;
    public float AttackCheckRadius;

    [Header("Attack Info")]
    [SerializeField] protected float AttackFrequency;
    protected float AttackTimer;
    public List<Unit> Enemies = new List<Unit>();

    public Unit Target;
    public bool HasRegisteredTarget
    {
        get
        {
            if(Target != null && Target.IsDead)
            {
                Target = null;
                return false;
            }
            return Target != null;
        }
    }

    protected int ComboCounter;


    protected override void Start()
    {
        base.Start();
        m_LastPosition = transform.position;
    }

    protected override void Update()
    {
        base.Update();

        m_Velocity = (transform.position - m_LastPosition).magnitude;
        m_LastPosition = transform.position;

        bool state = m_Velocity > 0;
        if (anim != null)
            anim.SetBool("Move", state);
    }

    public virtual void MoveToDestination(Vector2 _position)
    {
        Vector3 position = new Vector3(_position.x, _position.y, transform.position.z);
        ai.RegisterDestination(position);
        FlipController(position);
    }

    public void FlipController(Vector2 _mousePosition)
    {
        if (_mousePosition.x < transform.position.x && IsFacingRight)
        {
            Flip();
        }
        else if (_mousePosition.x > transform.position.x && !IsFacingRight)
        {
            Flip();
        }
    }

    protected void Flip()
    {
        IsFacingRight = !IsFacingRight;
        transform.Rotate(0, 180, 0);
        onFlipped?.Invoke();
    }

    protected void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position,ObjectCheckRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position,AttackCheckRadius);
    }

    public virtual void AssignTarget(Unit _unit) => Target = _unit;

    public virtual void UnassignTarget() => Target = null;

    public bool IsBuildingDetected()
    {
        if (!HasRegisteredTarget)
            return false;
        
        if (Target.TryGetComponent(out StructureUnit structure))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, ObjectCheckRadius);
            bool flag = colliders.ToList().Contains(structure.GetComponent<CapsuleCollider2D>());
           
            return flag;
        }

        var distance = Vector2.Distance(Target.transform.position,transform.position);
        return distance < ObjectCheckRadius;
    }

    public bool CanReachTarget(Unit _unit)
    {
        if (_unit == null)
            return false;

        var distance = Vector2.Distance(_unit.transform.position, transform.position);
        return distance < ObjectCheckRadius;
    }

    public override void Death()
    {
        base.Death();
        anim.SetTrigger("Death");
        UnassignTarget();
    }

   protected bool CanAttackTarget()
    {
        var distance = Vector2.Distance(Target.transform.position, transform.position);

        if (Target.TryGetComponent(out StructureUnit structure))
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, AttackCheckRadius);
            bool flag = colliders.ToList().Contains(Target.GetComponent<CapsuleCollider2D>());
            return flag;
        }
        return distance < AttackCheckRadius;
    }

    public void AnimationFinishTrigger()
    {
        anim.SetBool("Attack",false);
        anim.SetBool("Attack_Up",false);
        anim.SetBool("Attack_Down",false);
        ComboCounter++;
    }

    public void AnimationFinishTrigger_2()
    {
        anim.SetBool("Attack", false);
        anim.SetBool("Attack_Up", false);
        anim.SetBool("Attack_Top", false);
        anim.SetBool("Attack_Bottom", false);
        anim.SetBool("Attack_Down",false);
    }

    public void AnimationFinishTrigger_3()
    {
        anim.SetBool("Attack",false);
    }

    public void FindClosestEnemyInRange()
    {
        Enemies = m_GameManager.RegisteredUnits.Where(unit => unit != null && !unit.IsDead && unit.tag != this.tag && unit.tag != "Tree").ToList();

        float closestDistance = int.MaxValue;
        Unit closestEnemy = null;

        foreach (var enemy in Enemies)
        {
            float distance = Vector2.Distance(enemy.transform.position, transform.position);

            if (distance < closestDistance)
            {
                closestEnemy = enemy;
                closestDistance = distance;
            }
        }

        if (CanReachTarget(closestEnemy))
        {
            Target = closestEnemy;
        }
    }

    public void FindClosestEnemyWithoutRange()
    {
        Enemies.Clear();
        Enemies = m_GameManager.RegisteredUnits.Where(unit => unit != null && unit.gameObject != null && !unit.IsDead && unit.tag != this.tag && unit.tag != "Tree").ToList();

        float closestDistance = int.MaxValue;
        Unit closestEnemy = null;

        foreach (var enemy in Enemies)
        {
            float distance = Vector2.Distance(enemy.transform.position, transform.position);

            if (distance < closestDistance)
            {
                closestEnemy = enemy;
                closestDistance = distance;
            }
        }

        Target = closestEnemy;
    }

  

   

}
