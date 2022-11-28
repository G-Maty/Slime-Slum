using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class homingbullet_base : MonoBehaviour
{
    private Transform playerTrans; //追いかける対象のTransform
    [SerializeField] GameObject impact; //敵にぶつかったときのエフェクト
    [SerializeField] private float bulletSpeed;  　 //弾の速度
    [SerializeField] private float limitSpeed;      //弾の制限速度
    private Rigidbody2D rb;                         //弾のRigidbody2D
    private Transform bulletTrans;                  //弾のTransform

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bulletTrans = GetComponent<Transform>();
    }

    private void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        if(playerTrans == null) //プレイヤーが見つからない場合
        {
            Instantiate(impact, transform.position, transform.rotation); //エフェクトを出す
            Destroy(gameObject);
        }
        else
        {
            Vector3 vector3 = playerTrans.position - bulletTrans.position;  //弾から追いかける対象への方向を計算
            rb.AddForce(vector3.normalized * bulletSpeed);                  //方向の長さを1に正規化、任意の力をAddForceで加える

            float speedXTemp = Mathf.Clamp(rb.velocity.x, -limitSpeed, limitSpeed); //X方向の速度を制限
            float speedYTemp = Mathf.Clamp(rb.velocity.y, -limitSpeed, limitSpeed);  //Y方向の速度を制限
            rb.velocity = new Vector3(speedXTemp, speedYTemp);　　　　　　　　　　　//実際に制限した値を代入
        }
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Instantiate(impact, transform.position, transform.rotation); //ぶつかったときエフェクトを出す
        Destroy(gameObject); //あたったら弾を消す
    }
}
