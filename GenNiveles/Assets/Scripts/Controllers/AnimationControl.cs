using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimationControl : MonoBehaviour
{
    Animator _anim;
    bool _isWalking = false;

    // Variables de movimiento
    public float moveSpeed = 0.5f;
    private Rigidbody _rb;

    // Detección de suelo con Collider
    public LayerMask groundMask;  // Máscara de capa para el suelo
    private bool isGrounded;

    void Start()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Control de animaciones
        _isWalking = _anim.GetBool("isWalking");

        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) && !Input.GetKey(KeyCode.LeftShift) && !_isWalking)
        {
            _anim.SetBool("isWalking", true);
        }
        else if (!(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) && _isWalking)
        {
            _anim.SetBool("isWalking", false);
        }

        // Si el personaje está en el suelo y presionamos espacio para saltar
        //if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !_isJumping)
        //{
            // Desactivar NavMeshAgent para permitir el salto
            
           // _anim.SetBool("isJumping", true);
            //_rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);  // Salto
            //_isJumping = true;
        //}
        
        Move();
    }

    void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");  // A/D o flechas izquierda/derecha
        float vertical = Input.GetAxis("Vertical");      // W/S o flechas arriba/abajo

        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Rotación del personaje para mirar en la dirección del movimiento
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);

            // Movimiento en la dirección de las teclas WASD
            transform.Translate(direction * (moveSpeed * Time.deltaTime), Space.World);
        }
    }

    // Detección de colisión con el suelo
    private void OnCollisionEnter(Collision collision)
    {
        // Verificamos si el personaje está tocando algo en la capa del suelo
        if (((1 << collision.gameObject.layer) & groundMask) != 0)
        {
            isGrounded = true;
            //_anim.SetBool("isJumping", false);
            //_isJumping = false;
            
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Verificamos si el personaje ha dejado de tocar la capa del suelo
        if (((1 << collision.gameObject.layer) & groundMask) != 0)
        {
            isGrounded = false;
        }
    }
}