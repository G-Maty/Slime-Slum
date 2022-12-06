using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arbor;
using Arbor.BehaviourTree;
using UniRx;
using UniRx.Triggers;
using System;

/*
 * SlimeMachine撃破時の演出
 * DeathActionに一回入ればいいので基本的にOnAwakeに記述
 * OnStartに記述するとBehaviorTree側で永遠に呼ばれる
 */

[AddComponentMenu("")]
public class DeathAction_SlimeMachine : ActionBehaviour {

	private Animator anim;
	private bool actionCompleted = false;
	private SlimeMachine slimemachine;
	private ObservableStateMachineTrigger animEndTrigger; //アニメーション終了の通知のため
	//撃破演出完了を通知するため
	private Subject<Unit> ButtleFinSubject = new Subject<Unit>();
	public IObservable<Unit> ButtleFin //購読側だけ公開
	{
		get { return ButtleFinSubject; }
	}

	protected override void OnAwake() {
        anim = GetComponent<Animator>();
        slimemachine = GetComponent<SlimeMachine>();
        animEndTrigger = anim.GetBehaviour<ObservableStateMachineTrigger>();
        anim.SetTrigger("defeat");
		slimemachine.IsBossBreak = true;
		//撃破アニメーションが終了すると透明化する
        animEndTrigger.OnStateExitAsObservable()
            .Where(x => x.StateInfo.IsName("SlimeMachine_defeat"))
            .Subscribe(x =>
            {
				slimemachine.BossFadeOut(); //透明にする
                actionCompleted = true;
            }).AddTo(this);
    }

	protected override void OnStart() {
		anim.SetBool("walk",false);
        anim.SetBool("attack", false);
        //敵の移動をストップさせたい
    }

    protected override void OnExecute() {
		if (actionCompleted)
		{
			actionCompleted = false;
            FinishExecute(true);
        }
    }

	protected override void OnEnd() {
		ButtleFinSubject.OnNext(Unit.Default);
		ButtleFinSubject.OnCompleted();
	}
	
}
