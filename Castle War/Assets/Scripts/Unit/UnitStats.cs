using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
public enum DamageType
{
    Physical,
    Explosion,
    Healing,
    Lighting,
}

public class UnitStats : MonoBehaviour
{
    private Unit unit => GetComponent<Unit>();
    [Header("Unit Stats")]
    public Stats Damage;
    public Stats Armor;
    public Stats MaxHealth;
    public Stats CritChance;
    public int CurrentHealth;

    [Header("Damage Font")]
    [SerializeField] private GameObject DamageFont;
    [SerializeField] private DamageType damageType = DamageType.Physical;
    private Color fontColor;
    [Header("Exp Config")]
    [SerializeField] private GameObject LevelUpPrefab;
    [SerializeField] private Image LevelUI;
    public List<int> ExpList;
    public int currentExp;
    public int ExpIndex { get; private set; } = 0;

    public System.Action onHealthChanged;

    private void Start()
    {
        CurrentHealth = GetMaxHealthValue();
    }

    public void TakeDamage(UnitStats _stat) => TakeDamage(_stat, Damage.GetValue());

    public void TakeDamage(UnitStats _stat, int _damage)
    {
        if (_stat.GetComponent<Unit>().IsDead)
        {
            return;
        }

        if (Random.Range(0, 100) <= CritChance.GetValue())
        {
            _damage *= 2;
        }
        _damage -= _stat.Armor.GetValue();

        var fx = _stat.GetComponent<Unit>().fx;
        if (fx != null)
        {
            fx.Injured();
        }
        _stat.GetComponent<Unit>().ShowHealthBar();

        PopupDamageFont(_stat, _damage);

        DecreaseHealth(_stat, _damage);
    }

    private void PopupDamageFont(UnitStats _stat, int _damage, int _fontSize = 6)
    {
        if (_damage <= 0)
            return;

        switch (damageType)
            {
                case DamageType.Physical:
                    fontColor = Color.white;
                    break;
                case DamageType.Explosion:
                    fontColor = Color.red;
                    break;
                case DamageType.Healing:
                    fontColor = Color.green;
                    break;
                case DamageType.Lighting:
                    fontColor = Color.yellow;
                    break;
                default:
                    fontColor = Color.white;
                    break;
            }

        GameObject newFont = Instantiate(DamageFont);
        newFont.GetComponent<DamageFontUI>().SetFontValue(_damage, _fontSize, _stat.transform.position, fontColor, _stat.GetComponent<CapsuleCollider2D>().size.x * .5f);
    }

    private void PopupDamageFont(UnitStats _stat, int _damage, Color _color, int _fontSize = 6)
    {
        if (_damage <= 0)
            return;

        GameObject newFont = Instantiate(DamageFont);
        newFont.GetComponent<DamageFontUI>().SetFontValue(_damage, _fontSize, _stat.transform.position, _color, _stat.GetComponent<CapsuleCollider2D>().size.x * .5f);
    }

    public void TakeBurningDamage(int _damage)
    {
        PopupDamageFont(this, _damage, Color.red, 4);
        DecreaseHealth(this, _damage);
    }

    private void DecreaseHealth(UnitStats _stats, int _damage)
    {
        if (_stats.GetComponent<Unit>().IsDead)
            return;

        _stats.CurrentHealth -= _damage;
        if (_stats.CurrentHealth <= 0)
        {
            GetExp(_stats);
            _stats.CurrentHealth = 0;
            _stats.Death();
            if(_stats.LevelUI != null)
                _stats.LevelUI.gameObject.SetActive(false);
        }
        _stats.onHealthChanged?.Invoke();
    }

    public void Death()
    {
        unit.Death();
    }

    public int GetMaxHealthValue() => MaxHealth.GetValue();

    private void GetExp(UnitStats _stats)
    {
        if (unit.IsDead)
            return;

        int exp = _stats.currentExp;
        for (int i = 0; i < _stats.ExpIndex; i++)
        {
            exp += _stats.ExpList[i];
        }
        if (exp == 0)
        {
            exp = _stats.ExpList[0];
        }

        currentExp += Mathf.FloorToInt(exp * .4f);

        if (currentExp >= ExpList[ExpIndex] && ExpIndex < ExpList.Count)
        {
            while (currentExp >= ExpList[ExpIndex])
            {
                currentExp -= ExpList[ExpIndex];
                ExpIndex++;

                if (ExpIndex >= ExpList.Count)
                {
                    break;
                }

                Damage.AddModifier(Mathf.FloorToInt(Damage.GetValue() * .2f));
                MaxHealth.AddModifier(Mathf.FloorToInt(MaxHealth.GetValue() * .2f));
                Armor.AddModifier(Mathf.FloorToInt(Armor.GetValue() * .1f));
            }

            Instantiate(LevelUpPrefab, transform.position, Quaternion.identity);
            AudioManager.Get().PlaySFX(35);

            if(LevelUI != null)
                LevelUI.GetComponentInChildren<TextMeshProUGUI>().text = (ExpIndex + 1).ToString();
        }
    }
}
