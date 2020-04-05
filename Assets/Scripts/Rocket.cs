using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    Rigidbody rigidBody;
    AudioSource rocketSound;
    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float thrustScale = 1f;
    [SerializeField] int fuelLevel = 999;
    [SerializeField] int lives = 3;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rocketSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        Rotate();
        Thrust();
        DetectCollision();
    }

    void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Respawn":
                // DO NOTHING
                print("Fine");
                break;
            case "Finish":
                // DO NOTHING
                print("Fine");
                break;
            case "Fuel":
                // Collect fuel
                fuelLevel += 100;
                print("Fuel Collected");
                break;
            default:
                print("Dead");
                //TODO Kill player
                break;
        }
    }



    private void Rotate()
    {
        rigidBody.freezeRotation = true; //Take manual control of rotation
        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.back * rotationThisFrame);
        }

        rigidBody.freezeRotation = false; //Resume physics control of rotation
    }

    private void Thrust()
    {
        if (Input.GetKey(KeyCode.Space)) // Can thrust while rotating
        {
            rigidBody.AddRelativeForce(Vector3.up * thrustScale);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rocketSound.Play(); //Turn sounds on
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            rocketSound.Stop(); //Turn Sound Off
        }
    }

    private void DetectCollision()
    {
        
    }
}
