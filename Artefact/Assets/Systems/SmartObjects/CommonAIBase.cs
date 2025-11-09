using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AIStatConfiguration
{
    [field: SerializeField] public AIStat LinkedStat {  get; private set; }
    [field: SerializeField] public bool OverrideDefaults { get; private set; } = false;
    [field: SerializeField, Range(0f, 1f)] public float Override_InitialValue { get; protected set; } = 0.5f;
    [field: SerializeField, Range(0f, 1f)] public float Override_DecayRate { get; protected set; } = 0.005f;
    [field: SerializeField, Range(1, 5)] public int Override_HierarchyLevel { get; protected set; } = 1;
}

[RequireComponent(typeof(BaseNavigation))]
public class CommonAIBase : MonoBehaviour
{
    [Header("General")]
    [SerializeField] int HouseholdID = 1;
    [field: SerializeField] AIStatConfiguration[] Stats;
    [SerializeField] protected FeedbackUIPanel LinkedUI;

    [Header("Traits")]
    [SerializeField] protected List<Trait> Traits;

    protected BaseNavigation Navigation;

    protected bool StartedPerforming = false;

    public Blackboard IndividualBlackboard { get; protected set; }
    public Blackboard HouseholdBlackboard { get; protected set; }

    protected Dictionary<AIStat, float> DecayRates = new Dictionary<AIStat, float>();
    protected Dictionary<AIStat, AIStatPanel> StatUIPanels = new Dictionary<AIStat, AIStatPanel>();

    protected BaseInteraction CurrentInteraction
    {
        get 
        { 
            BaseInteraction interaction = null;
            IndividualBlackboard.TryGetGeneric(EBlackboardKey.Character_FocusObject, out interaction, null);
            return interaction; 
        }
        set 
        {
            BaseInteraction prevInteraction = null;
            IndividualBlackboard.TryGetGeneric(EBlackboardKey.Character_FocusObject, out prevInteraction, null);
            
            IndividualBlackboard.SetGeneric(EBlackboardKey.Character_FocusObject, value);
            
            List<GameObject> objectsInUse = null;
            HouseholdBlackboard.TryGetGeneric(EBlackboardKey.Household_ObjectsInUse, out objectsInUse, null);

            //are we starting to use something?
            if (value != null)
            {
                //need to create a list?
                if (objectsInUse == null)
                {
                    objectsInUse = new List<GameObject>();
                }

                // not already in list? add and update blackboard
                if (!objectsInUse.Contains(value.gameObject))
                {
                    objectsInUse.Add(value.gameObject);
                    HouseholdBlackboard.SetGeneric(EBlackboardKey.Household_ObjectsInUse, objectsInUse);
                }
            }
            //we've stopped using something
            else if (objectsInUse != null)
            {
                //attempt to remove and update blackboard if changed
                if (objectsInUse.Remove(prevInteraction.gameObject))
                {
                    HouseholdBlackboard.SetGeneric(EBlackboardKey.Household_ObjectsInUse, objectsInUse);
                }
            }
            
        }
    }
     

    protected virtual void Awake()
    {      
        Navigation = GetComponent<BaseNavigation>();
    }
        
    // Start is called before the first frame update
    protected virtual void Start()
    {
        HouseholdBlackboard = BlackboardManager.Instance.GetSharedBlackboard(HouseholdID);
        IndividualBlackboard = BlackboardManager.Instance.GetIndividualBlackboard(this);

        //set up stats
        foreach (var statConfig in Stats)
        {
            var linkedStat = statConfig.LinkedStat;
            float initialValue = statConfig.OverrideDefaults ? statConfig.Override_InitialValue : statConfig.LinkedStat.InitialValue;
            float decayRate = statConfig.OverrideDefaults ? statConfig.Override_DecayRate : statConfig.LinkedStat.DecayRate;
            int hierarchyLevel = statConfig.OverrideDefaults ? statConfig.Override_HierarchyLevel : statConfig.LinkedStat.HierarchyLevel;

            DecayRates[linkedStat] = decayRate;
            IndividualBlackboard.SetStat(linkedStat, initialValue);
            
            if (linkedStat.IsVisible)
            {
                StatUIPanels[linkedStat] = LinkedUI.AddStat(linkedStat, initialValue);
            }
        }
    }

    protected float ApplyTraitsTo(AIStat targetStat, Trait.ETargetType targetType, float currentValue)
    {
        foreach(var trait in Traits)
        {
            currentValue = trait.Apply(targetStat, targetType, currentValue);
        }

        return currentValue;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (CurrentInteraction != null)
        {
            if (Navigation.IsAtDestination && !StartedPerforming)
            {
                StartedPerforming = true;
                CurrentInteraction.Perform(this, OnInteractionFinished);
            }

        }        
        //apply decay rate
        foreach(var statConfig in Stats)
        {
            UpdateIndividualStat(statConfig.LinkedStat, -DecayRates[statConfig.LinkedStat] * Time.deltaTime, Trait.ETargetType.DecayRate);
        }
        
    }

    protected virtual void OnInteractionFinished(BaseInteraction interaction)
    {
        interaction.UnlockInteraction(this);
        CurrentInteraction = null;
        Debug.Log($"Finished {interaction.DisplayName}");
    }

    public void UpdateIndividualStat(AIStat linkedStat, float amount, Trait.ETargetType targetType)
    {
        float adjustedAmount = ApplyTraitsTo(linkedStat, targetType, amount);
        float newValue = Mathf.Clamp01(GetStatValue(linkedStat) + adjustedAmount);

        IndividualBlackboard.SetStat(linkedStat, newValue);
        
        if (linkedStat.IsVisible)
            StatUIPanels[linkedStat].OnStatChanged(newValue);
    }

    public float GetStatValue(AIStat linkedStat)
    {
        return IndividualBlackboard.GetStat(linkedStat);
    }
}
