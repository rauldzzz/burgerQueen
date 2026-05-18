using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Burger Recipe", menuName = "Burger Game/Burger Recipe")]
public class BurgerRecipe : ScriptableObject
{
    public string burgerName;
    public GameObject burger;
    public int reward;

    [Header("UI Images")]
    public Sprite finalBurgerImage; // La imagen de la hamburguesa completa
    public List<Sprite> ingredientImages; // Lista de imágenes para cada ingrediente
}