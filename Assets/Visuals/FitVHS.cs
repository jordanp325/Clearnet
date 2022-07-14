using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitVHS : MonoBehaviour
{
    Plane plane = new Plane(Vector3.back, new Vector3(0, 0, -9.5f));
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float dist1;
        Ray ray1 = Camera.main.ViewportPointToRay(Vector3.zero);
        plane.Raycast(ray1, out dist1);
        Vector3 point1 = ray1.GetPoint(dist1);

        float dist2;
        Ray ray2 = Camera.main.ViewportPointToRay(Vector3.one);
        plane.Raycast(ray2, out dist2);
        Vector3 point2 = ray2.GetPoint(dist2);

        transform.position = (point1 + point2) / 2;
        transform.localScale = new Vector3(Mathf.Abs(point1.x - point2.x), Mathf.Abs(point1.y - point2.y), .01f);
        
    }
}
