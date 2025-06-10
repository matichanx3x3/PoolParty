using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 originalPosition;
    float originalSize;

    void Start()
    {
        originalPosition = transform.position;
        originalSize = Camera.main.orthographicSize;
    }

    void Update()
    {
        //DEBUG: Está puesto para que al hacer click izquierdo se haga el zoom y con el derecho se vuelve a la posición original
        if (Input.GetMouseButtonDown(0))
        {
            MoveToTarget();
        }
        if (Input.GetMouseButtonDown(1))
        {
            ReturnToOriginal();
        }
    }

    public void MoveToTarget()
    {
        Vector3 targetPosition = new Vector3(1.5f, 0.5f, transform.position.z);
        float targetSize = 3.5f;
        StartCoroutine(SmoothMoveAndZoom(targetPosition, targetSize, 1f));
    }

    public void ReturnToOriginal()
    {
        StartCoroutine(SmoothMoveAndZoom(originalPosition, originalSize, 1f));
    }

    IEnumerator SmoothMoveAndZoom(Vector3 targetPosition, float targetSize, float duration)
    {
        Vector3 startPos = transform.position;
        float startSize = Camera.main.orthographicSize;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            transform.position = Vector3.Lerp(startPos, targetPosition, t);
            Camera.main.orthographicSize = Mathf.Lerp(startSize, targetSize, t);

            yield return null;
        }

        transform.position = targetPosition;
        Camera.main.orthographicSize = targetSize;
    }
}
