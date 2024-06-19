using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public float speed = 10.0f; // 沿Z轴的移动速度
    public float rotationSpeed = 150.0f; // 绕Z轴的旋转速度
    public float gravityForce = 500.0f; // 重力大小
    public float radius = 10.0f; // 圆柱半径
    public float gravityX = 0.0f; // 重力x分量
    public float gravityY = 0.0f; // 重力y分量
    public bool enableGravity = false; // 是否启用重力

    private Rigidbody rb;
    private Transform camTransform;
    private float camRadius = 5.0f; // 摄像机圆周半径

    private float camDistance = 15.0f; // 相机距角色的Z轴距离

    private float originalSpeed;
    private float originalCamDistance;
    private bool isSpeedSlowing = false;
    public float slowDuration = 2.0f; // 减速保持时间
    public float slowSpeedMultiplier = 0.5f; // 减速倍数
    public float slowCamMultiplier = 0.7f; // 相机拉近倍数
    private bool isSpeedBoosting = false;
    public float boostDuration = 1.0f; // 提速保持时间
    public float boostSpeedMultiplier = 2.0f; // 提速倍数
    public float boostCamMultiplier = 1.3f; // 相机拉远倍数
    public bool isRotating = false; // 是否正在自转
    private bool isZoomingIn = false; // 是否正在拉近
    public float zoomInDuration = 10.7f; // 拉近持续时间
    private float targetCamRadius = 8.0f; // 目标摄像机圆周半径
    private AudioSource audioSource;
    public GameObject model1;
    public GameObject model2;
    public GameObject model3;

    // 区间和角度参数列表
    [System.Serializable]
    public class BoostCondition
    {
        public float zMin;
        public float zMax;
        public float angleMin;
        public float angleMax;
    }

    public List<BoostCondition> boostConditions = new List<BoostCondition>();

    [System.Serializable]
    public class ThirdCondition
    {
        public float zMin;
        public float zMax;
    }
    public List<ThirdCondition> thirdConditions = new List<ThirdCondition>();

    void Start()
    {
        if (!TryGetComponent(out rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false; // 禁用默认重力
        camTransform = Camera.main.transform;

        // 初始位置设置在圆柱内壁上
        Vector3 startPosition = new Vector3(0, -radius, -5.0f);
        transform.position = startPosition;

        // 设置摄像机初始位置
        Vector3 camStartPosition = new Vector3(0, -camRadius, -5.0f-camDistance);
        camTransform.position = camStartPosition;

        originalSpeed = speed;
        originalCamDistance = camDistance;

        // 获取已有的AudioSource组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component missing from this game object. Please add an AudioSource component in the Inspector.");
        }
        else
        {
            audioSource.loop = true;
            audioSource.Play();
        }

        // 禁用角色本身的模型
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }

        // 启用默认模型
        model1.SetActive(true);
        model2.SetActive(false);
        model3.SetActive(false);
    }

    void FixedUpdate()
    {
        MoveForward();
        if(!enableGravity)
        {
            RotatePlayer();
        }
        HandleGravity();
        CheckBoundary();

        if (Input.GetKey(KeyCode.S) && !isSpeedSlowing)
        {
            StartCoroutine(SpeedSlow());
        }
        if (Input.GetKey(KeyCode.W) && !isSpeedBoosting)
        {
            StartCoroutine(SpeedBoost());
        }

        if (Input.GetKey(KeyCode.E) && !isZoomingIn)
        {
            StartCoroutine(ZoomIn());
        }
        // 检查特定区间和角度的加速条件
        CheckBoostConditions();
        CheckThirdConditions();

        // 更新模型显示
        UpdateModel();
    }

    void MoveForward()
    {
        Vector3 forwardMovement = Vector3.forward * speed * Time.fixedDeltaTime;
        transform.Translate(forwardMovement, Space.World);
        camTransform.Translate(forwardMovement, Space.World);
    }

    void RotatePlayer()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            RotateAroundZAxis(-rotationSpeed * Time.fixedDeltaTime);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            RotateAroundZAxis(rotationSpeed * Time.fixedDeltaTime);
        }
    }

    void RotateAroundZAxis(float angle)
    {
        // 获取角色当前位置在XZ平面的投影
        Vector3 currentPosition = transform.position;
        float x = currentPosition.x;
        float y = currentPosition.y;

        // 计算当前与Z轴的角度
        float currentAngle = Mathf.Atan2(y, x);

        // 更新角度
        currentAngle += angle * Mathf.Deg2Rad;

        // 计算新的位置
        float newX = radius * Mathf.Cos(currentAngle);
        float newY = radius * Mathf.Sin(currentAngle);
        transform.position = new Vector3(newX, newY, currentPosition.z);

        // 更新摄像机位置
        float camNewX = camRadius * Mathf.Cos(currentAngle);
        float camNewY = camRadius * Mathf.Sin(currentAngle);
        camTransform.position = new Vector3(camNewX, camNewY, currentPosition.z-camDistance);

        // 更新角色的自转
        transform.Rotate(Vector3.forward, angle);

        // 更新摄像机的旋转
        camTransform.Rotate(Vector3.forward, angle);
    }
    void HandleGravity()
    {
        if (Input.GetKey(KeyCode.UpArrow) && !enableGravity)
        {
            // 获取角色当前位置在XY平面的投影
            Vector3 currentPosition = transform.position;
            gravityX = currentPosition.x;
            gravityY = currentPosition.y;
            enableGravity = true;
            StartCoroutine(RotateDuringGravity());
        }
        if(enableGravity)
        {
            // 计算重力方向
            Vector3 gravityDirection = new Vector3(gravityX, gravityY, 0).normalized;
            // 应用重力
            rb.AddForce(gravityDirection * -gravityForce);
        }
    }
    void CheckBoundary()
    {
        Vector3 currentPosition = transform.position;
        float distanceFromCenter = new Vector3(currentPosition.x, currentPosition.y, 0).magnitude;

        if (distanceFromCenter > radius + 0.01)
        {
            // 规范位置到半径为radius的圆上
            Vector3 normalizedPosition = new Vector3(currentPosition.x, currentPosition.y, 0).normalized * radius;
            transform.position = new Vector3(normalizedPosition.x, normalizedPosition.y, currentPosition.z);

            // 清零非z轴速度
            Vector3 velocity = rb.velocity;
            velocity.x = 0;
            velocity.y = 0;
            rb.velocity = velocity;
            enableGravity = false;
        }
        
        // 更新摄像机位置
        float camNewX = camRadius / radius * currentPosition.x;
        float camNewY = camRadius / radius * currentPosition.y;
        camTransform.position = new Vector3(camNewX, camNewY, currentPosition.z-camDistance);
    }

    IEnumerator RotateDuringGravity()
    {
        if (!isRotating)
        {
            isRotating = true;
            float rotationAmount = 180.0f; // 自转角度
            float rotationSpeed = rotationAmount / Mathf.Sqrt(2 * radius / (gravityForce / rb.mass)); // 自转速度

            while (rotationAmount > 0)
            {
                float angle = Mathf.Min(rotationSpeed * Time.fixedDeltaTime, rotationAmount);
                transform.Rotate(Vector3.forward, angle);
                rotationAmount -= angle;
                yield return null;
            }

            isRotating = false;
        }
    }
    IEnumerator SpeedBoost()
    {
        isSpeedBoosting = true;

        float targetSpeed = originalSpeed * boostSpeedMultiplier;
        float targetCamDistance = originalCamDistance * boostCamMultiplier;
        float currentSpeed = speed;
        float currentCamDistance = camDistance;
        float endSpeed = targetSpeed * 1.05f;
        float endCamDistance = originalCamDistance + (targetCamDistance - originalCamDistance) * (endSpeed - originalSpeed) / (targetSpeed - originalSpeed);
        float endSpeedDown = originalSpeed * 0.9f;
        float endCamDistanceDown = targetCamDistance + (originalCamDistance - targetCamDistance) * (endSpeedDown - targetSpeed) / (originalSpeed - targetSpeed);

        // 提速阶段
        while (currentSpeed < targetSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, endSpeed, Time.fixedDeltaTime*3.0f);
            currentCamDistance = Mathf.Lerp(currentCamDistance, endCamDistance, Time.fixedDeltaTime*3.0f);
            speed = currentSpeed;
            camDistance = currentCamDistance;
            audioSource.pitch = speed / originalSpeed; // 同步音乐速度

            yield return null;
        }

        speed = targetSpeed;
        camDistance = targetCamDistance;
        audioSource.pitch = speed / originalSpeed; // 同步音乐速度

        // 保持提速
        yield return new WaitForSeconds(boostDuration);

        // 恢复阶段
        while (currentSpeed > originalSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, endSpeedDown, Time.fixedDeltaTime);
            currentCamDistance = Mathf.Lerp(currentCamDistance, endCamDistanceDown, Time.fixedDeltaTime);
            speed = currentSpeed;
            camDistance = currentCamDistance;
            audioSource.pitch = speed / originalSpeed; // 同步音乐速度

            yield return null;
        }

        speed = originalSpeed;
        camDistance = originalCamDistance;
        audioSource.pitch = 1.0f; // 恢复音乐速度
        isSpeedBoosting = false;
    }
    public void StartSlowDown()
    {
        if (!isSpeedSlowing)
        {
            StartCoroutine(SpeedSlow());
        }
        // 设置角色的rotation的x与y值为0
        Vector3 currentRotation = transform.rotation.eulerAngles;
        currentRotation.x = 0;
        currentRotation.y = 0;
        transform.rotation = Quaternion.Euler(currentRotation);
    }
    IEnumerator SpeedSlow()
    {
        isSpeedSlowing = true;
        float targetSpeed = originalSpeed * slowSpeedMultiplier;
        float targetCamDistance = originalCamDistance * slowCamMultiplier;
        float currentSpeed = speed;
        float currentCamDistance = camDistance;
        float endSpeed = targetSpeed * 0.9f;
        float endCamDistance = originalCamDistance + (targetCamDistance - originalCamDistance) * (endSpeed - originalSpeed) / (targetSpeed - originalSpeed);
        float endSpeedUp = originalSpeed * 1.05f;
        float endCamDistanceUp = targetCamDistance + (originalCamDistance - targetCamDistance) * (endSpeedUp - targetSpeed) / (originalSpeed - targetSpeed);

        // 减速阶段
        while (currentSpeed > targetSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, endSpeed, Time.fixedDeltaTime);
            currentCamDistance = Mathf.Lerp(currentCamDistance, endCamDistance, Time.fixedDeltaTime);
            speed = currentSpeed;
            camDistance = currentCamDistance;
            audioSource.pitch = speed / originalSpeed; // 同步音乐速度

            yield return null;
        }

        speed = targetSpeed;
        camDistance = targetCamDistance;
        audioSource.pitch = speed / originalSpeed; // 同步音乐速度

        // 保持减速
        yield return new WaitForSeconds(slowDuration);

        // 恢复阶段
        while (currentSpeed < originalSpeed)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, endSpeedUp, Time.fixedDeltaTime/3);
            currentCamDistance = Mathf.Lerp(currentCamDistance, endCamDistanceUp, Time.fixedDeltaTime/3);
            speed = currentSpeed;
            camDistance = currentCamDistance;
            audioSource.pitch = speed / originalSpeed; // 同步音乐速度

            yield return null;
        }

        speed = originalSpeed;
        camDistance = originalCamDistance;
        audioSource.pitch = 1.0f; // 恢复音乐速度
        isSpeedSlowing = false;
    }
    IEnumerator ZoomIn()
    {
        isZoomingIn = true;
        float targetCamDistance = 0.0f;
        float currentCamDistance = camDistance;
        float startCamDistance = camDistance;
        float startCamRadius = camRadius;
        float elapsedTime = 0.0f;

        while (elapsedTime < zoomInDuration)
        {
            elapsedTime += Time.fixedDeltaTime;
            camDistance = Mathf.Lerp(startCamDistance, targetCamDistance, elapsedTime / zoomInDuration);
            camRadius = Mathf.Lerp(startCamRadius, targetCamRadius, elapsedTime / zoomInDuration);
            camTransform.position = new Vector3(camRadius * Mathf.Cos(Mathf.Atan2(transform.position.y, transform.position.x)),
                                                camRadius * Mathf.Sin(Mathf.Atan2(transform.position.y, transform.position.x)),
                                                transform.position.z - camDistance);
            yield return null;
        }

        camDistance = targetCamDistance;
        camRadius = targetCamRadius;
        camTransform.position = new Vector3(camRadius * Mathf.Cos(Mathf.Atan2(transform.position.y, transform.position.x)),
                                            camRadius * Mathf.Sin(Mathf.Atan2(transform.position.y, transform.position.x)),
                                            transform.position.z - camDistance);
        // 隐藏角色
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        isZoomingIn = false;
    }

    void CheckBoostConditions()
    {
        float currentZ = transform.position.z;
        float currentAngleSecond = Mathf.Atan2(transform.position.y, transform.position.x) * Mathf.Rad2Deg;
        foreach (BoostCondition condition in boostConditions)
        {
            if (currentZ >= condition.zMin && currentZ <= condition.zMax && currentAngleSecond >= condition.angleMin && currentAngleSecond <= condition.angleMax)
            {
                if (!isSpeedBoosting)
                {
                    StartCoroutine(SpeedBoost());
                    break;
                }
            }
        }
    }
    void CheckThirdConditions()
    {
        float currentZ = transform.position.z;
        foreach (ThirdCondition condition in thirdConditions)
        {
            if (currentZ >= condition.zMin && currentZ <= condition.zMax)
            {
                speed = 50.0f;
                rotationSpeed = 75.0f;
                StartCoroutine(ZoomIn());
                StartCoroutine(AutoRotateLeftRight());
            }
        }
    }
    IEnumerator AutoRotateLeftRight()
    {
        while (true)
        {
            // 向左转2秒
            float rotationTime = 2.0f;
            float pauseTime = 1.0f;
            float elapsedTime = 0.0f;
            while (elapsedTime < rotationTime)
            {
                RotateAroundZAxis(-rotationSpeed * Time.fixedDeltaTime);
                elapsedTime += Time.fixedDeltaTime;
                yield return null;
            }
            // 暂停2秒
            yield return new WaitForSeconds(pauseTime);
            // 向右转2秒
            elapsedTime = 0.0f;
            while (elapsedTime < rotationTime)
            {
                RotateAroundZAxis(rotationSpeed * Time.fixedDeltaTime);
                elapsedTime += Time.fixedDeltaTime;
                yield return null;
            }
            // 再次暂停2秒
            yield return new WaitForSeconds(pauseTime);
        }
    }

    void UpdateModel()
    {
        float currentZ = transform.position.z;

        if (currentZ > 520)
        {
            model1.SetActive(false);
            model2.SetActive(false);
            model3.SetActive(true);
        }
        else if (currentZ > 265)
        {
            model1.SetActive(false);
            model2.SetActive(true);
            model3.SetActive(false);
        }
        else
        {
            model1.SetActive(true);
            model2.SetActive(false);
            model3.SetActive(false);
        }
    }
}
