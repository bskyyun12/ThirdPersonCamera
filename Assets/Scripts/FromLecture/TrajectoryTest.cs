using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TrajectoryTest : MonoBehaviour
{

	public Camera playerCamera;

	[Range(0f, 8)]
	public float launchSpeed;

	Vector3 mouseGroundPosition;

	void OnDrawGizmos()
	{
		// calc launch angle
		Vector3 dirPlayerToTarget = (mouseGroundPosition - transform.position).normalized;
		float distToTarget = Vector3.Distance(transform.position, mouseGroundPosition);

		(float angleLow, float angleHigh) launchAngles = GetLaunchAngle(distToTarget, launchSpeed);


		Gizmos.color = Color.white;
		const int pointCount = 32;
		const float dt = 1f / 20f;
		for (int i = 0; i < pointCount; i++)
		{
			float time = i * dt;
			Vector2 pt2DA = GetPointInTrajectory(transform.position, launchSpeed, launchAngles.angleLow, time);
			Vector2 pt2DB = GetPointInTrajectory(transform.position, launchSpeed, launchAngles.angleHigh, time);

			void TryDrawTrajectoryPoint(Vector2 point2D)
			{
				if (point2D.y < 0)
					return;
				Vector3 pt = dirPlayerToTarget * point2D.x; // lateral offset
				pt.y = point2D.y; // vertical offset
				Gizmos.DrawSphere(pt, 0.1f);
			}

			TryDrawTrajectoryPoint(pt2DA);
			TryDrawTrajectoryPoint(pt2DB);
		}


		Gizmos.color = Color.red;
		Gizmos.DrawSphere(mouseGroundPosition, 0.15f);
	}


	void Update()
	{
		mouseGroundPosition = GetCursorGroundPosition();
	}

	// d = s*s*sin(2*angle) / g
	// d * g = s*s*sin(2*angle)
	// (d * g) / (s*s) = sin(2*angle)
	// asin( (d*g) / (s*s) ) = 2*angle
	// asin( (d*g) / (s*s) ) / 2 = angle
	// angle = asin( (d*g) / (s*s) ) / 2 
	static (float, float) GetLaunchAngle(float dist, float speed)
	{
		float gravity = Physics.gravity.y;
		float asinContent = Mathf.Clamp((dist * -gravity) / (speed * speed), -1, 1);
		return
			(Mathf.Asin(asinContent) / 2,
				(Mathf.Asin(-asinContent) + Mathf.PI) / 2);
	}

	static Vector2 GetPointInTrajectory(Vector2 startPoint, float launchSpeed, float launchAngRad, float time)
	{
		float gravity = Physics.gravity.y;
		float xDisp = launchSpeed * time * Mathf.Cos(launchAngRad);
		float yDisp = launchSpeed * time * Mathf.Sin(launchAngRad) + .5f * gravity * time * time;
		return startPoint + new Vector2(xDisp, yDisp);
	}

	Vector3 GetCursorGroundPosition()
	{
		Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
		Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
		if (groundPlane.Raycast(ray, out float hitDist))
		{
			Vector3 groundPos = ray.GetPoint(hitDist);
			groundPos.y = 0f;
			return groundPos;
		}

		return default;
	}


}