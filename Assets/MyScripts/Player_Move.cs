using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Player_Move: MonoBehaviour
{
    public float moveSpeed = 5.5f;
    public float jumpForce = 200f;
    private bool isJump = false; //ジャンプ中かどうか
    private bool isup = false; //張り付き中かどうか
    private bool isRight;
    private bool isAttack = false; //攻撃中かどうか
    private bool damage = false;

    //shot関係
    public GameObject bullet;
    public Transform shotPoint;
    float coolTime = 0.2f; //待機時間
    float leftCoolTime; //待機している時間

    //接地判定関係
    [SerializeField] private LayerMask groundLayer; //for GroundCheck

    //レイヤーにEnemyを追加
    public LayerMask enemyLayer; //特定のレイヤーでのみコライダーを検知するフィルター(敵か敵じゃないかの区別)
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private Animator anim;

    public UnityAction warpCheckpoint; //ダメージ処理(デリゲート)
    public UnityAction<GameObject> checkPoint_Update;


    

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        isRight = true;
    }

    private void FixedUpdate()
    {
      
    }

    // Update is called once per frame
    void Update()
    {
        Movement(); //動く
    }

    void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        //float y = Input.GetAxis("Vertical");
        if (!damage) //ダメージ受けていないときに行動可能
        {
            Direction(x);
            Jump(); //ジャンプ
            if (!isJump)
            {
                shot(); //ショット
            }
            anim.SetFloat("run", Mathf.Abs(x)); //runアニメーション
            rb.velocity = new Vector2(x * moveSpeed, rb.velocity.y); //移動の実行
            if (isAttack) //攻撃中は動きを止める
            {
                rb.velocity = new Vector2(0, 0);
            }
        }
        else //ダメージ受けているときにその場に硬直
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
        }

    }

    void Direction(float inputX) //スケールを変える方法だと弾が右方向にしか飛ばないため
    {
        //右向き左入力で180度回転、左向き右入力で180度回転
        if (isRight && inputX < 0)
        {
            transform.Rotate(0, 180f, 0);
            isRight = false;
        }
        if (!isRight && inputX > 0)
        {
            transform.Rotate(0, 180f, 0);
            isRight = true;
        }
    }

    void Jump()
    {
        bool isground_up = checkground_up();
        bool isground_down = checkground_down();
        //float y = Input.GetAxis("Vertical");

        if (!isground_up && isground_down) //下接地中
        {
            if (Input.GetKeyDown(KeyCode.W) && !isup)
            {
                isJump = true;
                anim.SetBool("landing_down", false);
                anim.SetTrigger("jump");
                rb.gravityScale = -3f;
                rb.AddForce(transform.up * jumpForce); //力を加えてジャンプ
                isup = true;
            }
            if (!isup) //ジャンプした瞬間の誤作動を防止
            {
                isJump = false;
                anim.SetBool("landing_down",true); //着地アニメーション
            }
        }
        if(isground_up && !isground_down) //上接地中
        {
            if (Input.GetKeyDown(KeyCode.S) && isup) //空中ではジャンプできない
            {
                isJump = true;
                anim.SetBool("landing_up",false);
                anim.SetTrigger("jump");
                rb.gravityScale = 3f;
                rb.AddForce(transform.up * -jumpForce); //力を加えてジャンプ
                isup = false;
            }
            if (isup) //ジャンプした瞬間の誤作動を防止
            {
                isJump = false;
                anim.SetBool("landing_up",true); //着地アニメーション
            }
        }       
    }

    private void shot()
    {
        leftCoolTime -= Time.deltaTime; //クールタイム更新(shot関数は毎Update呼ばれる)
        if (leftCoolTime <= 0) //残り待機時間が0秒以下のとき
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isAttack = true;
                anim.SetTrigger("shot"); //ショットアニメーション
                Instantiate(bullet, shotPoint.position, transform.rotation); //弾を前方に発射する
                leftCoolTime = coolTime; //クールタイム発生
            }
        }
    }

    public void shotEnd() //shotアニメーションの最終フレームで利用
    {
        isAttack = false;
    }

    //Damage()では主に無敵時間とダメージアニメーションの実装
    //具体的なダメージ処理はDamageTimer中のonDeadに登録済み(デリゲート)
    private void Damage()
    {
        //既にダメージ状態（＝無敵時間中）なら終了
        if (damage)
        {
            return;
        }
        StartCoroutine("DamageTimer");
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
        for (int i = 0; i < 10; i++) //1秒
        {
            spriteRenderer.enabled = false; //無色点滅
            yield return new WaitForSeconds(0.05f);
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(0.05f);
        }
        damage = false;
        //ダメージ処理
        warpCheckpoint?.Invoke();
    }

    //接地チェック
    private bool CheckGround()
    {
        bool check_down = checkground_down();
        bool check_up = checkground_up();

        if (check_down && check_up) //接地していたらtrue
        {
            Debug.Log("double graound");
            return false;
        }
        else if(check_down || check_up)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    private bool checkground_down()
    {
        //光線の距離
        float raydown_distance = 0.5f;
        //float rayRadius = 0.2f;
        //光線衝突時、衝突オブジェクトの情報を格納(地面レイヤに限定)
        RaycastHit2D hitrayinfo_down;
        hitrayinfo_down = Physics2D.Raycast(transform.position,  Vector2.down, raydown_distance, groundLayer);
        //デバッグ用
        Debug.DrawRay(transform.position, Vector3.down * raydown_distance, Color.red);
        //光線が地面レイヤのコライダとぶつかったときにtrueを返す
        if(hitrayinfo_down.distance == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool checkground_up()
    {
        //光線の距離
        float rayup_distance = 0.5f;
        //float rayRadius = 0.2f;
        //光線衝突時、衝突オブジェクトの情報を格納(地面レイヤに限定)
        RaycastHit2D hitrayinfo_up;
        hitrayinfo_up = Physics2D.Raycast(transform.position,  Vector2.up, rayup_distance, groundLayer);
        //デバッグ用
        Debug.DrawRay(transform.position, Vector3.up * rayup_distance, Color.blue);
        //光線が地面レイヤのコライダとぶつかったときにtrueを返す
        if (hitrayinfo_up.distance == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Respawn"))
        {
            checkPoint_Update?.Invoke(collision.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap"))
        {
            Damage();
        }
    }

}
