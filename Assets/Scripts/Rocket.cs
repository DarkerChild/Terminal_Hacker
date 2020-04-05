 //using System;
//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 1f;
    [SerializeField] int fuelUseRate = 1;
    [SerializeField] float fuelLevel = 999f;
    [SerializeField] int lives = 3;
    [SerializeField] int level;
    [SerializeField] float dyingTime = 1f;
    [SerializeField] float transcendingTime = 1f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip death;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem portThrusterParticles;
    [SerializeField] ParticleSystem starbordThrusterParticles;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem deathParticles;

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
        //rocketBody = GetComponent<>;
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
            ParticleSystem fuelPickupParticles;
            MeshRenderer fuelPickupMeshRenderer;
            fuelPickupParticles = Collider.gameObject.GetComponentInChildren<ParticleSystem>();
            fuelPickupParticles.Play();
            fuelPickupMeshRenderer = Collider.gameObject.GetComponentInChildren<MeshRenderer>();
            fuelPickupMeshRenderer.enabled = false;
        }
    }

    private void StartSuccessSequence()
    {
        state = State.Transcending;
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        audioSource.Stop();
        mainEngineParticles.Stop();
        portThrusterParticles.Stop();
        starbordThrusterParticles.Stop();
        audioSource.PlayOneShot(success);
        successParticles.Play();
        Invoke("LoadNextLevel", transcendingTime);
    }

    private void StartDeathSequence()
    {
        state = State.Dying;
        rigidBody.constraints = RigidbodyConstraints.None;
        audioSource.Stop();
        mainEngineParticles.Stop();
        portThrusterParticles.Stop();
        starbordThrusterParticles.Stop();
        audioSource.PlayOneShot(death);
        deathParticles.Play();
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
            if (!portThrusterParticles.isPlaying)
            {
                portThrusterParticles.Play();
            }
            if (!starbordThrusterParticles.isPlaying)
            {
                starbordThrusterParticles.Play();
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
            if (!starbordThrusterParticles.isPlaying)
            {
                starbordThrusterParticles.Play();
                portThrusterParticles.Stop();
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.back * rotationThisFrame);
            if (!portThrusterParticles.isPlaying)
            {
                portThrusterParticles.Play();
                starbordThrusterParticles.Stop();
            }
        }
        else
        {
            portThrusterParticles.Stop();
            starbordThrusterParticles.Stop();
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
            mainEngineParticles.Stop();
        }
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);
        fuelLevel = fuelLevel - (fuelUseRate * Time.deltaTime);
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
        }
        if (!mainEngineParticles.isPlaying)
        {
            mainEngineParticles.Play();
        }
    }
}
