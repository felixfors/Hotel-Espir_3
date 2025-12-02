using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class JournalHUD : MonoBehaviour
{
    public Image[] categoryButton; // 0 left, 1 right
    private List<Sprite> categoryButton_spriteRegular = new();
    public Sprite [] categoryButton_Pressed;


    private void Awake()
    {
        categoryButton_spriteRegular.Add(categoryButton[0].sprite);
        categoryButton_spriteRegular.Add(categoryButton[1].sprite);
    }
    public void PressedCategoryButton(bool _left)
    {
        if(_left)
        {
            categoryButton[0].sprite = categoryButton_Pressed[0];          
        }
        else
        {
            categoryButton[1].sprite = categoryButton_Pressed[1];
        }
        StartCoroutine(ResetButtonNormalWithDelay(_left)); // Fördröjning
    }
    private IEnumerator ResetButtonNormalWithDelay(bool _left)
    {
        yield return new WaitForSeconds(0.05f);

        if (_left)
        {
            categoryButton[0].sprite = categoryButton_spriteRegular[0];
        }
        else
        {
            categoryButton[1].sprite = categoryButton_spriteRegular[1];
        }
    }
}
