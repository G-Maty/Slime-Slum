using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyTime : MonoBehaviour
{
    public float leftTime = 3f; //弾が消えるまでの時間
    void Start()
    {
        Destroy(gameObject, leftTime); //時間が立ったら弾を消す
    }
}
