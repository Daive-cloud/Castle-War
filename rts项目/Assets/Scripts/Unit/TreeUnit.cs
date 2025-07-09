using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeUnit : Unit
{
    private Animator anim => GetComponentInChildren<Animator>();
    private int Durability;

    protected override void Start()
    {
        base.Start();

        Durability = Random.Range(3,6);
    }

    public void Shake()
    {
        Durability--;

        if (Durability <= 0)
        {
            Death();
        }
        anim.SetTrigger("Shake");
    }

    public override void Death()
    {
        base.Death();

        anim.SetTrigger("Death");

        m_GameManager.WoodAmount += 100;
        m_GameManager.onResourcesChanged?.Invoke();
    }

}
