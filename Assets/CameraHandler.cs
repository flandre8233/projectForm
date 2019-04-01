using UnityEngine;
using System.Collections;

public class CameraHandler : SingletonMonoBehavior<CameraHandler> {

    private static readonly float PanSpeed = 20f;
    private static readonly float ZoomSpeedTouch = 0.1f;
    private static readonly float ZoomSpeedMouse = 2.5f;

    public static readonly float[] ZoomBounds = new float[] { 2.5f , 7f };


    private Camera cam;

    private bool panActive;
    private Vector3 lastPanPosition;
    private int panFingerId; // Touch mode only

    private bool zoomActive;
    private Vector2[] lastZoomPositions; // Touch mode only

     public void init() {
        cam = GetComponent<Camera>();
        float orthographicSize = cam.orthographicSize;
        ZoomBounds[ 0 ] = orthographicSize * 0.5f;
        ZoomBounds[ 1 ] = orthographicSize * 1.5f;

#if UNITY_ANDROID || UNITY_IOS
		cam.orthographicSize = 60f;
#endif

        globalUpdateManager.instance.registerUpdateDg(ToUpdate);
    }

    private void OnDestroy() {
        globalUpdateManager.instance.UnregisterUpdateDg(ToUpdate);
    }

    void ToUpdate() {
        if (Input.touchSupported && Application.platform != RuntimePlatform.WebGLPlayer) {
            HandleTouch();
        } else {
            HandleMouse();
        }
    }

    void HandleTouch() {
        switch (Input.touchCount) {

            case 1: // Panning
                zoomActive = false;

                // If the touch began, capture its position and its finger ID.
                // Otherwise, if the finger ID of the touch doesn't match, skip it.
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) {
                    lastPanPosition = touch.position;
                    panFingerId = touch.fingerId;
                    panActive = true;
                } else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved) {
                    PanCamera(touch.position);
                }
                break;

            case 2: // Zooming
                panActive = false;

                Vector2[] newPositions = new Vector2[] { Input.GetTouch(0).position, Input.GetTouch(1).position };
                if (!zoomActive) {
                    lastZoomPositions = newPositions;
                    zoomActive = true;
                } else {
                    // Zoom based on the distance between the new positions compared to the 
                    // distance between the previous positions.
                    float newDistance = Vector2.Distance(newPositions[ 0 ], newPositions[ 1 ]);
                    float oldDistance = Vector2.Distance(lastZoomPositions[ 0 ], lastZoomPositions[ 1 ]);
                    float offset = newDistance - oldDistance;

                    ZoomCamera(offset, ZoomSpeedTouch);

                    lastZoomPositions = newPositions;
                }
                break;

            default:
                panActive = false;
                zoomActive = false;
                break;
        }
    }
    public bool inPan = false;

    void HandleMouse() {
        // On mouse down, capture it's position.
        // On mouse up, disable panning.
        // If there is no mouse being pressed, do nothing.
        if (Input.GetMouseButtonDown(0)) {
            panActive = true;
            lastPanPosition = Input.mousePosition;
        } else if (Input.GetMouseButtonUp(0)) {
            inPan = false;
            panActive = false;
        } else if (Input.GetMouseButton(0)) {
            PanCamera(Input.mousePosition);
        }

        // Check for scrolling to zoom the camera
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        zoomActive = true;
        ZoomCamera(scroll, ZoomSpeedMouse);
        zoomActive = false;
        
    }

    void PanCamera(Vector3 newPanPosition) {
        if (!panActive) {
            return;
        }
        if (!inPan && Vector3.Distance(lastPanPosition,Input.mousePosition) >= 0.5f) {
            print(Vector3.Distance(lastPanPosition, Input.mousePosition));
            inPan = true;
        }

        // Translate the camera position based on the new input position
        Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        Vector3 move = new Vector3(offset.x * PanSpeed, offset.y * PanSpeed, 0);
        transform.Translate(move, Space.World);
        ClampToBounds();

        lastPanPosition = newPanPosition;
    }

    void ZoomCamera(float offset, float speed) {
        if (!zoomActive || offset == 0) {
            return;
        }

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - (offset * speed), ZoomBounds[ 0 ], ZoomBounds[ 1 ]);
        ClampToBounds();
    }

    void ClampToBounds() {

        Camera cam = GetComponent<Camera>();
        var vertExtent = cam.orthographicSize;
        var horzExtent = vertExtent * Screen.width / Screen.height;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(transform.position.x, -gameModel.instance.mapRadius + horzExtent + 1, gameModel.instance.mapRadius - horzExtent + 1);
        pos.y = Mathf.Clamp(transform.position.y, -gameModel.instance.mapRadius + vertExtent + 1, gameModel.instance.mapRadius - vertExtent + 1);

        transform.position = pos;
    }
}