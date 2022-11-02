using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int hp = 5; //体力
    public GameObject deathEffect; //撃破時のエフェクト
    public void OnDamage()
    {
        hp -= 1;
        if(hp <= 0)
        {
            Instantiate(deathEffect,transform.position,transform.rotation); //撃破時にエフェクトを出す
            Destroy(gameObject); //敵を消去する
        }
    }
}
