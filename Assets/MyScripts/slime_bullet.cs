using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Unity.VisualScripting;

public class slime_bullet : MonoBehaviour
{
    [SerializeField] GameObject impact; //敵にぶつかったときのエフェクト
    [SerializeField] float speed = 10f; //弾速
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed; //玉を飛ばす
    }

    /*
    private void OnTriggerEnter2D(Collider2D collision) //弾丸は壁と敵にのみ衝突
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")　|| collision.tag == "Enemy") //壁はレイヤーで判断
        {        
        }
    }
    */

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Instantiate(impact, transform.position, transform.rotation); //ぶつかったときエフェクトを出す
        Destroy(gameObject); //あたったら弾を消す
    }

}
