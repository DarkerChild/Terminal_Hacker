 //using System;
//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float thrustScale = 1f;
    [SerializeField] int fuelUseRate = 1;
    [SerializeField] float fuelLevel = 999f;
    [SerializeField] int lives = 3;
    [SerializeField] int level;
    [SerializeField] float dyingTime = 1f;
    [SerializeField] float transcendingTime = 1f;
    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip death;

    Rigidbody rigidBody;
    AudioSource audioSource;
    Scene scene;


    enum State { Alive, Dying, Transcending };
    State state = State.Alive;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Alive) {
            RespondToRotateInput();
            RespondToThrustInput();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive) {return;} //Ignore collisions if not alive.

        switch (collision.gameObject.tag)
        {
            case "Respawn":
                break;
            case "Finish":
                StartSuccessSequence();
                break;
            default:
                StartDeathSequence();
                break;
        }
    }

    void OnTriggerEnter(Collider Collider)
    {
        if (Collider.gameObject.tag == "Fuel")
        {
            fuelLevel += 100;
            Collider.gameObject.SetActive(false);
        }
    }

    private void StartSuccessSequence()
    {
        state = State.Transcending;
        audioSource.Stop();
        audioSource.PlayOneShot(success);
        Invoke("LoadNextLevel", transcendingTime);
    }

    private void StartDeathSequence()
    {
        state = State.Dying;
        rigidBody.constraints = RigidbodyConstraints.None;
        audioSource.Stop();
        audioSource.PlayOneShot(death);
        Invoke("LoadFirstLevel", dyingTime);
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene(1);
        state = State.Alive;
    }

    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
        state = State.Alive;
    }



    private void RespondToRotateInput()
    {
        rigidBody.freezeRotation = true; //Take manual control of rotation
        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) {

        }
        else if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.back * rotationThisFrame);
        }
        rigidBody.freezeRotation = false; //Resume physics control of rotation
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            audioSource.Stop();
        }
    }

    private void ApplyThrust()
    {
        if (Input.GetKey(KeyCode.Space)) // Can thrust while rotating
        {
            rigidBody.AddRelativeForce(Vector3.up * thrustScale);
            fuelLevel = fuelLevel - (Time.deltaTime * fuelUseRate);
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(mainEngine);
            }
        }
    }
}
