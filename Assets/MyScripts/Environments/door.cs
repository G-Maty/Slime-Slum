using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/*
 * 開閉壁の処理
 * FungusFlowchartコマンドで呼び出し
 * Doorオブジェクトにアタッチ
 * ドアを構成する各ブロックのスプライトを配列に格納
 */

public class door : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer[] doorBlockSprite;
    [Tooltip("デフォルトで壁を出現させるか")]
    public bool isStartSet = true;

    // Start is called before the first frame update
    void Start()
    {
        //デフォルトでドアを出現させない場合は透明化、非アクティブにする
        if (!isStartSet)
        {
            foreach (SpriteRenderer spr in doorBlockSprite)
            {
                spr.color = new Color(255, 255, 255, 0);
                spr.gameObject.SetActive(false);
            }
        }    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void open_door()
    {
        StartCoroutine(StartOpenDoor());
    }

    public void close_door()
    {
        StartCoroutine(StartCloseDoor());
    }

    IEnumerator StartOpenDoor()
    {
        
        for (int i = doorBlockSprite.Length-1; i > -1; i--) //ドアをフェードアウト
        {
            doorBlockSprite[i].DOFade(0, 1).SetLink(gameObject);
            yield return new WaitForSeconds(0.5f);
        }
        for(int j = 0; j < doorBlockSprite.Length; j++) //ドアを非アクティブ
        {
            doorBlockSprite[j].gameObject.SetActive(false);
        }
    }

    IEnumerator StartCloseDoor()
    {
        foreach (SpriteRenderer spr in doorBlockSprite) //ドアをアクティブ
        {
            spr.gameObject.SetActive(true);
        }

        foreach (SpriteRenderer spr in doorBlockSprite) //ドアをフェードイン
        {
            spr.DOFade(1, 1).SetLink(gameObject);
            yield return new WaitForSeconds(0.5f);
        }
    }
}
