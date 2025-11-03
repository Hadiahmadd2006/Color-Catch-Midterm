using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;
    private Rigidbody rb;

    public TMP_Text scoreText;
    public AudioSource sfxAudioSource;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical   = Input.GetAxis("Vertical");

        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward;
        Vector3 camRight   = cam.right;
        camForward.y = 0f;
        camRight.y   = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * moveVertical + camRight * moveHorizontal;

        rb.AddForce(moveDir * speed, ForceMode.Acceleration);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            Coin coin = other.GetComponent<Coin>();
            if (coin != null && GameManager.Instance != null)
                GameManager.Instance.OnCoinCollected(coin);

            other.gameObject.SetActive(false);
        }
    }
}
