using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateEnshinObject : MonoBehaviour
{
    // 角速度
    [SerializeField] float rotationSpeed = 10f;   // 回転速度

    void Update()
    {
        // Y軸方向に回転する
        this.gameObject.transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }
}
