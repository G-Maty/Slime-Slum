using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * エフェクトがプレイヤーに追従する挙動
 * 弾丸補充エフェクトPrefabにアタッチ
 */

public class recovery_eff : MonoBehaviour
{
    [SerializeField]
    private float destroyTime;
    private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        StartCoroutine(objDestroy());
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position; //
    }

    IEnumerator objDestroy()
    {
        yield return new WaitForSeconds(destroyTime);
        Destroy(gameObject);
    }
}
