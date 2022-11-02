using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpForce = 600f;

    public GameObject bullet;
    public Transform shotPoint;
    Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;


    //弾を飛ばすクールタイム（時間が長いほど連射しづらくなる）
    float coolTime = 0.2f; //待機時間
    float leftCoolTime; //待機している時間

    bool isRight; //右向きかどうか
    private bool isJump = false; //ジャンプ中かどうか
    private bool isAttack = false; //攻撃中かどうか
    private bool damage = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        isRight = true;
        leftCoolTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        shot();
    }

    void Direction(float inputX) //スケールを変える方法だと弾が右方向にしか飛ばないため
    {
        //右向き左入力で180度回転、左向き右入力で180度回転
        if(isRight && inputX < 0)
        {
            transform.Rotate(0, 180f, 0);
            isRight = false;
        }
        if(!isRight && inputX > 0)
        {
            transform.Rotate(0, 180f, 0);
            isRight = true;
        }
    }

    void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        //float y = Input.GetAxis("Vertical");
        Direction(x); //スケールを変える方法だと弾が右方向にしか飛ばないため
        if (Input.GetKeyDown(KeyCode.W) && !isJump) //空中ではジャンプできない
        {
            rb.AddForce(transform.up * jumpForce); //力を加えてジャンプ
            //anim.SetTrigger("jump");
            isJump = true;
        }
        anim.SetFloat("run", Mathf.Abs(x)); //runアニメーション
        rb.velocity = new Vector2(x * moveSpeed, rb.velocity.y); //移動の実行
        if (isAttack) //攻撃中は動きを止める
        {
            rb.velocity = new Vector2(0, 0);
        }

    }

    void shot()
    {
        leftCoolTime -= Time.deltaTime; //クールタイム更新(shot関数は毎Update呼ばれる)
        if(leftCoolTime <= 0) //残り待機時間が0秒以下のとき
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                anim.SetTrigger("shot"); //ショットアニメーション
                Instantiate(bullet, shotPoint.position, transform.rotation); //弾を前方に発射する
                leftCoolTime = coolTime; //クールタイム発生
            }
        }
    }

    private void Damage()
    {
        //既にダメージ状態（＝無敵時間中）なら終了
        if (damage)
        {
            return;
        }
        StartCoroutine("DamageTimer");
        /*
         * ダメージ処理
         */
    }

    //ダメージを受けた瞬間の無敵時間のタイマー
    private IEnumerator DamageTimer()
    {
        //既にダメージ状態なら終了
        if (damage)
        {
            yield break;
        }
        damage = true;
        //anim.SetTrigger("damage");
        //無敵時間中の点滅
        for (int i = 0; i < 10; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(0.05f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.05f);
        }
        damage = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)　//接地および当たり判定
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            isJump = false;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Damage();
        }
    }

}
