using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackScreenController : MonoBehaviour
{
    private Animator anim => GetComponent<Animator>();

    public void FadeIn() => anim.SetBool("Fade",true);

    public void FadeOut() => anim.SetBool("Fade",false);
}
