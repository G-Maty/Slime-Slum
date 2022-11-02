using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyTime : MonoBehaviour
{
    public float leftTime = 3f; //’e‚ªÁ‚¦‚é‚Ü‚Å‚ÌŠÔ
    void Start()
    {
        Destroy(gameObject, leftTime); //ŠÔ‚ª—§‚Á‚½‚ç’e‚ğÁ‚·
    }
}
