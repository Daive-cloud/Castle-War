using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public List<ActionSO> Actions = new List<ActionSO>();
    protected GameManager m_GameManager;
   
    protected SpriteRenderer sr => GetComponentInChildren<SpriteRenderer>();
    public UnitStats stats => GetComponent<UnitStats>();

    [Header("Check Time")]
    [SerializeField] protected float CheckFrequency;
    protected float CheckTimer = 0f;

    public bool IsDead = false;

    [Header("Health Bar")]
    [SerializeField] protected GameObject HealthBar;

    public System.Action onFlipped;

    protected virtual void Start()
    {
        m_GameManager = GameManager.Get();
        m_GameManager.RegisterUnit(this);

        HealthBar.SetActive(false);
    }

    protected virtual void Update()
    {
       UpdateBehaviour();
    }

    protected virtual void UpdateBehaviour()
    {
       
    }

    public virtual void SelectedUnit()
    {
        HealthBar.SetActive(true);
    }

    public virtual void UnselectedUnit()
    {
        HealthBar.SetActive(false);
    }

    public virtual void Death()
    {
        IsDead = true;
        m_GameManager.RemoveUnit(this);
        if (HealthBar != null)
            HealthBar.SetActive(false);
    }

    public void DestroyUnit() => Destroy(gameObject);
  
}
