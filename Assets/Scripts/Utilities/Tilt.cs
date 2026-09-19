using UnityEngine;

public class Tilt : MonoBehaviour
{
    public LayerMask mask;

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            var slopeRotation = Quaternion.FromToRotation(transform.up, hit.normal);
            transform.rotation = Quaternion.Slerp(transform.rotation, slopeRotation * transform.rotation, 10 * Time.deltaTime);
        }
    }
}
