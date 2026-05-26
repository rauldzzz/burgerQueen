using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Burger Assembly Recipe", menuName = "Burger Game/Burger Assembly Recipe")]
public class BurgerAssemblyRecipe : ScriptableObject
{
    [System.Serializable]
    public class BurgerAssemblyStep
    {
        public GameObject currentStatePrefab;
        public GameObject requiredIngredientPrefab;
        public GameObject nextStatePrefab;
    }

    public string burgerName;
    public GameObject startingStatePrefab;
    public List<BurgerAssemblyStep> steps = new List<BurgerAssemblyStep>();

    public bool TryGetStep(string currentStateName, string heldItemName, out BurgerAssemblyStep step)
    {
        string normalizedCurrentStateName = NormalizePrefabName(currentStateName);
        string normalizedHeldItemName = NormalizePrefabName(heldItemName);

        for (int i = 0; i < steps.Count; i++)
        {
            BurgerAssemblyStep candidate = steps[i];

            if (candidate == null) continue;
            if (candidate.currentStatePrefab == null) continue;
            if (candidate.requiredIngredientPrefab == null) continue;
            if (candidate.nextStatePrefab == null) continue;

            if (NormalizePrefabName(candidate.currentStatePrefab.name) != normalizedCurrentStateName) continue;
            if (NormalizePrefabName(candidate.requiredIngredientPrefab.name) != normalizedHeldItemName) continue;

            step = candidate;
            return true;
        }

        step = null;
        return false;
    }

    public bool TryGetStepByNextState(string nextStateName, out BurgerAssemblyStep step)
    {
        string normalizedNextStateName = NormalizePrefabName(nextStateName);

        for (int i = 0; i < steps.Count; i++)
        {
            BurgerAssemblyStep candidate = steps[i];

            if (candidate == null) continue;
            if (candidate.currentStatePrefab == null) continue;
            if (candidate.requiredIngredientPrefab == null) continue;
            if (candidate.nextStatePrefab == null) continue;

            if (NormalizePrefabName(candidate.nextStatePrefab.name) != normalizedNextStateName) continue;

            step = candidate;
            return true;
        }

        step = null;
        return false;
    }

    private static string NormalizePrefabName(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return name.Replace("(Clone)", "").Trim();
    }
}