using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Vector3 previousPosition;

    Vector3 movementVector;
    [SerializeField] private float rotationThreshold = 0.0001f;
    [SerializeField] private float rotationSpeed = 10f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        previousPosition = transform.position;
        movementVector = Vector3.zero;

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 delta = transform.position - previousPosition;
        movementVector = Vector3.Lerp(movementVector, delta, 0.1f);

        if (movementVector.sqrMagnitude > rotationThreshold)
        {
            Quaternion target = Quaternion.LookRotation(movementVector);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
        }

        previousPosition = transform.position;
    }
}