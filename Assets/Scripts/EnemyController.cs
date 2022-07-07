using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float Speed = 1;
    private Vector2 direction;
    [SerializeField] private Transform Target;
    [SerializeField] private float range;
    [SerializeField] private LayerMask layers;
    [SerializeField] private LayerMask patrolLayers;
    [SerializeField] private bool patrolTheArea;
    private float offset = 0.1f;
    private bool detected;
    private Rigidbody2D Rigidbody2D;
    private float initialDirection = 1f;
    private Animator Animator;
    // Start is called before the first frame update
    void Start()
    {

        Rigidbody2D = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        direction = (Vector2)Target.position - (Vector2)transform.position;
        RaycastHit2D rayInfo = Physics2D.Raycast(transform.position, direction, range, layers);
        if (rayInfo)
        {

            if (rayInfo.collider.gameObject.tag == "Player")
            {
                detected = true;
                transform.localScale = new Vector3(direction.x < 0 ? -1.0f : 1.0f, transform.localScale.y, transform.localScale.z);
                Debug.Log("Player detected.");
                Debug.DrawRay(transform.position, direction * range, Color.green);
            }
            else
            {
                detected = false;
            }
        }
        else
        {
            detected = false;
        }

        if (patrolTheArea && !detected)
        {
            this.Rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
            var enemyPosition = new Vector3(transform.position.x + (transform.localScale.x < 0 ? -offset : offset), transform.position.y);

            var grounded = Physics2D.Raycast(enemyPosition, Vector3.down, 0.1f);
            if (!grounded)
            {
                initialDirection *= -1;
            }
            else if (Physics2D.Raycast(transform.position, Vector3.right, 0.1f, layers))
            {
                initialDirection = -1.0f;
            }
            else if (Physics2D.Raycast(transform.position, Vector3.left, 0.1f, layers))
            {
                initialDirection = 1.0f;
            }
            transform.localScale = new Vector3(initialDirection < 0 ? -1.0f : 1.0f, transform.localScale.y, transform.localScale.z);
            Rigidbody2D.velocity = new Vector2(initialDirection * Speed, Rigidbody2D.velocity.y);

            Animator.SetBool("running", true);
        }
        else
        {
            Rigidbody2D.velocity = new Vector2(0, Rigidbody2D.velocity.y);
            Animator.SetBool("running", false);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
