using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    const float smoothTime = .1f;

    Animator animator;
    PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        float walkRunSpeedPercent = playerMovement.currentSpeed / playerMovement.runSpeed;
        animator.SetFloat("speedPercent", walkRunSpeedPercent, smoothTime, Time.deltaTime);
    }
}
