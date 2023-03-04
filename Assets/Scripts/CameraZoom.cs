using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    // メインカメラ
    private Camera _mainCamera;

    // 定数定義
    // ズーム速度
    const float ZOOM_SPEED = 0.1f;
    // カメラの最小の視野
    const float ZOOM_MIN = 40.0f;
    // カメラの最大の視野
    const float ZOOM_MAX = 60.0f; 

	void Start()
	{
        // カメラの参照を取得
        _mainCamera = GetComponent<Camera>(); 
	}

	void Update()
	{
        // マルチタッチ(２点同時タッチ)でないなら終了
        if (Input.touchCount != 2)
			return;

		// ２点のタッチ情報を取得する
		var touchData_0 = Input.GetTouch(0);
		var touchData_1 = Input.GetTouch(1);

        // １フレーム前の２点間の距離を求める
        float oldTouchDistance = Vector2.Distance( 
			touchData_0.position - touchData_0.deltaPosition, 
			touchData_1.position - touchData_1.deltaPosition
			);

		// 現在の２点間の距離を求める
		float currentTouchDistance = Vector2.Distance(touchData_0.position, touchData_1.position);

		// ２点間の距離の変化量に応じてズームする(カメラの視野の広さを変更する)
		float distanceMoved = oldTouchDistance - currentTouchDistance;
		_mainCamera.fieldOfView += distanceMoved * ZOOM_SPEED;

		// カメラの視野を指定の範囲に収める
		_mainCamera.fieldOfView = Mathf.Clamp(_mainCamera.fieldOfView, ZOOM_MIN, ZOOM_MAX);
	}
}