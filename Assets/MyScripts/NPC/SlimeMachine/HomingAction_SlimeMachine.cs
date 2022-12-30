using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Arbor;
using Arbor.BehaviourTree;

[AddComponentMenu("")]
public class HomingAction_SlimeMachine : ActionBehaviour {

    private Animator anim;
    private SlimeMachine slimemachine;
    private bool actionCompleted = false; //アクションが一通り終了したらtrue
    [SerializeField]
    private int shot_count;
    [SerializeField]
    private float shot_cooltime;

    protected override void OnAwake() {
	}

	protected override void OnStart() {
        anim = GetComponent<Animator>();
        slimemachine = GetComponent<SlimeMachine>();
        StartCoroutine(homing());
    }

	protected override void OnExecute() {
        if (actionCompleted)
        {
            actionCompleted = false;
            FinishExecute(true); //必須
        }
    }

	protected override void OnEnd() {
	}

    IEnumerator homing()
    {
        anim.SetBool("attack", true);
        yield return new WaitForSeconds(1);
        for (int i = 0; i < shot_count; i++)
        {
            if (slimemachine.IsBossBreak)
            {
                actionCompleted = true;
                yield break;
            }
            slimemachine.SlimeMachine_homingshot();
            slimemachine.SEplayOneShot("shot"); //SE
            yield return new WaitForSeconds(shot_cooltime);
        }
        actionCompleted = true;
    }

}
