using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBodyCollision : MonoBehaviour
{
    private Rigidbody rigidbody;
    private AudioSource hitAudio;
    public AudioClip[] hitSound;
    public float volume;
    [Range(1,25)]
    public int loudnessLevel;
    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        hitAudio = GetComponent<AudioSource>();
    }
    public void OnCollisionEnter(Collision collision)
    {
        float velocity = collision.relativeVelocity.magnitude;
        volume = (velocity - (2.5f)) / (6 - (2.5f));
        volume = Mathf.Clamp01(volume);
        if (collision.relativeVelocity.magnitude >2)
        {
            hitAudio.pitch = Random.Range(0.9f, 1.2f);
            hitAudio.volume = volume;
            hitAudio.PlayOneShot(hitSound[Random.Range(0,hitSound.Length)]);
            if(volume >0.1f)
                SendSoundToEnemy();
        }
    }
    private void SendSoundToEnemy()
    {
        int soundZone = EnemyController.instance.hearingDistance / EnemyController.instance.hearingDistance; // vi har 25st hörsel zoner
        float currentZone = soundZone * loudnessLevel;
        Debug.Log("ljudet hörs " + currentZone + "m");

        EnemyController.instance.SoundImpact(loudnessLevel, transform);
        PlayerAudioDetection.instance.SoundImpact(loudnessLevel);
        SendSoundToRats(currentZone);
    }
    private void SendSoundToRats(float currentZone)
    {
        float detectionRadius = currentZone;
        LayerMask ratLayer = LayerMask.GetMask("Rat");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius, ratLayer);

        foreach (var hit in hitColliders)
        {
            RatAI rat = hit.GetComponent<RatAI>();
            if (rat != null)
            {
                // Här kan du t.ex. säga åt råttan att den blir rädd
                rat.ScaredRat();
            }
        }
    }
}
