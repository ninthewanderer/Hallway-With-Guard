using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CatBehavior))]
public class CatBehaviorEditor : Editor
{
    private void OnSceneGUI()
    {
        // Creates the view radius arc around the cat.
        CatBehavior catBehavior = target as CatBehavior;
        Handles.color = Color.white;
        Handles.DrawWireArc(catBehavior.transform.position, Vector3.up, Vector3.forward, 360, catBehavior.viewRadius);
        
        // Calculates view angle of the cat to draw it in as a gizmo.
        Vector3 viewAngleLeft = DirectionFromAngle(catBehavior.transform.eulerAngles.y, -catBehavior.viewAngle / 2);
        Vector3 viewAngleRight = DirectionFromAngle(catBehavior.transform.eulerAngles.y, catBehavior.viewAngle / 2);

        Handles.color = Color.yellow;
        Handles.DrawLine(catBehavior.transform.position, catBehavior.transform.position + viewAngleLeft * catBehavior.viewRadius);
        Handles.DrawLine(catBehavior.transform.position, catBehavior.transform.position + viewAngleRight * catBehavior.viewRadius);

        // If the player is seen, a line will be drawn between them and the cat.
        if (catBehavior.playerSpotted)
        {
            Handles.color = Color.green;
            Handles.DrawLine(catBehavior.transform.position, catBehavior.player.transform.position);
        }
    }

    // Calculates the viewing angle of the cat.
    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
