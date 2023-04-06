using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TentacleMovement : MonoBehaviour
{
    public SpriteRenderer[] tentacleSprites;
    public float amplitude = 0.5f;
    public float frequency = 2.0f;
    public float speed = 1.0f;
    public Vector2 direction = Vector2.right;

    private float timeOffset;

    private void Start()
    {
        timeOffset = Random.Range(0.0f, 2.0f * Mathf.PI);
    }

    private void Update()
    {
        MoveTentacle();
    }

    private void MoveTentacle()
    {
        for (int i = 0; i < tentacleSprites.Length; i++)
        {
            float phaseOffset = (float)i / tentacleSprites.Length * Mathf.PI;
            float angle = Time.time * frequency + phaseOffset + timeOffset;
            float x = transform.position.x + i * direction.x;
            float y = transform.position.y + Mathf.Sin(angle) * amplitude;

            tentacleSprites[i].transform.position = new Vector3(x, y, tentacleSprites[i].transform.position.z);
        }

        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
}
