using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public List<ActionSO> Actions = new List<ActionSO>();
    protected GameManager m_GameManager;

    protected SpriteRenderer sr => GetComponentInChildren<SpriteRenderer>();
    public UnitStats stats => GetComponent<UnitStats>();
    public VFX fx => GetComponent<VFX>();

    [Header("Check Time")]
    [SerializeField] protected float CheckFrequency;
    protected float CheckTimer = 0f;

    public bool IsDead = false;

    [Header("Health Bar")]
    [SerializeField] protected GameObject HealthBar;

    public System.Action onFlipped;
    public System.Action onKilledTarget;
    public System.Action onTakedDamage;

    protected virtual void Start()
    {
        m_GameManager = GameManager.Get();
        m_GameManager.RegisterUnit(this);
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
        PlaySelectedSound();
        ShowHealthBar();
    }

    public virtual void UnselectedUnit()
    {
//        Debug.Log($"Unselect unit {this.name}");
        HideHealthBar();
    }

    public virtual void Death()
    {
        IsDead = true;
        PlayDeathSound();
        m_GameManager.RemoveUnit(this);

        if (m_GameManager.SelectedUnits.Contains(this))
        {
            m_GameManager.SelectedUnits.Remove(this);
        }
        if (HealthBar != null)
            HideHealthBar();
    }

    public void ShowHealthBar() => HealthBar.SetActive(true);
    public void HideHealthBar() => HealthBar.SetActive(false);
    public void DestroyUnit() => Destroy(gameObject);

    public virtual void PlayDeathSound()
    {

    }

    public virtual void PlaySelectedSound()
    {

    }
  
}
