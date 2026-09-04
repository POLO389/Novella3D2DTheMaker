using System.Collections;
using UnityEngine;

public class ComputerInteraction : MonoBehaviour
{
    [Header("Камера игрока")]
    public Camera playerCamera;

    [Header("Объект игрока (для отключения ходьбы)")]
    public MonoBehaviour playerMovementScript; // скрипт ходьбы игрока

    [Header("Точка сидения перед монитором")]
    public Transform sitPoint;

    [Header("Подсказка [E]")]
    public GameObject interactionHint;

    [Header("Дистанция взаимодействия")]
    public float interactionDistance = 3.5f;

    [Header("Скорость перехода за стол")]
    public float transitionSpeed = 3.0f;

    private bool isPlayerNear = false;
    private bool isSitting = false;
    private Transform originalCameraParent;
    private Vector3 originalCameraLocalPos;
    private Quaternion originalCameraLocalRot;
    private Coroutine cameraTransitionCoroutine;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (playerCamera != null)
        {
            originalCameraParent = playerCamera.transform.parent;
            originalCameraLocalPos = playerCamera.transform.localPosition;
            originalCameraLocalRot = playerCamera.transform.localRotation;
        }

        if (interactionHint != null)
            interactionHint.SetActive(false);
    }

    void Update()
    {
        if (playerCamera == null || sitPoint == null) return;

        // Расстояние от стола до игрока
        float distance = Vector3.Distance(transform.position, playerCamera.transform.position);
        isPlayerNear = distance <= interactionDistance;

        if (isPlayerNear && !isSitting)
        {
            if (interactionHint != null) interactionHint.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                SitDown();
            }
        }
        else if (!isSitting)
        {
            if (interactionHint != null) interactionHint.SetActive(false);
        }

        // Встать из-за стола
        if (isSitting && Input.GetKeyDown(KeyCode.Escape))
        {
            StandUp();
        }
    }

    public void SitDown()
    {
        isSitting = true;
        if (interactionHint != null) interactionHint.SetActive(false);

        // Отключаем управление персонажем, пока сидим
        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        // Разблокируем курсор для кликов по экрану
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cameraTransitionCoroutine != null) StopCoroutine(cameraTransitionCoroutine);
        cameraTransitionCoroutine = StartCoroutine(MoveCamera(sitPoint.position, sitPoint.rotation, sitPoint));
    }

    public void StandUp()
    {
        isSitting = false;

        // Возвращаем камеру назад
        if (cameraTransitionCoroutine != null) StopCoroutine(cameraTransitionCoroutine);
        
        Vector3 targetWorldPos = originalCameraParent != null 
            ? originalCameraParent.TransformPoint(originalCameraLocalPos) 
            : playerCamera.transform.position;
        Quaternion targetWorldRot = originalCameraParent != null 
            ? originalCameraParent.rotation * originalCameraLocalRot 
            : playerCamera.transform.rotation;

        cameraTransitionCoroutine = StartCoroutine(MoveCameraBack(targetWorldPos, targetWorldRot));
    }

    private IEnumerator MoveCamera(Vector3 targetPos, Quaternion targetRot, Transform newParent)
    {
        playerCamera.transform.SetParent(null); // отвязываем от игрока на время полета

        float t = 0f;
        Vector3 startPos = playerCamera.transform.position;
        Quaternion startRot = playerCamera.transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            playerCamera.transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            playerCamera.transform.rotation = Quaternion.Lerp(startRot, targetRot, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        playerCamera.transform.position = targetPos;
        playerCamera.transform.rotation = targetRot;
        playerCamera.transform.SetParent(newParent);
    }

    private IEnumerator MoveCameraBack(Vector3 targetPos, Quaternion targetRot)
    {
        playerCamera.transform.SetParent(null);

        float t = 0f;
        Vector3 startPos = playerCamera.transform.position;
        Quaternion startRot = playerCamera.transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            playerCamera.transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            playerCamera.transform.rotation = Quaternion.Lerp(startRot, targetRot, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // Привязываем камеру обратно к персонажу
        playerCamera.transform.SetParent(originalCameraParent);
        playerCamera.transform.localPosition = originalCameraLocalPos;
        playerCamera.transform.localRotation = originalCameraLocalRot;

        // Включаем обратно ходьбу
        if (playerMovementScript != null)
            playerMovementScript.enabled = true;

        // Прячем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}