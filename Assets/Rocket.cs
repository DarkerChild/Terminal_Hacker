 //using System;
//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Rocket : MonoBehaviour
{
    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float mainThrust = 1f;
    [SerializeField] float mainEngineFuelUseRate = 1f;
    [SerializeField] float thrusterFuelUseRate = 1f;
    [SerializeField] float startingFuelLevel = 999f;
    [SerializeField] float currentFuelLevel;
    [SerializeField] int lives = 3;
    [SerializeField] int level;
    [SerializeField] float dyingTime = 1f;
    [SerializeField] float transcendingTime = 1f;
    

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip portThrusterEngine;
    [SerializeField] AudioClip starbordThrusterEngine;
    [SerializeField] AudioClip success;
    [SerializeField] AudioClip death;

    [SerializeField] ParticleSystem mainEngineParticles;
    [SerializeField] ParticleSystem portThrusterParticles;
    [SerializeField] ParticleSystem starbordThrusterParticles;
    [SerializeField] ParticleSystem successParticles;
    [SerializeField] ParticleSystem deathParticles;

    Vector3 startingPosition;
    Rigidbody rigidBody;
    AudioSource[] audioSources;
    AudioSource audiosourceMainEngine;
    AudioSource audiosourcePortThruster;
    AudioSource audiosourceStarbordThruster;

    //enum State { Alive, Dying, Transcending };
    //State state = State.Alive;
    bool isTransitioning = false;

    bool CollisionsDisabled = false;


    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        audioSources = GetComponents<AudioSource>();
        audiosourceMainEngine = audioSources[0];
        audiosourcePortThruster = audioSources[1];
        audiosourceStarbordThruster = audioSources[2];
        startingPosition = transform.position;
        currentFuelLevel = startingFuelLevel;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isTransitioning)
        {
            if (currentFuelLevel > 0)
            {
                RespondToRotateInput();
                RespondToThrustInput();
            }
            else
            {
                StopShipSoundsAndParticles();
            }
            if (Debug.isDebugBuild)
            {
                RunDebugFunctions();
            }
        }
    }

    private void RunDebugFunctions()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CollisionsDisabled = !CollisionsDisabled;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextLevel();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isTransitioning || (CollisionsDisabled)) {return;} //Ignore collisions if not alive.

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
        PickupFuel(Collider);
    }

    private void PickupFuel(Collider Collider)
    {
        if (Collider.gameObject.tag == "Fuel")
        {
            currentFuelLevel += 100;
            ParticleSystem fuelPickupParticles;
            fuelPickupParticles = Collider.gameObject.GetComponentInChildren<ParticleSystem>();
            fuelPickupParticles.Play();

            MeshRenderer fuelPickupMeshRenderer;
            fuelPickupMeshRenderer = Collider.gameObject.GetComponentInChildren<MeshRenderer>();
            fuelPickupMeshRenderer.enabled = false;

            Collider fuelPickupCollider;
            fuelPickupCollider = Collider.gameObject.GetComponent<Collider>();
            fuelPickupCollider.enabled = false;
        }
    }

    private void StartSuccessSequence()
    {
        isTransitioning = true;
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        StopShipSoundsAndParticles();
        audiosourceMainEngine.PlayOneShot(success);
        successParticles.Play();
        Invoke("LoadNextLevel", transcendingTime);
    }

    private void StartDeathSequence()
    {
        isTransitioning = true;
        rigidBody.constraints = RigidbodyConstraints.None;
        StopShipSoundsAndParticles();
        audiosourceMainEngine.PlayOneShot(death);
        deathParticles.Play();
        lives--;
        if (lives < 1)
        {
            Invoke("LoadFirstLevel", dyingTime);
        }
        else
        {
            Invoke("resetPosition", dyingTime);
        }
    }

    private void resetPosition()
    {
        //transform.rotation.

        rigidBody.freezeRotation = true;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        transform.position = startingPosition;
        currentFuelLevel = startingFuelLevel;
        rigidBody.freezeRotation = false;
        isTransitioning = false;
    }

    private void StopShipSoundsAndParticles()
    {
        audiosourceMainEngine.Stop();
        audiosourcePortThruster.Stop();
        audiosourceStarbordThruster.Stop();
        mainEngineParticles.Stop();
        portThrusterParticles.Stop();
        starbordThrusterParticles.Stop();
    }

    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
        isTransitioning = false;
    }

    private void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex;
        if (currentSceneIndex == SceneManager.sceneCountInBuildSettings-1)
        {
            nextSceneIndex = 0;
        }
        else
        {
            nextSceneIndex = currentSceneIndex+1 ;
        }
        SceneManager.LoadScene(nextSceneIndex);
        isTransitioning = false;
    }


    private void RespondToRotateInput()
    {
        rigidBody.angularVelocity = Vector3.zero; //Remove angular velocity.
        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
        {
            RotateBothDirections();
        }
        else if (Input.GetKey(KeyCode.A))
        {
            RotateLeft(rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            RotateRight(rotationThisFrame);
        }
        else
        {
            portThrusterParticles.Stop();
            starbordThrusterParticles.Stop();
            audiosourcePortThruster.Stop();
            audiosourceStarbordThruster.Stop();
        }

    }

    private void RotateBothDirections()
    {
        currentFuelLevel = currentFuelLevel - (2 * thrusterFuelUseRate * Time.deltaTime);
        TurnOnPortThrusterParticles();
        TurnOnStarbordThrusterParticles();
        TurnOnPortThrusterAudio();
        TurnOnStarbordThrusterAudio();
    }

    private void RotateRight(float rotationThisFrame)
    {
        transform.Rotate(Vector3.back * rotationThisFrame);
        currentFuelLevel = currentFuelLevel - (thrusterFuelUseRate * Time.deltaTime);
        TurnOnPortThrusterParticles();
        TurnOnPortThrusterAudio();
        starbordThrusterParticles.Stop();
        audiosourceStarbordThruster.Stop();
    }

    private void RotateLeft(float rotationThisFrame)
    {
        transform.Rotate(Vector3.forward * rotationThisFrame);
        currentFuelLevel = currentFuelLevel - (thrusterFuelUseRate * Time.deltaTime);
        TurnOnStarbordThrusterParticles();
        TurnOnStarbordThrusterAudio();
        portThrusterParticles.Stop();
        audiosourcePortThruster.Stop();
    }

    private void TurnOnStarbordThrusterAudio()
    {
        if (!audiosourceStarbordThruster.isPlaying) 
        { 
            audiosourceStarbordThruster.Play(); 
        }
    }

    private void TurnOnPortThrusterAudio()
    {
        if (!audiosourcePortThruster.isPlaying) 
        { 
            audiosourcePortThruster.Play(); 
        }
    }

    private void TurnOnStarbordThrusterParticles()
    {
        if (!starbordThrusterParticles.isPlaying) 
        { 
            starbordThrusterParticles.Play(); 
        }
    }

    private void TurnOnPortThrusterParticles()
    {
        if (!portThrusterParticles.isPlaying) 
        {
            portThrusterParticles.Play(); 
        }
    }

    private void RespondToThrustInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrust();
        }
        else
        {
            audiosourceMainEngine.Stop();
            mainEngineParticles.Stop();
        }
    }

    private void ApplyThrust()
    {
        rigidBody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);
        currentFuelLevel = currentFuelLevel - (mainEngineFuelUseRate * Time.deltaTime);
        if (!audiosourceMainEngine.isPlaying)
        {
            audiosourceMainEngine.Play();
        }
        if (!mainEngineParticles.isPlaying)
        {
            mainEngineParticles.Play();
        }
    }
}
