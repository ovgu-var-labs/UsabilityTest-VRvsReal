using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovementAuto : MonoBehaviour
{
    // This script was made to have the secondary camera move automatically for thew purpose of recording a video
    public float moveSpeed;
    public float rotationSpeed;
    private float rotSpeed;
    private float mSpeed;
    private bool move = false;
    private bool slowDown = false;

    void Start()
    {
        rotSpeed = rotationSpeed;
        mSpeed = moveSpeed;
        StartCoroutine(StartWait());
        StartCoroutine(StartSlow());
    }

    void Update()
    {
        if (move)
        {
            // Movement
            transform.Translate(new Vector3(-1.12f, 0, 1) * mSpeed * Time.deltaTime);
            // Rotation
            transform.Rotate(new Vector3(0, 1, 0) * rotSpeed * Time.deltaTime);
            if (rotSpeed < 6 && !slowDown) { rotSpeed = rotSpeed * 1.01f; }
            if (slowDown) { mSpeed = mSpeed / 1.03f; rotSpeed = rotSpeed / 1.03f; }
        }
    }

    IEnumerator StartWait()
    {
        yield return new WaitForSeconds(3);
        move = true;
    }

    IEnumerator StartSlow()
    {
        yield return new WaitForSeconds(11);
        slowDown = true;
    }
}
