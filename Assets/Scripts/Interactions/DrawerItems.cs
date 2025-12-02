using UnityEngine;

public class DrawerItems : MonoBehaviour
{
    public Transform itemHolder;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Interactable" && other.gameObject.layer == 9)
        {
            other.transform.parent = itemHolder;
        }
    }
}
