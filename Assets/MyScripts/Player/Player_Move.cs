using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.UI;
using DG.Tweening;
using Fungus;

/*
 * プレイヤーの移動・アニメーション・ダメージ判定
 * SE・エフェクト
 */


public class Player_Move: MonoBehaviour
{
    public float moveSpeed = 5.5f;
    public float jumpForce = 200f;
    private bool isJump = false; //ジャンプ中かどうか
    private bool isup = false; //張り付き中かどうか
    private bool isRight;
    private bool isAttack = false; //攻撃中かどうか
    private bool damage = false;
    private float previousGravityScale;
    private Collider2D boxcollider2d;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    //SE関係
    private AudioSource audioSource;
    [SerializeField] private AudioClip jumpSE;
    [SerializeField] private AudioClip attackSE;
    [SerializeField] private AudioClip damageSE;
    [SerializeField] private AudioClip recoverySE;
    [SerializeField] private AudioClip emptySE;

    //shot関係
    [SerializeField]
    private string AttackKeyCode = "space";
    public GameObject bullet;
    public Transform shotPoint;
    public int remainingBullets { get; set; } //残弾数(自動プロパティ,バックフィールドに_remainingBullets生成)
    float coolTime = 0.2f; //待機時間
    float leftCoolTime; //待機している時間
    private Subject<int> _shot = new Subject<int>();
    public IObservable<int> shot_observable => _shot; //購読する機能(Subscribe)のみを外に公開するため
    private Subject<Unit> _recoveryBullets = new Subject<Unit>();
    public IObservable<Unit> recovery_observable => _recoveryBullets;

    //接地判定関係
    [SerializeField] private LayerMask groundLayer; //for GroundCheck
    
    //ダメージ処理通知
    private Subject<Unit> _warpCheckPoint = new Subject<Unit>();
    public IObservable<Unit> warpCheckPoint_observable => _warpCheckPoint;
    //チェックポイント更新通知
    private Subject<GameObject> _checkpointUpdate = new Subject<GameObject>();
    public IObservable<GameObject> checkpointUpdate_observabele => _checkpointUpdate;

    //プレイヤーUI(向き,アニメーションを操作)
    private GameObject BulletMGCanvas; //回転を無効化する
    private Image baseImage; //UI
    private Image bulletGauge; //UI
    private bool UIcounter = false;
    [SerializeField]
    private GameObject recovery_eff;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxcollider2d = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        BulletMGCanvas = transform.Find("BulletMGCanvas").gameObject;
        baseImage = transform.Find("BulletMGCanvas/BaseImage").GetComponent<Image>(); //子要素のUIを取得
        bulletGauge = transform.Find("BulletMGCanvas/BulletGauge").GetComponent<Image>();
        isRight = true;
    }

    // Update is called once per frame
    void Update()
    {
        Movement(); //動く
        if (!isAttack)
        {
            StartCoroutine(UIAnimationSet());
        }
    }

    //プレイヤーを停止（これ単体では１フレームのみ）
    //この関数を使ったあと、スクリプトをenable = falseにすれば止まる。
    public void Freeze_player() 
    {
        previousGravityScale = rb.gravityScale;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
    }

    //プレイヤーの停止解除（これ単体では１フレームのみ）
    //この関数を使う前にスクリプトをenable = trueにするのを忘れずに！
    public void Unzip_player() //Freeze_playerとセットで使うのが好ましい
    {
        //重力の引き継ぎ
        rb.gravityScale = previousGravityScale;
    }

    //プレイヤーを強制的に下地面接地アイドル状態へ
    public void DefaltCondition_player()
    {
        //重力3、isupフラグfalse,下Idleアニメーションへ
        rb.gravityScale = 3f;
        isup = false;
        isJump = false;
        anim.SetTrigger("event_idle");
    }

    void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        //float y = Input.GetAxis("Vertical");

        if(Mathf.Abs(rb.velocity.y) > 15f) //y軸の速度制限
        {
            if(rb.velocity.y > 0)
            {
                rb.velocity = new Vector2(x * moveSpeed, 15f);
            }
            else
            {
                rb.velocity = new Vector2(x * moveSpeed, -15f);
            }
        }

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
            Freeze_player();
        }
    }

    void Direction(float inputX) //スケールを変える方法だと弾が右方向にしか飛ばないため
    {
        //右向き左入力で180度回転、左向き右入力で180度回転
        if (isRight && inputX < 0)
        {
            transform.Rotate(0, 180f, 0);
            BulletMGCanvas.transform.Rotate(0, 180f, 0); //UIは回転を直す
            isRight = false;
        }
        if (!isRight && inputX > 0)
        {
            transform.Rotate(0, 180f, 0);
            BulletMGCanvas.transform.Rotate(0, 180f, 0); //UIは回転を直す
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
                audioSource.PlayOneShot(jumpSE);
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
                audioSource.PlayOneShot(jumpSE);
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
        if (leftCoolTime <= 0 && remainingBullets > 0) //残り待機時間が0秒以下のとき、かつ残弾数が1以上のとき
        {
            if (Input.GetKeyDown(AttackKeyCode))
            {
                //UI表示
                baseImage.DOFade(1f, 0f).SetLink(gameObject);
                bulletGauge.DOFade(1f, 0f).SetLink(gameObject);
                isAttack = true;
                anim.SetTrigger("shot"); //ショットアニメーション
                audioSource.PlayOneShot(attackSE);
                Instantiate(bullet, shotPoint.position, transform.rotation); //弾を前方に発射する
                _shot.OnNext(1); //1発発射したことを通知
                leftCoolTime = coolTime; //クールタイム発生
            }
        }else if(remainingBullets <= 0) //残弾数が0以下のとき
        {
            if (Input.GetKeyDown(AttackKeyCode))
            {
                //UI表示
                baseImage.DOFade(1f, 0f).SetLink(gameObject);
                bulletGauge.DOFade(1f, 0f).SetLink(gameObject);
                audioSource.PlayOneShot(emptySE); //SE
            }
        }
    }

    public void shotEnd() //shotアニメーションの最終フレームで利用
    {
        isAttack = false;
    }

    //Damage()では主に無敵時間とダメージアニメーションの実装、ストリームの購読中止
    //具体的なダメージ処理はDamageTimer中のonDeadに登録済み(デリゲート)
    private void Damage()
    {
        //既にダメージ状態（＝無敵時間中）なら終了
        if (damage)
        {
            return;
        }
        boxcollider2d.enabled = false;
        audioSource.PlayOneShot(damageSE);
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
        _shot.OnCompleted(); //購読完了（ストリームを終了させるため）
        _recoveryBullets.OnCompleted();　//購読完了（ストリームを終了させるため）
        _checkpointUpdate.OnCompleted(); //購読完了（ストリームを終了させるため）
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
        _warpCheckPoint.OnNext(Unit.Default);
        _warpCheckPoint.OnCompleted();
    }

    //UIフェードアニメーション
    private IEnumerator UIAnimationSet()
    {
        //毎フレーム呼ばれる
        if (isAttack || UIcounter)
        {
            yield break;
        }
        else
        {
            if (baseImage.color.a == 0) //アルファ値0ならそもそもアニメーションを呼ぶ必要ない
            {
                yield break;
            }
            UIcounter = true;
            for(int i=0; i < 30; i++)
            {
                if (isAttack)
                {
                    UIcounter = false;
                    yield break;
                }
                yield return new WaitForSeconds(0.1f);
            }
            //プレイヤー待機中も4秒ごとにずっと呼ばれるのでメモリ効率悪そうなのでアルファ値0なら飛ばす
            Sequence sequence = DOTween.Sequence();
            sequence.Append(baseImage.DOFade(0f, 1f));
            sequence.Join(bulletGauge.DOFade(0f, 1f));
            sequence.Play().OnUpdate(() =>
            {
                if (isAttack)
                {
                    sequence.Kill();
                    UIcounter = false;
                    baseImage.DOFade(1f, 0f).SetLink(gameObject);
                    bulletGauge.DOFade(1f, 0f).SetLink(gameObject);
                }
            }).OnComplete(() => UIcounter = false).SetLink(gameObject);
        }
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
            _checkpointUpdate.OnNext(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("RecoveryPoint"))
        {
            //UI表示
            baseImage.DOFade(1f, 0f).SetLink(gameObject);
            bulletGauge.DOFade(1f, 0f).SetLink(gameObject);
            audioSource.PlayOneShot(recoverySE); //SE
            Instantiate(recovery_eff, transform.position, transform.rotation); //回復eff生成
            _recoveryBullets.OnNext(Unit.Default); //RecoveryPoint通過の通知
        }
    }

    //プレイヤーの当たり判定
    private void OnCollisionEnter2D(UnityEngine.Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Trap") || collision.gameObject.CompareTag("Enemy"))
        {
            Damage();
        }
    }
}
