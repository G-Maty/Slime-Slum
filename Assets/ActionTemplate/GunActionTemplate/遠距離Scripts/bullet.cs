using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public GameObject impact; //敵にぶつかったときのエフェクト
    float speed = 10f; //弾速
    Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed; //玉を飛ばす
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Enemy")
        {
            //作成したコードEnemyクラス型の空変数enemを作成し、ぶつかったオブジェクトのコンポーネントEnemyクラスを取得する
            Enemy enem = collision.GetComponent<Enemy>();
            enem.OnDamage();     //OnDamage関数を呼び出す
            Instantiate(impact, transform.position, transform.rotation); //弾が敵にぶつかったときエフェクトを出す
        }
        Destroy(gameObject); //敵にあたったら弾を消す
    }
}
