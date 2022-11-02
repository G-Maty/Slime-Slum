using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *　移動・ジャンプ・接近攻撃の見本コード
 */

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float jumpForce = 600f;
    private bool isJump = false; //ジャンプ中かどうか
    private bool isAttack = false; //攻撃中かどうか
    private bool damage = false;


    public Transform attackPoint; //攻撃当たり判定部分
    public float attackRadius; //攻撃当たり判定の半径
    //レイヤーにEnemyを追加
    public LayerMask enemyLayer; //特定のレイヤーでのみコライダーを検知するフィルター(敵か敵じゃないかの区別)
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    Animator anim;
    int at = 1; //攻撃力

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isJump) //スペースが押されて、ジャンプ中じゃなければ
        {
            anim.SetTrigger("attack"); //attackアニメーション(このモーション途中のイベントでattack関数が呼ばれる)
            isAttack = true;
        }
        Movement(); //動く

    }

    public void attack() //attackアニメーションのイベント機能で呼び出しされる
    {
        //当たり判定部にぶつかったものがhitEnemysにはいる
        Collider2D[] hitEnemys = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
        foreach(Collider2D hitEnemy in hitEnemys)
        {
            hitEnemy.GetComponent<EnemyManager>().OnDamage(at); //ぶつかった敵のスクリプトのOnDamage関数を呼ぶ
        }
    }

    private void OnDrawGizmosSelected() //当たり判定部の可視化
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position,attackRadius);
    }

    void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        //float y = Input.GetAxis("Vertical");
        if (x > 0)
        {
            transform.localScale = new Vector3(-1.0f, 1.0f, 1);
        }
        if (x < 0)
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1);
        }
        if(Input.GetKeyDown(KeyCode.W) && !isJump) //空中ではジャンプできない
        {
            rb.AddForce(transform.up * jumpForce); //力を加えてジャンプ
            anim.SetTrigger("jump");
            isJump = true;
        }
        anim.SetFloat("run", Mathf.Abs(x)); //runアニメーション
        rb.velocity = new Vector2(x * moveSpeed, rb.velocity.y); //移動の実行
        if (isAttack) //攻撃中は動きを止める
        {
            rb.velocity = new Vector2(0, 0);
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

    public void AttackFinish() //攻撃モーション終わりにイベントで呼ばれ、isAttackフラグを戻す
    {
        isAttack = false;
    }

}
