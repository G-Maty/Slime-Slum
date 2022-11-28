using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using Fungus;

[CommandInfo("Timeline", "Play Timeline", "Play Timeline.")]
[AddComponentMenu("")]
public class PlayTimelineForFungus : Command
{
    // PlayableDirectorÇ÷ÇÃéQè∆
    [Tooltip("Playable Director")]
    [SerializeField]
    protected PlayableDirector playableDirector;

    // TimelineÇÃçƒê∂Ç™äÆóπÇ∑ÇÈÇ‹Ç≈ë“ã@Ç∑ÇÈ
    [Tooltip("Wait Until Finished")]
    [SerializeField]
    protected BooleanData waitUntilFinished = new BooleanData(true);

    public override void OnEnter()
    {
        if (playableDirector == null)
        {
            Continue();
            return;
        }

        playableDirector.Play();
        if (waitUntilFinished.Value)
        {
            StartCoroutine(WaitTimeline());
        }
        else
        {
            Continue();
        }
    }

    private IEnumerator WaitTimeline()
    {
        while (playableDirector.state == PlayState.Playing) yield return null;
        Continue();
    }

    public override string GetSummary()
    {
        if (playableDirector == null)
        {
            return "Error: No PlayableDirector selected";
        }
        return playableDirector.name;
    }

    public override Color GetButtonColor()
    {
        return new Color32(235, 191, 217, 255);
    }

    public override bool HasReference(Variable variable)
    {
        return base.HasReference(variable) ||
            (waitUntilFinished.booleanRef == variable);
    }
}