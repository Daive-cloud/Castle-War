using UnityEngine;

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
    public int Damage;
    public int Armor;
    public int MaxHealth;
    public int CritChance;
    public int CurrentHealth;

    [Header("Damage Font")]
    [SerializeField] private GameObject DamageFont;
    [SerializeField] private DamageType damageType = DamageType.Physical;
    private Color fontColor;

    public System.Action onHealthChanged;

    private void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public void TakeDamage(UnitStats _stat) => TakeDamage(_stat, Damage);

    public void TakeDamage(UnitStats _stat, int _damage)
    {
        if (_stat.GetComponent<Unit>().IsDead)
        {
            return;
        }

        bool isCrit = false;
        if (Random.Range(0, 100) <= CritChance)
        {
            isCrit = true;
            _damage *= 2;
        }
        _damage -= _stat.Armor;

        var fx = _stat.GetComponent<Unit>().fx;
        if (fx != null)
        {
            fx.Injured();
        }
        _stat.GetComponent<Unit>().ShowHealthBar();
        Color color = isCrit ? Color.yellow : Color.white;

        PopupDamageFont(_stat, _damage);

        DecreaseHealth(_stat, _damage);
    }

    private void PopupDamageFont(UnitStats _stat, int _damage, int _fontSize = 6)
    {
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
        newFont.GetComponent<DamageFontUI>().SetFontValue(_damage, _fontSize, _stat.transform.position,fontColor , _stat.GetComponent<CapsuleCollider2D>().size.x * .5f);
    }

    private void PopupDamageFont(UnitStats _stat, int _damage,Color _color, int _fontSize = 6)
    {
         GameObject newFont = Instantiate(DamageFont);
        newFont.GetComponent<DamageFontUI>().SetFontValue(_damage, _fontSize, _stat.transform.position,_color , _stat.GetComponent<CapsuleCollider2D>().size.x * .5f);
    }

    public void TakeBurningDamage(int _damage)
    {
        PopupDamageFont(this, _damage,Color.red, 4);
        DecreaseHealth(this, _damage);
    }

    private void DecreaseHealth(UnitStats _stats, int _damage)
    {
        if (_stats.GetComponent<Unit>().IsDead)
            return;

        _stats.CurrentHealth -= _damage;
        if (_stats.CurrentHealth <= 0)
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
