using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

public class BrickPieceController : MonoBehaviour
{
    public float shrinkSpeed = 0.05f; // 缩小速度
    public float speed = 3.0f;

    private Vector3 shrinkDirection; // 缩小方向
    private Vector3 initialScale;
    private float currentScale;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        initialScale = transform.localScale;
        shrinkDirection = (transform.position - player.transform.position).normalized;
        currentScale = initialScale.x;
    }

    private void Update()
    {
        // 缩小方块
        currentScale -= shrinkSpeed * Time.deltaTime;
        currentScale = Mathf.Max(currentScale, 0f); // 确保不会变成负数

        // 设置新的缩放
        transform.localScale = initialScale * currentScale;

        // 移动方块
        transform.Translate(shrinkDirection * speed * Time.deltaTime);
        transform.Translate(transform.forward * 5f * Time.deltaTime);

        // 如果方块已经很小，销毁它
        if (currentScale <= 0.01f)
        {
            Destroy(gameObject);
        }
    }
}
