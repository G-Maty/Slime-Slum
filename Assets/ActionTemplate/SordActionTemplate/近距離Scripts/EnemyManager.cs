using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *　敵の見本コード
 */

public class EnemyManager : MonoBehaviour
{
    public int hp;
    Animator anim;
    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void OnDamage(int damage) //プレイヤーのattack関数で呼び出される
    {
        //引数は受けるダメージ、つまりプレイヤーの攻撃力
        hp -= damage;
        anim.SetTrigger("hurt"); //ダメージアニメーション
        if(hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        hp = 0;
        anim.SetTrigger("die"); //dieアニメーション
    }

}
