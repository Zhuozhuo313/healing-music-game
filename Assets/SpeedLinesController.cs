using UnityEngine;

public class SpeedLinesController : MonoBehaviour
{
    public ParticleSystem speedLines; // 速度线条的粒子系统
    public float maxEmissionRate = 300f; // 发射率的上限
    private ParticleSystem.EmissionModule emissionModule;
    private Vector3 previousPosition;

    void Start()
    {
        if (speedLines == null)
        {
            Debug.LogError("SpeedLines particle system is not assigned.");
            return;
        }
        emissionModule = speedLines.emission;

        // 记录初始位置
        previousPosition = transform.position;
    }

    void Update()
    {
        if (speedLines == null)
        {
            return;
        }

        // 计算当前帧与上一帧之间的z轴速度
        float speedZ = Mathf.Abs(2*(transform.position - previousPosition).z / Time.fixedDeltaTime);

        // 根据z轴速度调整粒子系统的发射率，并设置上限
        float emissionRate = Mathf.Min(speedZ * speedZ * 0.2f, maxEmissionRate);
        emissionModule.rateOverTime = emissionRate;

        // 更新上一帧的位置
        previousPosition = transform.position;
    }
}
