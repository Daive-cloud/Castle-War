using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUI : MonoBehaviour
{
    public void ChooseEnemy()
    {
        AudioManager.Get().PlaySFX(3);
    }
}
