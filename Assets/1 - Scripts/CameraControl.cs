using Cinemachine;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivalChaos
{
    public class CameraControl : MonoBehaviour
    {
        public static CameraControl instance;

        [Header("Movement Settings")]
        public float panSens = 0.3f;
        public float smoothDamp = 4f;
        public float rotationSens = 1.5f;
        public float rotationSmooth = 25;

        [Header("Zoom Settings")]
        public float minHeight = 10f;
        public float maxHeight = 75f;
        public float scrollZoomSensitivity = 10f;
        public float heightDampening = 5f;

        [Header("Movement Bounds")]
        public float cameraBoundMinX;
        public float cameraBoundMaxX;
        public float cameraBoundMinZ;
        public float cameraBoundMaxZ;

        [Header("Axis Strings")]
        public string horizontalAxis = "Horizontal";
        public string verticalAxis = "Vertical";
        public string zoomingAxis = "Mouse ScrollWheel";
        public string mouseHorizontalAxis = "Mouse X";
        public string mouseVerticalAxis = "Mouse Y";

        public Vector2 inputAxis;
        public Vector2 mouseAxis;
        public float mouseScroll;
        [HideInInspector] public Vector3 newPos;
        [HideInInspector] public Quaternion newRot;
        [SerializeField] private float difference;
        [SerializeField] private float targetHeight;
        private bool rotating;
        Transform _transform;
        public bool controlsEnabled;

        [SerializeField] private CinemachineFreeLook freeLookCamera;
        //[SerializeField] private CinemachineVirtualCamera virtualCamera;
        //[SerializeField] private CinemachineTransposer transposer;


        private void OnEnable()
        {
            MainBase.OnMainBaseSpawned += SetCamera;
            GameTimer.OnTimeRemaining += OnTimeRemainingEnableControls;
        }

        private void OnDisable()
        {
            MainBase.OnMainBaseSpawned -= SetCamera;
            GameTimer.OnTimeRemaining -= OnTimeRemainingEnableControls;
        }

        private void Awake()
        {
            instance = this;

            Application.targetFrameRate = 60; //move

            _transform = transform;

            //((GameNetworkManager)NetworkManager.singleton).cam = this;
        }

        private void SetCamera(MainBase mainBase)
        {
            freeLookCamera.m_YAxis.Value = 0.3f;
            newPos = mainBase.transform.position;
            transform.rotation = mainBase.transform.rotation;
        }

        private void Update()
        {
            if (!controlsEnabled)
            {
                freeLookCamera.m_YAxis.m_MaxSpeed = 0;

                return;
            }

            freeLookCamera.m_YAxis.m_MaxSpeed = 0.8f;

            inputAxis = new Vector2(Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis)).normalized;
            mouseAxis = new Vector2(Input.GetAxis(mouseHorizontalAxis), Input.GetAxis(mouseVerticalAxis)).normalized;
            mouseScroll = Input.GetAxisRaw(zoomingAxis);

            //HandleZoomInput();
            HandleMovementInput();
            HandleRotationInput();
        }

        private void OnTimeRemainingEnableControls(float timeRemaining)
        {
            if(timeRemaining <= 1) controlsEnabled = true;
        }

        private void LateUpdate()
        {
/*            HandleMovementInput();
            HandleRotationInput();
*/        }

        private void HandleZoomInput()
        {
            if (mouseScroll == 0) return;

            //transposer.m_FollowOffset.y += (mouseScroll * scrollZoomSensitivity);
        }

        private void HandleMovementInput() //add camera smoothing
        {
            var facing = inputAxis.magnitude > 0 ? _transform.forward.normalized * inputAxis.y + _transform.right.normalized * inputAxis.x : Vector3.zero;
            newPos += facing * panSens;

            /*_transform.position = Vector3.Lerp(_transform.position,
                new Vector3(Mathf.Clamp(newPos.x, cameraBoundMinX, cameraBoundMaxX),
                targetHeight + difference,
                Mathf.Clamp(newPos.z, cameraBoundMinZ, cameraBoundMaxZ)),
                Time.deltaTime * GetNormalizedValue(smoothDamp, 1f, 10f));*/

            _transform.position = new Vector3(Mathf.Clamp(newPos.x, cameraBoundMinX, cameraBoundMaxX),
                                  targetHeight + difference,
                                  Mathf.Clamp(newPos.z, cameraBoundMinZ, cameraBoundMaxZ));
        }

        private void HandleRotationInput() //add smoothing, set x axis value of viritual cam
        {
            Cursor.visible = rotating ? Cursor.visible = false : Cursor.visible = true;
            Cursor.lockState = rotating ? CursorLockMode.Locked : CursorLockMode.None;

            var transformEulerAngles = _transform.eulerAngles;

            if (Input.GetKey(KeyCode.Mouse2))
            {
                rotating = true;

                //clamp
                transformEulerAngles.y += mouseAxis.x;
                //virtualFreeLookCamera.m_XAxis.Value += mouseAxis.x;

            }
            else rotating = false;

            _transform.eulerAngles = Vector3.Lerp(_transform.eulerAngles, transformEulerAngles, Time.deltaTime * rotationSens);
        }

        private float GetNormalizedValue(float value, float newMin, float newMax)
        {
            return newMin + value * (newMax - newMin);
        }
    }
}