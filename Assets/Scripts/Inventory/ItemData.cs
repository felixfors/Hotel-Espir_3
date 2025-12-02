using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/NewItem", order = 1)]
public class ItemData : ScriptableObject
{
    public string itemName;
    public bool usable;
    public bool destroyOnUse;
    public bool twoHanded;
    public AudioClip[] pickUpSound;
    public AudioClip useSound;
    public int animationID;
    public ItemCategory itemCategory;
    public enum ItemCategory { _default, disc, banishmentbox};
}
