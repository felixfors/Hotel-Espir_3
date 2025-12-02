using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShadowQuality", menuName = "ScriptableObjects/ShadowQuality", order = 1)]


public class ShadowQuality : ScriptableObject
{
    public List<Cascade> cascade = new();
    [System.Serializable]
    public class Cascade
    {
        public float maxDistance = 25f;
        public float updateRate = 0.1f;
    }
}
