using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public float parallaxEffectMultiplier;  // 패럴랙스 효과 배율
    private Transform cameraTransform;  // 카메라의 트랜스폼
    private Vector3 lastCameraPosition;  // 마지막 카메라 위치
    private float textureUnitSizeX;  // 텍스처의 가로 크기 (단위는 유닛)

    void Start()
    {
        cameraTransform = Camera.main.transform;  // 메인 카메라의 트랜스폼을 가져옴
        lastCameraPosition = cameraTransform.position;  // 초기 카메라 위치를 저장
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;  // 스프라이트 렌더러에서 스프라이트를 가져옴
        Texture2D texture = sprite.texture;  // 스프라이트에서 텍스처를 가져옴
        textureUnitSizeX = texture.width / sprite.pixelsPerUnit;  // 텍스처의 가로 크기 (유닛)를 계산
    }

    void Update()
    {
        // 카메라의 이동량을 계산
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        
        // 배경을 카메라의 이동량에 비례하여 이동
        transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier, 0);
        
        // 마지막 카메라 위치를 현재 위치로 업데이트
        lastCameraPosition = cameraTransform.position;

        // 배경이 무한히 이어지도록 하기 위한 로직
        if (Mathf.Abs(cameraTransform.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offsetPositionX = (cameraTransform.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(cameraTransform.position.x + offsetPositionX, transform.position.y);
        }
    }
}
