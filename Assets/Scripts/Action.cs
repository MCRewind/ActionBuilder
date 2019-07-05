using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu]
public class Action : ScriptableObject
{
    [SerializeField] public Dictionary<string, List<Box>> Boxes;
}