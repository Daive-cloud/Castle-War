using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStats : MonoBehaviour
{
    private Unit unit => GetComponent<Unit>();
    [Header("Unit Stats")]
    public int Damage;
    public int Armor;
    public int MaxHealth;
    public int CritChance;

    public int CurrentHealth;

    [Header("Damage Font")]
    [SerializeField] private GameObject DamageFont;

    public System.Action onHealthChanged;

    private void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(UnitStats _stat)
    {
      
        int damage = Damage;

        if(Random.Range(0,100) <= CritChance)
        {
            damage *= 2;
        }
        damage -= _stat.Armor;

        var fx = _stat.GetComponent<Unit>().fx;
        if (fx != null)
        {
            fx.Injured();
        }
        _stat.GetComponent<Unit>().ShowHealthBar();
        GameObject newFont = Instantiate(DamageFont,_stat.transform.position + Vector3.up,Quaternion.identity);
        newFont.GetComponent<DamageFontUI>().SetDamageValue(damage);
   
        DecreaseHealth(_stat,damage);
    }

    private void DecreaseHealth(UnitStats _stats,int _damage)
    {
        if (_stats.GetComponent<Unit>().IsDead)
            return;

        _stats.CurrentHealth -= _damage;
        if(_stats.CurrentHealth <= 0)
        {
            _stats.CurrentHealth = 0;
            _stats.Death();
        }
        _stats.onHealthChanged?.Invoke();
    }

    public void Death()
    {
        unit.Death();
    }

    public int GetMaxHealthValue() => MaxHealth;
}
