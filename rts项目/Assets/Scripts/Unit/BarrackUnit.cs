using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrackUnit : StructureUnit
{
    public override void UnselectedUnit()
    {
        base.UnselectedUnit();

        m_GameManager.CancleTraining();
    }
}
