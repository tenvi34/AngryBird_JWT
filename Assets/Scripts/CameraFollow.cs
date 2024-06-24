using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance;  // 싱글톤 인스턴스
    public Transform target;  // 카메라가 따라갈 타겟
    public float smoothSpeed = 0.125f;  // 카메라 이동 속도
    public Vector3 offset;  // 카메라 오프셋
    public bool isFollowing = false;  // 카메라가 타겟을 따라가는지 여부

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LateUpdate()
    {
        if (isFollowing && target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }

    public void FollowBird(Transform bird)
    {
        target = bird;
        isFollowing = true;
    }

    public void StopFollowing()
    {
        isFollowing = false;
    }
}
