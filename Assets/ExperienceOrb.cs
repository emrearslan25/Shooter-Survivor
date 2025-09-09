using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceOrb : MonoBehaviour
{
    [Header("Orb Settings")]
    public float experienceValue = 10f;
    public float moveSpeed = 6f;
    public float lifetime = 14f;
    public float pickupRange = 0.5f;

    [Header("Visual")]
    public ParticleSystem pickupEffect;
    public Light orbLight;

    // State
    private Transform target;
    private bool isAttracted = false;
    private float spawnTime;

    void Start()
    {
    spawnTime = Time.time;
    Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (isAttracted && target != null)
        {
            // Move towards player (2D)
            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

            // Check if close enough to pickup
            if (Vector2.Distance(transform.position, target.position) <= pickupRange)
            {
                Pickup(target.GetComponent<PlayerController>());
            }
        }
        else
        {
            // Stay in place until attracted
        }
    }

    public void SetExperienceValue(float value)
    {
        experienceValue = value;

        // Scale orb based on value
    float scale = Mathf.Log10(Mathf.Max(10f, value)) * 0.25f + 0.9f;
        transform.localScale = Vector3.one * scale;

        // Adjust light intensity
        if (orbLight != null)
        {
            orbLight.intensity = Mathf.Lerp(0.8f, 2.2f, Mathf.InverseLerp(0.9f, 1.8f, scale));
        }
    }

    public void AttractToPlayer(Transform playerTransform)
    {
        target = playerTransform;
        isAttracted = true;
        moveSpeed *= 2f; // Faster when attracted
    }

    public void Pickup(PlayerController player)
    {
        if (player == null) return;

        // Give experience to player
        ExperienceSystem expSystem = FindObjectOfType<ExperienceSystem>();
        if (expSystem != null)
        {
            expSystem.GainExperience(experienceValue);
        }

        // Pickup effect
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        // Destroy orb
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            AttractToPlayer(player.transform);
        }
    }
}
