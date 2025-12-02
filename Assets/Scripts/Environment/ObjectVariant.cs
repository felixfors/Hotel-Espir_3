using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectVariant : MonoBehaviour
{
    public Transform variantParent;

    public int objectFromList;

    public List<GameObject> variants = new();


    

    public Vector3 objectRotation;

    [ContextMenu("GetRotation")]
    public void GetRotation()
    {
        objectRotation = variantParent.localEulerAngles;
    }

        [ContextMenu("GetVariants")]
    public void GetVariants()
    {
        variants.Clear();
        foreach (Transform child in variantParent.transform)
        {
            variants.Add(child.gameObject);
        }
    }
    [ContextMenu("ChooseVariant")]
    public void ChooseVariant()
    {
        if (variants.Count == 0)
            return;

        
        //Rätta till rotation på variant om prefaben har ändrats
        variantParent.localEulerAngles = objectRotation;

        string rename = variants[objectFromList].transform.name;
        // välj variant
      
        GameObject newObj = Instantiate(variants[objectFromList], variants[objectFromList].transform.position, variants[objectFromList].transform.rotation);
        newObj.transform.parent = variantParent;
        newObj.transform.name = rename;
        newObj.SetActive(true);


        foreach (GameObject obj in variants)
        {
            DestroyImmediate(obj);
        }
        variants.Clear();
    }
}
