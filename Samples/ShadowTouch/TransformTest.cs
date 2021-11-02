using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformTest : MonoBehaviour
{
    public Transform obj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    bool inc = false;

    // Update is called once per frame
    void Update()
    {
        // translate
        //obj.TranslateByLocalVector(new Vector3(0, 0, -0.01f));
        //obj.TranslateByWorldVector(new Vector3(0, 0.01f, 0));


        // rotate
        //obj.RotateAroundLocalOrigin(Quaternion.AngleAxis(1.0f, Vector3.up));
        //obj.RotateAroundWorldOrigin(Quaternion.AngleAxis(1.0f, Vector3.up));

        //obj.RotateAroundLocalPoint(new Vector3(0,0,2), Quaternion.AngleAxis(1.0f, Vector3.up));
        //obj.RotateAroundWorldPoint(new Vector3(2, -1, -1), Quaternion.AngleAxis(1.0f, Vector3.up));


        // scale 
        if (obj.localScale.x < 0.1f) {
            inc = true;
        } else if (obj.localScale.x > 3.0f) {
            inc = false;
        }
        float s = 1.01f;
        if (!inc) {
            s = 0.99f;
        }
        //obj.ScaleAroundLocalOrigin(s);
        //obj.ScaleAroundWorldOrigin(s);

        //obj.ScaleAroundLocalPoint(new Vector3(0.5f, 0.5f, 0.5f), s);
        //obj.ScaleAroundWorldPoint(new Vector3(1, 0, 1), s);
    }
}
