using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NoteData", menuName = "ScriptableObjects/NewNote", order = 1)]
public class NoteData : ScriptableObject
{
    public string noteTitle;
    public int noteID;
    public NoteCategory noteCategory;
    public enum NoteCategory { clues, lore };
}
