using Unity.VisualScripting;
using UnityEngine;

public class OnCollideBrick : MonoBehaviour
{
    public GameObject smallCubePrefab; // 小方块的预制体

    void Start()
    {

    }
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) // 假设碰撞对象是玩家
        {
            print("Collide!");
            BreakCube(collision.gameObject.transform);
        }
    }

    private void BreakCube(Transform playerTransform)
    {
        Vector3 cubeSize = transform.localScale;
        float smallCubeSize = cubeSize.x / 5f; // 计算小方块的大小

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                Vector3 spawnPosition = transform.position +
                    new Vector3(i * smallCubeSize - cubeSize.x / 2f + smallCubeSize / 2f,
                                j * smallCubeSize - cubeSize.y / 2f + smallCubeSize / 2f,
                                0f);

                GameObject obj = Instantiate(smallCubePrefab, spawnPosition, Quaternion.identity);
                // MeshRenderer mr = obj.GetOrAddComponent<MeshRenderer>();
                // MeshFilter mf = obj.GetComponent<MeshFilter>();
                // mr = GetComponent<MeshRenderer>();
                // mf = GetComponent<MeshFilter>();
                // obj.transform.localScale = cubeSize * smallCubeSize;// Not accepted
            }
        }

        Destroy(gameObject); // 销毁原始的Cube
    }
}
