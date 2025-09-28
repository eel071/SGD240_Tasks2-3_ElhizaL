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

    protected BaseNavigation Navigation;

    protected BaseInteraction CurrentInteraction = null;

    protected bool StartedPerforming = false;

    public float CurrentHunger { get; protected set; }
    public float CurrentEnergy { get; protected set; }
    public float CurrentBladder { get; protected set; }
    public float CurrentFun { get; protected set; }


    protected virtual void Awake()
    {
        HungerDisplay.value = CurrentHunger = InitialHungerLevel;
        EnergyDisplay.value = CurrentEnergy = InitialEnergyLevel;
        BladderDisplay.value = CurrentBladder = InitialBladderLevel;
        FunDisplay.value = CurrentFun = InitialFunLevel;

        Navigation = GetComponent<BaseNavigation>();
    }
    // Start is called before the first frame update
    protected virtual void Start()
    {

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

        CurrentHunger = Mathf.Clamp01(CurrentHunger - BaseHungerDecayRate * Time.deltaTime);
        HungerDisplay.value = CurrentHunger;

        CurrentEnergy = Mathf.Clamp01(CurrentEnergy - BaseEnergyDecayRate * Time.deltaTime);
        EnergyDisplay.value = CurrentEnergy;

        CurrentBladder = Mathf.Clamp01(CurrentBladder - BaseBladderDecayRate * Time.deltaTime);
        BladderDisplay.value = CurrentBladder;

        CurrentFun = Mathf.Clamp01(CurrentFun - BaseFunDecayRate * Time.deltaTime);
        FunDisplay.value = CurrentFun;

    }

    protected virtual void OnInteractionFinished(BaseInteraction interaction)
    {
        interaction.UnlockInteraction();
        CurrentInteraction = null;
        Debug.Log($"Finished {interaction.DisplayName}");
    }

    public void UpdateIndividualStat(EStat target, float amount)
    {
        switch(target)
        {
            case EStat.Hunger: CurrentHunger += amount; break;
            case EStat.Energy: CurrentEnergy += amount; break;
            case EStat.Bladder: CurrentBladder += amount; break;
            case EStat.Fun: CurrentFun += amount; break;
        }
    }
}
