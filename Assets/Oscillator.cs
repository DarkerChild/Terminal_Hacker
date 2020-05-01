using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillator : MonoBehaviour
{
    float movementFactor;
    [SerializeField] Vector3 movementVector = new Vector3(10f, 10f, 10f); //The Maximum offset from starting position
    [SerializeField] [Min(0.001f)]float cycleTime = 2f; //Number of seconds per cycle

    Vector3 startingPosition;

    // Start is called before the first frame update
    void Start()
    {
        startingPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        const float tau = Mathf.PI * 2; //About 6.28
        float cycles = Time.time / cycleTime;
        float rawSinWave = Mathf.Sin(cycles * tau);
        movementFactor = (rawSinWave/2)+0.5f;

        Vector3 offset = movementVector * movementFactor;
        transform.position = startingPosition + (offset);
    }
}
