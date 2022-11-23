using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet_base : MonoBehaviour
{
    [SerializeField] GameObject impact; //敵にぶつかったときのエフェクト
    [SerializeField] public float speed = 10f; //弾速
    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = -transform.right * speed; //玉を飛ばす
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Instantiate(impact, transform.position, transform.rotation); //ぶつかったときエフェクトを出す
        Destroy(gameObject); //あたったら弾を消す
    }
}
