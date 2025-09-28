using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(BaseNavigation))]

public class NotSoSimpleAI : CommonAIBase
{
    [SerializeField] protected float PickInteractionInterval = 2f;
    [SerializeField] protected float DefaultInteractionScore = 0f;
    [SerializeField] protected int InteractionPickSize = 5;

    protected float TimeUntilNextInteractionPicked = -1f;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (CurrentInteraction == null)
        {
            TimeUntilNextInteractionPicked -= Time.deltaTime;

            //time to pick an interaction
            if (TimeUntilNextInteractionPicked <= 0)
            {
                TimeUntilNextInteractionPicked = PickInteractionInterval;
                PickBestInteraction();
            }
        }
    }
        
    float ScoreInteraction(BaseInteraction interaction)
    {
        if (interaction.StatChanges.Length == 0)
        {
            return DefaultInteractionScore;
        }

        float score = 0f;

        foreach(var change in interaction.StatChanges)
        {
            score += ScoreChange(change.Target, change.Value);
        }
        return score;
    }

    float ScoreChange(EStat target, float amount)
    {
        float currentValue = 0f;
        
        switch (target)
        {
            case EStat.Hunger: currentValue = CurrentHunger; break;
            case EStat.Energy: currentValue = CurrentEnergy; break;
            case EStat.Bladder: currentValue = CurrentBladder; break;
            case EStat.Fun: currentValue = CurrentFun; break;
        }

        //return value through an equation to get the utility score
        //to be further complicated at a later date (this is where curves and such would be put in)
        return (1f - currentValue) * amount;
    }

    class ScoredInteraction
    {
        public SmartObject TargetObject;
        public BaseInteraction Interaction;
        public float Score;
    }

    void PickBestInteraction()
    {
        //loop through all objects
        List<ScoredInteraction> unsortedInteractions = new List<ScoredInteraction>();
        foreach(var smartObject in SmartObjectManager.Instance.RegisteredObjects)
        {
            //loop through all interactions
            foreach(var interaction in smartObject.Interactions)
            {
                if (!interaction.CanPerform()) { continue; }

                float score = ScoreInteraction(interaction);

                unsortedInteractions.Add(new ScoredInteraction { TargetObject = smartObject, Interaction = interaction, Score = score });
            }
        }

        if (unsortedInteractions.Count == 0) { return; }
        
        //sort and pick from list of best interactions
        var sortedInteractions = unsortedInteractions.OrderByDescending(scoredInteractions => scoredInteractions.Score).ToList();
        int maxIndex = Mathf.Min(InteractionPickSize, sortedInteractions.Count);

        var selectedIndex = Random.Range(0, maxIndex);

        var selectedObject = sortedInteractions[selectedIndex].TargetObject;
        var selectedInteraction = sortedInteractions[selectedIndex].Interaction;

        CurrentInteraction = selectedInteraction;
        CurrentInteraction.LockInteraction();
        StartedPerforming = false;

        //move to the target
        if (!Navigation.SetDestination(selectedObject.InteractionPoint))
        {
            Debug.LogError($"Could not move to {selectedObject.name}");
            CurrentInteraction = null;
        }
        else
            Debug.Log($"Going to {CurrentInteraction.DisplayName} at {selectedObject.DisplayName}");               
    }

}
