using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatBody : MonoBehaviour
{
    [Header("Ledare och följare")]
    public Transform leader;               // Första länken eller RatAgent
    public List<Transform> followers;      // Resten av kuberna

    [Header("Inställningar")]
    public float followDistance = 0.5f;    // Avstånd mellan varje kub
    public float moveSpeed = 10f;          // Hur snabbt de glider
    public float rotateSpeed = 10f;        // Hur snabbt de vrider sig
    public float recordMinDistance = 0.05f;// Minsta rörelse för att spara punkt

    [Header("Svansgunga")]
    public int swingStartIndex = 5;        // Första kuben som börjar gunga
    public float swingAmplitude = 0.1f;    // Hur mycket de gungar i sidled
    public float swingFrequency = 5f;      // Hastighet på gungan

    [Header("Offsets")]
    public List<float> yOffsets = new List<float>(); // Manuell Y-offset för varje follower

    private List<Vector3> history = new List<Vector3>();

    void Start()
    {
        // Lägg in startpositionen
        history.Insert(0, leader.position);
    }

    void Update()
    {
        // Spara en ny punkt bara om ledaren har rört sig en bit
        if (Vector3.Distance(history[0], leader.position) > recordMinDistance)
        {
            history.Insert(0, leader.position);
        }

        // Flytta och rotera varje följare
        for (int i = 0; i < followers.Count; i++)
        {
            int index = Mathf.RoundToInt((followDistance / recordMinDistance) * (i + 1));

            if (index < history.Count)
            {
                Transform follower = followers[i];
                Vector3 point = history[index];

                // Använd manuell Y-offset
                float yOffset = (i < yOffsets.Count) ? yOffsets[i] : 0f;
                Vector3 targetPos = point + Vector3.up * yOffset;

                // Flytta mot historikpunkten
                follower.position = Vector3.Lerp(
                    follower.position,
                    targetPos,
                    Time.deltaTime * moveSpeed
                );

                // Om denna follower är efter "swingStartIndex" -> lägg på gung
                if (i >= swingStartIndex)
                {
                    float offset = Mathf.Sin(Time.time * swingFrequency + i) * swingAmplitude;
                    Vector3 side = follower.right;
                    follower.position += side * offset;
                }

                // Rotera mot kuben framför
                Transform lookTarget = (i == 0) ? leader : followers[i - 1];
                Vector3 dir = lookTarget.position - follower.position;

                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir);
                    follower.rotation = Quaternion.Slerp(
                        follower.rotation,
                        targetRot,
                        Time.deltaTime * rotateSpeed
                    );
                }
            }
        }

        // Håll historiken rimligt lång
        if (history.Count > 2000)
        {
            history.RemoveRange(1000, history.Count - 1000);
        }
    }
}
