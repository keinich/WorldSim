using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

  public Terrain terrain;

  public GameObject subRig;

  public Transform cameraTransform;
  public Camera cam;

  public float normalSpeed;
  public float fastSpeed;
  public float movementTime;
  public float rotationSpeed;
  public Vector3 zoomSpeed;

  public Vector3 newPosition;
  public Quaternion newRotationHorizintal;
  public Quaternion newRotationVertical;
  public Vector3 newZoom;


  public Vector3 dragStartPosition;
  public Vector3 dragCurrentPosition;

  public Vector3 rotateStartPosition;
  public Vector3 rotateCurrentPosition;

  // Start is called before the first frame update
  void Start() {
    newPosition = transform.position;
    newRotationHorizintal = transform.rotation;
    newRotationVertical = subRig.transform.rotation;
    newZoom = cameraTransform.localPosition;
  }

  // Update is called once per frame
  void Update() {
    HandleMouseInput();
    HandleMovementInput();
  }

  private void HandleMouseInput() {

    if (Input.mouseScrollDelta.y != 0) {
      newZoom += Input.mouseScrollDelta.y * zoomSpeed;
    }

    if (Input.GetMouseButtonDown(0)) {
      Plane plane = new Plane(Vector3.up, Vector3.zero);
      Ray ray = cam.ScreenPointToRay(Input.mousePosition);

      float entry;

      if (plane.Raycast(ray, out entry)) {
        dragStartPosition = ray.GetPoint(entry);
        dragStartPosition.y = 0;
      }
    }

    if (Input.GetMouseButton(0)) {
      Plane plane = new Plane(Vector3.up, Vector3.zero);
      Ray ray = cam.ScreenPointToRay(Input.mousePosition);

      float entry;

      if (plane.Raycast(ray, out entry)) {
        dragCurrentPosition = ray.GetPoint(entry);
        dragCurrentPosition.y = 0;

        newPosition = transform.position + dragStartPosition - dragCurrentPosition;
        //print($"startposition.y = {dragStartPosition.y}");
        //print($"dragCurrentPosition.y = {dragCurrentPosition.y}");

        newPosition.y = terrain.SampleHeight(newPosition);
        print($"newPosition.y = {newPosition.y}");
      }
    }

    if (Input.GetMouseButtonDown(2)) {
      rotateStartPosition = Input.mousePosition;
    }
    if (Input.GetMouseButton(2)) {
      rotateCurrentPosition = Input.mousePosition;
      Vector3 difference = rotateStartPosition - rotateCurrentPosition;
      rotateStartPosition = rotateCurrentPosition;

      newRotationHorizintal *= Quaternion.Euler(Vector3.up * (-difference.x / 5F));

      newRotationVertical *= Quaternion.Euler(Vector3.right * (difference.y / 5F));
    }
  }

  private void HandleMovementInput() {

    float movementSpeed;
    if (Input.GetKey(KeyCode.LeftShift)) {
      movementSpeed = fastSpeed;
    }
    else {
      movementSpeed = normalSpeed;
    }

    if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
      newPosition += (transform.forward * movementSpeed);
    }
    if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
      newPosition += (transform.right * -movementSpeed);
    }
    if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
      newPosition += (transform.forward * -movementSpeed);
    }
    if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
      newPosition += (transform.right * movementSpeed);
    }

    if (Input.GetKey(KeyCode.Q)) {
      //newRotation *= Quaternion.Euler(Vector3.up * rotationSpeed);
    }
    if (Input.GetKey(KeyCode.E)) {
      //newRotation *= Quaternion.Euler(Vector3.up * -rotationSpeed);
    }

    if (Input.GetKey(KeyCode.R)) {
      newZoom += zoomSpeed;
    }
    if (Input.GetKey(KeyCode.F)) {
      newZoom -= zoomSpeed;
    }
    transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
    transform.rotation = Quaternion.Lerp(transform.rotation, newRotationHorizintal, Time.deltaTime * movementTime);
    subRig.transform.localRotation = Quaternion.Lerp(subRig.transform.localRotation, newRotationVertical, Time.deltaTime * movementTime);
    cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
  }
}
