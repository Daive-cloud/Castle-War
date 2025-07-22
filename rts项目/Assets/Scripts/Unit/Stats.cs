

using System.Collections.Generic;

[System.Serializable]
public class Stats
{
    public int baseValue;
    public List<int> modifiers;

    public void AddModifier(int _value)
    {
        modifiers.Add(_value);
    }

    public void RemoveModifier(int _value)
    {
        modifiers.Remove(_value);
    }

    public int GetValue()
    {
        int value = baseValue;
        foreach (var modifier in modifiers)
        {
            value += modifier;
        }
        return value;
    }
}
