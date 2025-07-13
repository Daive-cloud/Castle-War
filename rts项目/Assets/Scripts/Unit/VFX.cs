using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX : MonoBehaviour
{
    public GameObject BloodEffect;

    public void Injured() => Instantiate(BloodEffect,transform.position,Quaternion.identity);
}
