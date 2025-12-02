using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRigidbodyCollision : MonoBehaviour
{
    public float pushPower = 2.0f;
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {

        if (hit.collider.attachedRigidbody && hit.gameObject.layer != 22 && hit.gameObject.layer != 13)
            PushRigidbody(hit);
        
        PushDoor(hit);
        
    }
    private void PushRigidbody(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic)
        {
            return;
        }
        // We dont want to push objects below us
        if (hit.moveDirection.y < 0.3f)
        {
            //return;
        }
        Debug.Log("Vi träffade något");
        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // If you know how fast your character is trying to move,
        // then you can also multiply the push velocity by that.

        // Apply the push
        Debug.Log("Vi träffar " + body.transform.name);
        body.velocity = pushDir * pushPower;

    }
    private void PushDoor(ControllerColliderHit hit)
    {
        if (!PlayerController.instance.isMoving)
            return;

        if (hit.transform.tag == "Interactable")
        {
            if (!hit.transform.GetComponent<Doors>())
                return;
            Doors door = hit.transform.GetComponent<Doors>();

            if (door.locked)
                return;

            if (door.rotation == 0)
                return;
            float playerSpeed = PlayerController.instance.movementSpeed;
            playerSpeed /= 100;
            door.PushDoor(playerSpeed * pushPower, PlayerController.instance.transform);
        }
    }
}
