using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngrybirdController : MonoBehaviour
{
    // 궤적 그리기
    public Transform slingshot;  // 새총 오브젝트
    public Transform birdPrefab;  // 발사할 새 프리팹
    public Trajectory trajectory;  // 궤적을 그릴 Trajectory 스크립트
    public float launchForceMultiplier = 10f;  // 발사 힘의 크기를 조절하는 변수
    public float panSpeed = 0.5f;  // 카메라 이동 속도
    public float reloadTime = 2f;  // 재발사 대기 시간

    private Transform bird;  // 현재 발사할 새 인스턴스
    private Vector3 startPoint;  // 마우스 드래그 시작 지점
    private bool isDragging = false;  // 드래그 상태를 확인하는 플래그
    private bool isPanning = false;  // 화면 이동 상태를 확인하는 플래그
    private bool canLaunch = true;  // 발사 가능 여부를 확인하는 플래그
    private Vector3 lastPanPosition;  // 마지막 팬 위치

    void Start()
    {
        SpawnBird();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))  // 마우스 왼쪽 버튼을 눌렀을 때
        {
            StartDrag();
        }
        else if (Input.GetMouseButton(0) && isDragging)  // 마우스 왼쪽 버튼을 누르고 있는 동안
        {
            ContinueDrag();
        }
        else if (Input.GetMouseButton(0) && isPanning)  // 마우스 왼쪽 버튼을 누르고 있는 동안 팬
        {
            ContinuePan();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)  // 마우스 왼쪽 버튼을 뗐을 때
        {
            ReleaseDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isPanning)  // 마우스 왼쪽 버튼을 뗐을 때 팬 해제
        {
            isPanning = false;
        }
    }

    void StartDrag()
    {
        startPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        startPoint.z = 0;  // 2D 게임이므로 z 좌표를 0으로 설정

        if (canLaunch && Vector3.Distance(startPoint, bird.position) < 1f)  // 새 근처에서 드래그 시작
        {
            isDragging = true;  // 드래그 상태로 변경
            trajectory.ClearLine();  // 기존 궤적을 초기화
        }
        else if (!canLaunch)  // 새를 재발사할 수 없는 상태
        {
            Debug.Log("장전 중");
        }
        else  // 새가 아닌 다른 곳에서 드래그 시작
        {
            isPanning = true;
            lastPanPosition = Input.mousePosition;
        }
    }

    void ContinueDrag()
    {
        Vector3 currentPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentPoint.z = 0;

        Vector3[] points = CalculateTrajectoryPoints(bird.position, (startPoint - currentPoint) * launchForceMultiplier, 50);
        trajectory.RenderLine(bird.position, points);  // 궤적을 렌더링
    }

    void ReleaseDrag()
    {
        isDragging = false;
        Vector3 launchDirection = startPoint - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        launchDirection.z = 0;

        bird.GetComponent<Rigidbody2D>().AddForce(launchDirection * launchForceMultiplier, ForceMode2D.Impulse);
        trajectory.ClearLine();  // 궤적을 초기화
        canLaunch = false;  // 새를 발사할 수 없도록 설정
        StartCoroutine(ReloadBird());  // 재발사 대기 시간 시작

        CameraFollow.instance.FollowBird(bird);  // 카메라가 새를 따라가도록 설정
    }

    void ContinuePan()
    {
        Vector3 currentPanPosition = Input.mousePosition;
        Vector3 difference = Camera.main.ScreenToWorldPoint(lastPanPosition) - Camera.main.ScreenToWorldPoint(currentPanPosition);

        Camera.main.transform.position += difference;
        lastPanPosition = currentPanPosition;
    }

    // 포물선(궤적) 계산 코드 ver.1
    // Vector3[] CalculateTrajectoryPoints(Vector3 startPosition, Vector3 velocity, int numPoints)
    // {
    //     Vector3[] points = new Vector3[numPoints];
    //     float timeStep = 0.1f;  // 시간 간격
    //     Vector3 gravity = Physics2D.gravity;  // 중력 벡터
    //     
    //     for (int i = 0; i < numPoints; i++)
    //     {
    //         float time = i * timeStep;
    //         points[i] = startPosition + velocity * time + 0.5f * gravity * time * time;  // 물리 법칙을 이용한 궤적 계산
    //     }
    //     
    //     return points;  // 계산된 궤적 점들을 반환
    // }
    
    // 포물선(궤적) 계산 코드 ver.1.5
    Vector3[] CalculateTrajectoryPoints(Vector3 startPosition, Vector3 velocity, int numPoints)
    {
        List<Vector3> points = new List<Vector3>();
        float timeStep = 0.1f;  // 시간 간격
        Vector3 gravity = Physics2D.gravity;  // 중력 벡터
        Vector3 currentPosition = startPosition;

        for (int i = 0; i < numPoints; i++)
        {
            float time = i * timeStep;
            Vector3 point = startPosition + velocity * time + 0.5f * gravity * time * time;  // 물리 법칙을 이용한 궤적 계산
            points.Add(point);

            RaycastHit2D hit = Physics2D.Raycast(currentPosition, point - currentPosition, (point - currentPosition).magnitude);
            if (hit.collider != null)
            {
                points.Add(hit.point);
                break;
            }

            currentPosition = point;
        }
        
        Debug.Log("궤적 로그");

        return points.ToArray();  // 계산된 궤적 점들을 반환
    }

    
    // 포물선(궤적) 계산 코드 ver.2
    void PredictTrajectory(Vector3 startPos, Vector3 vel)
    {  
        int step = 60;   
        float deltaTime = Time.fixedDeltaTime;
        Vector3 gravity = Physics.gravity;
  
        Vector3 position = startPos;
        Vector3 velocity = vel;

        for (int i = 0; i < step; i++)
        {
            position += velocity * deltaTime + 0.5f * gravity * deltaTime * deltaTime;
            velocity += gravity * deltaTime;
     
            print(position);
        }
    }

    IEnumerator ReloadBird()
    {
        yield return new WaitForSeconds(reloadTime);  // 재발사 대기 시간
        canLaunch = true;  // 새를 발사할 수 있도록 설정
        SpawnBird();
        Debug.Log("발사 준비 완료");
    }

    void SpawnBird()
    {
        if (bird != null)  // 이미 새가 존재하는 경우에만 새 위치를 업데이트
        {
            Destroy(bird.gameObject);  // 이전 새 오브젝트를 삭제
        }
        bird = Instantiate(birdPrefab, slingshot.position, Quaternion.identity);
        bird.position = slingshot.position;
        CameraFollow.instance.FollowBird(bird);
    }
}
