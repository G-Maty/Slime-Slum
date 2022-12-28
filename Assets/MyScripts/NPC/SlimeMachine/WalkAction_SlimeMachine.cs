using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arbor;
using Arbor.BehaviourTree;
using DG.Tweening;

[AddComponentMenu("")]
public class WalkAction_SlimeMachine : ActionBehaviour {

    //SerializeField : ArborのBehaviorTree側で設定

    private Animator anim;
    [SerializeField]
    private float walk_second;
    [SerializeField]
    private float walk_interval;
    [SerializeField]
    private float walk_distans;
    private GameObject BossCanvas; //反転を防ぐため
    private bool actionCompleted = false; //アクションが一通り終了したらtrue

    protected override void OnAwake()
    {
    }

    protected override void OnStart()
    {
        anim = GetComponent<Animator>();
        BossCanvas = transform.Find("BossCanvas").gameObject;
        //DOTweenで動作
        var sequence = DOTween.Sequence();
        sequence.Append(this.transform.DOLocalMoveX(-walk_distans, walk_second).SetEase(Ease.Linear).SetRelative(true).SetLink(gameObject));
        sequence.AppendInterval(walk_interval);
        sequence.Append(this.transform.DOScaleX(-1, 0));
        sequence.Append(BossCanvas.transform.DOScaleX(-1, 0)); //キャンバスの反転を防ぐ
        sequence.Append(this.transform.DOLocalMoveX(walk_distans, walk_second).SetEase(Ease.Linear).SetRelative(true).SetLink(gameObject));
        sequence.Append(this.transform.DOScaleX(1, 0));
        sequence.Append(BossCanvas.transform.DOScaleX(1, 0)); //キャンバスの反転を防ぐ
        sequence.Play().OnStart(() => anim.SetBool("walk", true)).OnComplete(() =>
        {
            actionCompleted = true;
        }).SetLink(this.gameObject);
    }

    protected override void OnExecute()
    {
        if (actionCompleted)
        {
            actionCompleted = false; //フラグを元に戻す
            FinishExecute(true); //BehaviorTreeにtrueを返してアクション終了
        }
    }

    protected override void OnEnd()
    {
    }

}
