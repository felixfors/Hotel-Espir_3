using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObject : MonoBehaviour
{
    public List<GameObject> variants = new();
    private int currentVariant = -1;

    [ContextMenu("RandomVariant")] // Måste köras om vid varje nytt tillägg
    private void RandomVariant()
    {
        int randomSelection = Random.Range(0,variants.Count);
        for(int i = 0; i < variants.Count; i ++)
        {
            if(i == randomSelection)
            {
                variants[i].SetActive(true);
            }
            else
            {
                variants[i].SetActive(false);
            }
        }
    }
    [ContextMenu("RandomPrefab")]
    private void RandomPrefab()
    {
        foreach (Transform child in gameObject.transform)
        {
            DestroyImmediate(child.gameObject);
        }

        List<int> possibleIndices = new();
        for (int i = 0; i < variants.Count; i++)
        {
            if (i != currentVariant)
                possibleIndices.Add(i);
        }

        currentVariant = possibleIndices[Random.Range(0, possibleIndices.Count)];

        GameObject newObj = Instantiate(variants[currentVariant], transform.position, transform.rotation);
        newObj.transform.parent = transform;
        newObj.SetActive(true);
    }

}
