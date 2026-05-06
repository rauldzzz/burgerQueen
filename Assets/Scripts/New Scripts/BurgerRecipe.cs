using UnityEngine;
using System.Collections.Generic;

// Esto crea una opción en el menú de Unity al hacer clic derecho
[CreateAssetMenu(fileName = "New Burger Recipe", menuName = "Burger Game/Burger Recipe")]
public class BurgerRecipe : ScriptableObject
{
    public string burgerName;
    public GameObject burger; // Los prefabs de carne, pan, queso, etc.
    public int reward; // El dinero que ganas
}