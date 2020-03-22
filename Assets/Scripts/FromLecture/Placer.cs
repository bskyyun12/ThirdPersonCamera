using UnityEngine;

public class Placer : MonoBehaviour
{

    public Transform objectToPlace;

    void OnDrawGizmos()
    {
        Transform tf = transform;
        Vector3 rayDir = tf.forward;
        Vector3 origin = tf.position;
        Ray ray = new Ray(origin, rayDir);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(origin, hit.point);
            objectToPlace.position = hit.point;

            Vector3 upDir = hit.normal;
            Gizmos.color = Color.green;
            Gizmos.DrawLine(hit.point, hit.point + upDir);

            Vector3 rightDir = Vector3.Cross(upDir, rayDir).normalized;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(hit.point, hit.point + rightDir);

            Gizmos.color = Color.cyan;
            Vector3 forwardDir = Vector3.Cross(rightDir, upDir);
            Gizmos.DrawLine(hit.point, hit.point + forwardDir);

            Quaternion objRot = Quaternion.LookRotation(forwardDir, hit.normal);
            objectToPlace.rotation = objRot;
        }
    }

}