using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Doors))]
public class DoorEditor : Editor
{
    private void OnSceneGUI()
    {
        Doors door = (Doors)target;
        if (door.hinge == null) return;

        Transform hinge = door.hinge;

        // Hämta nuvarande värden
        float minY = door.doorClamp.x;
        float maxY = door.doorClamp.y;

        // Lokala riktningar
        Vector3 localForward = Vector3.forward;

        // Rita "spöklinjer" för visualisering (lokalt)
        Handles.color = Color.red;
        Handles.DrawLine(
            hinge.TransformPoint(Vector3.zero),
            hinge.TransformPoint(Quaternion.Euler(0, minY, 0) * localForward * 1f)
        );

        Handles.color = Color.green;
        Handles.DrawLine(
            hinge.TransformPoint(Vector3.zero),
            hinge.TransformPoint(Quaternion.Euler(0, maxY, 0) * localForward * 1f)
        );

        // Rita Handles för justering (lokalt)
        Handles.color = Color.red;
        Vector3 localMinDir = Quaternion.Euler(0, minY, 0) * localForward;
        Vector3 worldMinPos = hinge.TransformPoint(localMinDir * 1f);

        Vector3 newMinWorldPos = Handles.FreeMoveHandle(
            worldMinPos,
            0.1f,
            Vector3.zero,
            Handles.SphereHandleCap
        );
        Handles.Label(newMinWorldPos, "Min Y");

        Handles.color = Color.green;
        Vector3 localMaxDir = Quaternion.Euler(0, maxY, 0) * localForward;
        Vector3 worldMaxPos = hinge.TransformPoint(localMaxDir * 1f);

        Vector3 newMaxWorldPos = Handles.FreeMoveHandle(
            worldMaxPos,
            0.1f,
            Vector3.zero,
            Handles.SphereHandleCap
        );
        Handles.Label(newMaxWorldPos, "Max Y");

        // Uppdatera värden om användaren flyttar handles
        if (GUI.changed)
        {
            Undo.RecordObject(door, "Adjust Door Rotation Limits");

            // Konvertera världspunkter tillbaka till lokala
            Vector3 newMinLocalDir = hinge.InverseTransformPoint(newMinWorldPos).normalized;
            Vector3 newMaxLocalDir = hinge.InverseTransformPoint(newMaxWorldPos).normalized;

            door.doorClamp.x = Quaternion.LookRotation(newMinLocalDir, Vector3.up).eulerAngles.y;
            door.doorClamp.y = Quaternion.LookRotation(newMaxLocalDir, Vector3.up).eulerAngles.y;

            // Hantera värden som wrappar över 360°
            if (door.doorClamp.x > 180f) door.doorClamp.x -= 360f;
            if (door.doorClamp.y > 180f) door.doorClamp.y -= 360f;

            EditorUtility.SetDirty(door);
        }
    }
}