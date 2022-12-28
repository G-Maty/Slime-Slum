using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/*
 * 敵（スピリット）の挙動
 */

public class Spirit : MonoBehaviour
{
    private SpriteRenderer spr;
    private AudioSource audioSource;
    private Collider2D coll;
    [SerializeField] private AudioClip destroySE;

    // Start is called before the first frame update
    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        coll = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            audioSource.PlayOneShot(destroySE);
            coll.enabled = false;
            spr.DOFade(0f, 0.5f).OnComplete(() => Destroy(gameObject)).SetLink(gameObject);
        }
    }
}
