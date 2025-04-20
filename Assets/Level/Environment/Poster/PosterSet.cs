using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PosterSet", menuName = "Level/Environment/Poster Set", order = 0)]
public class PosterSet : ScriptableObject
{
    [SerializeField] public List<Texture> Posters = new();
}
