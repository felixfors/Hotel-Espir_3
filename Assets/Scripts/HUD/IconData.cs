using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(fileName = "InputSpriteData", menuName = "ScriptableObjects/InputSpriteData", order = 2)]
public class IconData : ScriptableObject
{
    public List<GamepadSprites> gamepadSprites = new();
    [System.Serializable]
    public class GamepadSprites
    {
        public string inputName;
        public Sprite xboxSprite;
        public Sprite playstationSprite;
    }

    public List<KeyboardSprites> keyboardSprites = new();
    [System.Serializable]
    public class KeyboardSprites
    {
        public string inputName;
        public Sprite sprite;
    }
}
