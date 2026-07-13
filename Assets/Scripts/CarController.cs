using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentbreakForce;
    private bool isBreaking;

    public Transform[] spawnPoints;
    private Rigidbody rb;

    public bool IsGhostMode { get; private set; }

    [SerializeField] private float ghostTime = 3f;
    [SerializeField] private GameObject[] enemyCars;

    // Settings
    [SerializeField] private float motorForce, breakForce, maxSteerAngle;

    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Wheel Models
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    [SerializeField] private AudioSource engineSound;
    [SerializeField] private AudioSource brakeSound;

    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 2f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        engineSound.loop = true;
    }

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();

        UpdateEngineSound();
        UpdateBrakeSound();
    }

    private void Update()
    {
        // R 키를 누르면 리스폰
        if (Input.GetKeyDown(KeyCode.R))
        {
            RespawnAtClosest();
        }
    }

    private void GetInput()
    {
        // Steering
        horizontalInput = Input.GetAxis("Horizontal");

        // Acceleration
        verticalInput = Input.GetAxis("Vertical");

        // Brake
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;


        // 엑셀을 떼면 약한 감속
        if (Mathf.Abs(verticalInput) < 0.1f)
        {
            rb.linearVelocity *= 0.993f;
        }


        currentbreakForce = isBreaking ? breakForce : 0f;

        ApplyBreaking();
    }

    private void ApplyBreaking()
    {
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;

        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;

        wheelCollider.GetWorldPose(out pos, out rot);

        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    private void SetIgnoreEnemyCollision(bool ignore)
    {
        Collider[] myColliders = GetComponentsInChildren<Collider>();

        foreach (GameObject enemy in enemyCars)
        {
            if (enemy == null)
                continue;

            Collider[] enemyColliders = enemy.GetComponentsInChildren<Collider>();

            foreach (Collider myCol in myColliders)
            {
                foreach (Collider enemyCol in enemyColliders)
                {
                    Physics.IgnoreCollision(myCol, enemyCol, ignore);
                }
            }
        }
    }

    private void RespawnAtClosest()
    {
        // 스폰 포인트가 없으면 종료
        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        // 가장 가까운 스폰 포인트 찾기
        Transform closestPoint = spawnPoints[0];
        float closestDistance = Vector3.Distance(transform.position, closestPoint.position);

        foreach (Transform point in spawnPoints)
        {
            float distance = Vector3.Distance(transform.position, point.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = point;
            }
        }

        // 차량 속도 초기화
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 차량 이동
        transform.position = closestPoint.position;
        transform.rotation = closestPoint.rotation;

        // 고스트 모드 시작
        StartCoroutine(GhostMode());
    }

    private IEnumerator GhostMode()
    {
        IsGhostMode = true;

        SetIgnoreEnemyCollision(true);

        yield return new WaitForSeconds(ghostTime);

        SetIgnoreEnemyCollision(false);

        IsGhostMode = false;
    }

    public float GetSpeed()
    {
        // m/s -> km/h 변환
        float speed = rb.linearVelocity.magnitude * 3.6f;

        return speed;
    }

    private void UpdateEngineSound()
    {
        float speed = rb.linearVelocity.magnitude;

        // 차량이 움직이면 엔진음 재생
        if (speed > 0.5f)
        {
            if (!engineSound.isPlaying)
                engineSound.Play();

            engineSound.pitch = Mathf.Lerp(
                minPitch,
                maxPitch,
                speed / 30f
            );
        }
        else
        {
            if (engineSound.isPlaying)
                engineSound.Stop();
        }
    }

    private void UpdateBrakeSound()
    {
        float speed = rb.linearVelocity.magnitude;

        // 브레이크 입력 && 차량이 움직이고 있을 때만
        if (isBreaking && speed > 1f)
        {
            if (!brakeSound.isPlaying)
                brakeSound.Play();
        }
        else
        {
            if (brakeSound.isPlaying)
                brakeSound.Stop();
        }
    }

    public void StopCarSound()
    {
        if (engineSound != null && engineSound.isPlaying)
        {
            engineSound.Stop();
        }

        if (brakeSound != null && brakeSound.isPlaying)
        {
            brakeSound.Stop();
        }
    }
}