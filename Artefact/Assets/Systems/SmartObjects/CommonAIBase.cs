using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EStat
{
    Hunger,
    Energy,
    Bladder,
    Fun
}

[RequireComponent(typeof(BaseNavigation))]
public class CommonAIBase : MonoBehaviour
{
    [Header("General")]
    [SerializeField] int HouseholdID = 1;

    [Header("Hunger")]
    [SerializeField] float InitialHungerLevel = 0.5f;
    [SerializeField] float BaseHungerDecayRate = 0.005f;
    [SerializeField] UnityEngine.UI.Slider HungerDisplay;

    [Header("Energy")]
    [SerializeField] float InitialEnergyLevel = 0.5f;
    [SerializeField] float BaseEnergyDecayRate = 0.001f;
    [SerializeField] UnityEngine.UI.Slider EnergyDisplay;

    [Header("Bladder")]
    [SerializeField] float InitialBladderLevel = 0.5f;
    [SerializeField] float BaseBladderDecayRate = 0.005f;
    [SerializeField] UnityEngine.UI.Slider BladderDisplay;

    [Header("Fun")]
    [SerializeField] float InitialFunLevel = 0.5f;
    [SerializeField] float BaseFunDecayRate = 0.005f;
    [SerializeField] UnityEngine.UI.Slider FunDisplay;

    [Header("Traits")]
    [SerializeField] protected List<Trait> Traits;

    protected BaseNavigation Navigation;

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

    protected bool StartedPerforming = false;

    public float CurrentHunger
    {
        get { return IndividualBlackboard.GetFloat(EBlackboardKey.Character_Stat_Hunger); }
        set { IndividualBlackboard.Set(EBlackboardKey.Character_Stat_Hunger, value); }
    }
    public float CurrentEnergy
    {
        get { return IndividualBlackboard.GetFloat(EBlackboardKey.Character_Stat_Energy); }
        set { IndividualBlackboard.Set(EBlackboardKey.Character_Stat_Energy, value); }
    }
    public float CurrentBladder
    {
        get { return IndividualBlackboard.GetFloat(EBlackboardKey.Character_Stat_Bladder); }
        set { IndividualBlackboard.Set(EBlackboardKey.Character_Stat_Bladder, value); }
    }
    public float CurrentFun
    {
        get { return IndividualBlackboard.GetFloat(EBlackboardKey.Character_Stat_Fun); }
        set { IndividualBlackboard.Set(EBlackboardKey.Character_Stat_Fun, value); }
    }

    public Blackboard IndividualBlackboard { get; protected set; }
    public Blackboard HouseholdBlackboard { get; protected set; }

    protected virtual void Awake()
    {      
        Navigation = GetComponent<BaseNavigation>();
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        HouseholdBlackboard = BlackboardManager.Instance.GetSharedBlackboard(HouseholdID);
        IndividualBlackboard = BlackboardManager.Instance.GetIndividualBlackboard(this);

        HungerDisplay.value = CurrentHunger = InitialHungerLevel;
        EnergyDisplay.value = CurrentEnergy = InitialEnergyLevel;
        BladderDisplay.value = CurrentBladder = InitialBladderLevel;
        FunDisplay.value = CurrentFun = InitialFunLevel;
    }

    protected float ApplyTraitsTo(EStat targetStat, Trait.ETargetType targetType, float currentValue)
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
                
        CurrentHunger = Mathf.Clamp01(CurrentHunger - ApplyTraitsTo(EStat.Hunger, Trait.ETargetType.DecayRate, BaseHungerDecayRate) * Time.deltaTime);
        HungerDisplay.value = CurrentHunger;

        CurrentEnergy = Mathf.Clamp01(CurrentEnergy - ApplyTraitsTo(EStat.Energy, Trait.ETargetType.DecayRate, BaseEnergyDecayRate) * Time.deltaTime);
        EnergyDisplay.value = CurrentEnergy;

        CurrentBladder = Mathf.Clamp01(CurrentBladder - ApplyTraitsTo(EStat.Bladder, Trait.ETargetType.DecayRate, BaseBladderDecayRate) * Time.deltaTime);
        BladderDisplay.value = CurrentBladder;

        CurrentFun = Mathf.Clamp01(CurrentFun - ApplyTraitsTo(EStat.Fun, Trait.ETargetType.DecayRate, BaseFunDecayRate) * Time.deltaTime);
        FunDisplay.value = CurrentFun;

    }

    protected virtual void OnInteractionFinished(BaseInteraction interaction)
    {
        interaction.UnlockInteraction(this);
        CurrentInteraction = null;
        Debug.Log($"Finished {interaction.DisplayName}");
    }

    public void UpdateIndividualStat(EStat target, float amount)
    {
        float adjustedAmount = ApplyTraitsTo(target, Trait.ETargetType.Impact, amount);

        switch (target)
        {
            case EStat.Hunger: CurrentHunger = Mathf.Clamp01(CurrentHunger + adjustedAmount); break;
            case EStat.Energy: CurrentEnergy = Mathf.Clamp01(CurrentEnergy + adjustedAmount); break;
            case EStat.Bladder: CurrentBladder = Mathf.Clamp01(CurrentBladder + adjustedAmount); break;
            case EStat.Fun: CurrentFun = Mathf.Clamp01(CurrentFun + adjustedAmount); break;
        }
    }
}
