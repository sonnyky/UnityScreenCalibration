using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairAnimation : MonoBehaviour
{
    RectTransform m_Transform;

    Vector3 m_InitialRotation = new Vector3(0f,0f,0f);
    Vector3 m_InitialScale = new Vector3(1f, 1f, 1f);

    Vector3 m_Scale = new Vector3(0.03f, 0.03f, 0f);

    float m_RotationDelta = 0.7f;

    // Start is called before the first frame update
    void Start()
    {
        m_Transform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((transform.eulerAngles.z > 45f && transform.eulerAngles.z < 90f) || (transform.eulerAngles.z < 315f && transform.eulerAngles.z > 270f))
        {
            m_RotationDelta *= -1f;
        }
        transform.Rotate(0f, 0f, m_RotationDelta, Space.Self);

        if (transform.localScale.x > 1.5f || transform.localScale.x < 1f)
        {
            m_Scale *= -1f;
        }
        transform.localScale += m_Scale;
    }

    private void OnDisable()
    {
        
    }

    private void OnEnable()
    {
        
    }
}
