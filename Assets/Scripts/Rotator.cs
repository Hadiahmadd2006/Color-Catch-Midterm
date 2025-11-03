using UnityEngine;

public class Rotator : MonoBehaviour
{
    public float speed = 5;
    void Start()
    {
        
    }

    void Update()
    {
        transform.Rotate(new Vector3(0, 10, 0) * Time.deltaTime * speed);
    }
}
