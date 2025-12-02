using System.Collections.Generic;
using UnityEngine;

public class TransformAnimation : MonoBehaviour
{
    public bool loop;
    public bool pingPong;
    public bool playing;
    public int currentTargetID;

    private int direction = 1; // 1 = framåt, -1 = bakåt

    private Vector3 position_Target;
    private Quaternion rotation_Target;
    private Vector3 scale_Target;

    private float transitionSpeed;

    public List<TransformData> transformData = new();

    [System.Serializable]
    public class TransformData
    {
        public Vector3 position;
        public Quaternion rotation;   // använd quaternion för stabilitet
        public Vector3 scale = Vector3.one;
        public float transition_Speed = 1f;
    }

    void Start()
    {
        if (transformData.Count > 0)
        {
            currentTargetID = Mathf.Clamp(currentTargetID, 0, transformData.Count - 1);
            SetTarget(currentTargetID);
        }
    }

    void Update()
    {
        if (!playing || transformData.Count == 0)
            return;

        // Local space istället för world space
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, position_Target, transitionSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rotation_Target, transitionSpeed * 100 * Time.deltaTime);
        transform.localScale = Vector3.MoveTowards(transform.localScale, scale_Target, transitionSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.localPosition, position_Target) < 0.001f &&
            Quaternion.Angle(transform.localRotation, rotation_Target) < 0.5f &&
            Vector3.Distance(transform.localScale, scale_Target) < 0.001f)
        {
            NextTransform();
        }
    }

    private void SetTarget(int id)
    {
        id = Mathf.Clamp(id, 0, transformData.Count - 1);
        position_Target = transformData[id].position;
        rotation_Target = transformData[id].rotation;
        scale_Target = transformData[id].scale;
        transitionSpeed = transformData[id].transition_Speed;
    }

    private void NextTransform()
    {
        currentTargetID += direction;

        if (loop)
        {
            if (pingPong)
            {
                // evig pendling
                if (currentTargetID >= transformData.Count)
                {
                    currentTargetID = transformData.Count - 2;
                    direction = -1;
                }
                else if (currentTargetID < 0)
                {
                    currentTargetID = 1;
                    direction = 1;
                }
            }
            else
            {
                // klassisk loop
                if (currentTargetID >= transformData.Count) currentTargetID = 0;
                else if (currentTargetID < 0) currentTargetID = transformData.Count - 1;
            }
        }
        else // loop == false
        {
            if (pingPong)
            {
                // Stanna när vi når slut eller början, men spara riktning så nästa Play börjar åt motsatt håll.
                if (currentTargetID >= transformData.Count)
                {
                    currentTargetID = transformData.Count - 1; // stanna på sista
                    SetTarget(currentTargetID);
                    direction = -1; // nästa gång vi spelar: gå bakåt
                    playing = false;
                    return;
                }
                else if (currentTargetID < 0)
                {
                    currentTargetID = 0; // stanna på första
                    SetTarget(currentTargetID);
                    direction = 1; // nästa gång vi spelar: gå framåt
                    playing = false;
                    return;
                }
            }
            else
            {
                // Enkel resa 0 -> last -> reset 0 -> stop
                if (currentTargetID >= transformData.Count)
                {
                    currentTargetID = 0;
                    SetTarget(currentTargetID);
                    playing = false;
                    return;
                }
            }
        }

        SetTarget(currentTargetID);
    }

    // play: true startar animation, false stoppar
    public void PlayAnimation(bool play)
    {
        if (!play)
        {
            playing = false;
            return;
        }

        if (transformData.Count == 0)
        {
            playing = false;
            return;
        }

        if (pingPong && !loop)
        {
            if (!playing)
            {
                // animationen är stoppad
                if (currentTargetID == 0) direction = 1;
                else if (currentTargetID == transformData.Count - 1) direction = -1;
            }
            else
            {
                // animationen är mitt i en resa → vänd direkt
                direction *= -1;

                // sätt target så att objektet börjar röra sig mot den keyframe i nya riktningen
                int nextTargetID = currentTargetID + direction;
                nextTargetID = Mathf.Clamp(nextTargetID, 0, transformData.Count - 1);

                currentTargetID = nextTargetID;
                SetTarget(currentTargetID);
            }
        }
        else
        {
            // övriga fall: starta från början
            currentTargetID = 0;
            direction = 1;
            SetTarget(currentTargetID);
        }

        playing = true;
    }

    [ContextMenu("SetStartTransform")]
    private void SetStartTransform()
    {
        if (transformData.Count == 0)
        {
            TransformData newData = new TransformData();
            transformData.Add(newData);
        }

        transformData[0].position = transform.localPosition;
        transformData[0].rotation = transform.localRotation;
        transformData[0].scale = transform.localScale;
        SetTarget(0);
    }

    [ContextMenu("AddKeyFrame")]
    private void AddKeyFrame()
    {
        TransformData newData = new TransformData();
        newData.position = transform.localPosition;
        newData.rotation = transform.localRotation;
        newData.scale = transform.localScale;
        transformData.Add(newData);
    }
}
