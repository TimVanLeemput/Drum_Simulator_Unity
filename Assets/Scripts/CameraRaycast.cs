using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;


public class CameraRaycast : MonoBehaviour
{

    public event Action<RaycastHit> OnRaycastHit;
    [SerializeField, Range(5, 100)] float detectionDistance = 0;
    [SerializeField] bool drumDetect = false;
    [SerializeField] Ray screenRay;
    [SerializeField] LayerMask drumLayer;
    [SerializeField] Stick stick = null;
    [SerializeField] GameObject drumElementHit = null;
    [SerializeField] Drumkit bassDrum = null;
    [SerializeField] RaycastHit drumHit;

    //Inputs 
    [SerializeField] MyInputs controls = null;
    [SerializeField] InputAction mousePos = null;
    [SerializeField] InputsComponent inputs = null;

    float initialDetectionDistance = 0;
    void Awake()
    {
        controls = new MyInputs();
    }
    void Start()
    {
        Init();
    }

    void Init()
    {
        initialDetectionDistance = detectionDistance;
        inputs = GetComponent<InputsComponent>();
    }

    void Update()
    {
        Detect();
    }

    void Detect()
    {
        Vector2 _pos2D = mousePos.ReadValue<Vector2>();
        Vector3 _pos = new Vector3(_pos2D.x, _pos2D.y, detectionDistance);
        screenRay = Camera.main.ScreenPointToRay(_pos);
        bool _hitDrum = Physics.Raycast(screenRay, out RaycastHit _drumHitResult, detectionDistance, drumLayer);
        Debug.DrawRay(screenRay.origin, screenRay.direction * 20);
        drumDetect = _hitDrum;

        if (drumDetect)
        {
            Debug.Log("Drum detected");
            drumHit = _drumHitResult;
            detectionDistance = _drumHitResult.distance + 2;
            UpdateStickPosition(_drumHitResult.point);
            inputs.Hit.performed += HitDrum;    
            OnRaycastHit?.Invoke(_drumHitResult);

        }
        else
        {
            detectionDistance = initialDetectionDistance;
            inputs.Hit.performed -= HitDrum;
        }

    }

    public void HitDrum(InputAction.CallbackContext _context)
    {
        Debug.Log("Called HitDrum Input");
        InteractWithDrumElement(drumHit);
    }

    void UpdateStickPosition(Vector3 _pos)
    {
        stick.transform.position = _pos;
    }

    void InteractWithDrumElement(RaycastHit _hitResult)
    {
        if (!_hitResult.transform) return;
        drumElementHit = _hitResult.transform.gameObject;
        if (!drumElementHit) return;
        Drumkit _drumKitElement = drumElementHit.GetComponent<Drumkit>();
        if (!_drumKitElement) return;
        _drumKitElement.PlaySound();
    }

 

    void OnEnable()
    {
        mousePos = controls.Player.MousePos;
        mousePos.Enable();
    }









}
