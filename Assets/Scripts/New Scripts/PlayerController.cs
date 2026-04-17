using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Vector3 previousPosition;

    Vector3 movementVector;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        previousPosition = Vector3.zero;
        movementVector = Vector3.zero;

    }

    // Update is called once per frame
    void Update()
    {
        movementVector = Vector3.Lerp(movementVector, transform.position-previousPosition, 0.1f);
        transform.LookAt(transform.position + movementVector);
        previousPosition = transform.position;
    }
}
