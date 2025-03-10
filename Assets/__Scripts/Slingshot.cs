using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slingshot : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public float velocityMult = 10f;
    public GameObject projLinePrefab;
    public AudioClip launchSound;
    public AudioClip whirringSound;
    private AudioSource audioSource;

    [Header("Rubber Band Settings")]
    [SerializeField] private LineRenderer rubber;
    [SerializeField] private Transform firstPoint;  // Left arm of slingshot
    [SerializeField] private Transform secondPoint; // Right arm of slingshot

    [Header("UI")]
    public Slider powerMeter; 
    private float power = 0f;

    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;

    void Awake()
    {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive(false);
        launchPos = launchPointTrans.position;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        rubber.positionCount = 3;  // Ensure we have 3 points for the rubber band
        ResetRubberBand();
    }

    void Update()
    {
        if (aimingMode)
        {
            Vector3 mousePos2D = Input.mousePosition;
            mousePos2D.z = -Camera.main.transform.position.z;
            Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

            Vector3 mouseDelta = mousePos3D - launchPos;
            float maxMagnitude = this.GetComponent<SphereCollider>().radius;
            if (mouseDelta.magnitude > maxMagnitude)
            {
                mouseDelta.Normalize();
                mouseDelta *= maxMagnitude;
            }
            
            power = mouseDelta.magnitude / maxMagnitude;
            if (powerMeter != null)
            {
                powerMeter.value = power;  
            }

            Vector3 projPos = launchPos + mouseDelta;
            projectile.transform.position = projPos;

            // Update rubber band while dragging
            UpdateRubberBand(projPos);
        }

        if (Input.GetMouseButtonUp(0) && aimingMode)
        {
            aimingMode = false;
            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -(projectile.transform.position - launchPos) * velocityMult;

            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);
            FollowCam.POI = projectile;
            Instantiate<GameObject>(projLinePrefab, projectile.transform);

            PlayLaunchSound();
            AudioSource projAudio = projectile.GetComponent<AudioSource>();
            if (projAudio != null)
            {
                projAudio.Play();
            }

            projectile = null;
            MissionDemolition.SHOT_FIRED();
            if (powerMeter != null)
            {
                powerMeter.value = 0f;
            }

            // Reset rubber band after launch
            StartCoroutine(ResetRubberBandAfterDelay());
        }
    }

    void OnMouseEnter()
    {
        launchPoint.SetActive(true);
    }

    void OnMouseExit()
    {
        launchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        aimingMode = true;
        projectile = Instantiate(projectilePrefab);
        projectile.transform.position = launchPos;
        projectile.GetComponent<Rigidbody>().isKinematic = true;

        AudioSource projAudio = projectile.AddComponent<AudioSource>();
        projAudio.clip = whirringSound;
        projAudio.loop = true;
        projAudio.playOnAwake = false;
    }

    // 🔹 Keeps the rubber band attached to slingshot arms
    void UpdateRubberBand(Vector3 middlePoint)
    {
        rubber.SetPosition(0, firstPoint.position);  // Left arm
        rubber.SetPosition(1, middlePoint);          // Stretching part
        rubber.SetPosition(2, secondPoint.position); // Right arm
    }

    // 🔹 Resets the rubber band back to default position
    void ResetRubberBand()
    {
        Vector3 middle = (firstPoint.position + secondPoint.position) / 2;
        rubber.SetPosition(0, firstPoint.position);
        rubber.SetPosition(1, middle);
        rubber.SetPosition(2, secondPoint.position);
    }

    // 🔹 Delay before resetting rubber band after launch
    IEnumerator ResetRubberBandAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        ResetRubberBand();
    }

    public void PlayLaunchSound()
    {
        if (launchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(launchSound);
        }
    }
}


