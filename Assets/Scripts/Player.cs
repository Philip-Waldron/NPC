using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public int Ammo;
    public float DisguiseIntegrity;

    [SerializeField]
    private float _timeToMove;
    [SerializeField]
    private float _bulletChargeTime;

    private Vector2 _moveDirection;
    private bool _moving;
    
    private bool _bulletCharging;
    private bool _chargeReady;
    private int _startBulletChargeFrame;
    private float _chargingFor;
    private LineRenderer _bulletLine;
    private Coroutine _lineRendererAnimation;

    private void Awake()
    {
        _bulletLine = gameObject.AddComponent<LineRenderer>();
        _bulletLine.positionCount = 2;
        _bulletLine.startWidth = 0.1f;
        _bulletLine.material = new Material(Shader.Find("Unlit/Color"));
        _bulletLine.material.color = Color.cyan;
        _bulletLine.alignment = LineAlignment.TransformZ;
    }

    private void Update()
    {
        if (!_moving && _moveDirection != Vector2.zero)
        {
            StartCoroutine(MoveToPosition(transform, transform.position + new Vector3(_moveDirection.x, _moveDirection.y), _timeToMove));
        }

        if (_bulletCharging && _startBulletChargeFrame != Time.frameCount)
        {
            _bulletLine.SetPositions(new Vector3[] { transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y)) });
            _chargingFor += Time.deltaTime;

            if (_chargingFor >= _bulletChargeTime && !_chargeReady)
            {
                if (_lineRendererAnimation != null)
                {
                    StopCoroutine(_lineRendererAnimation);
                }
                
                _lineRendererAnimation = StartCoroutine(FlashLineRenderer(0.2f, 1, Color.white, Color.cyan));
                _chargeReady = true;
            }
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector2 moveDirection = context.ReadValue<Vector2>();

            if (moveDirection == Vector2.up || moveDirection == Vector2.down ||
                moveDirection == Vector2.left || moveDirection == Vector2.right)
            {
                _moveDirection = moveDirection;
            }
        }
        else if (context.ReadValue<Vector2>() == Vector2.zero)
        {
            _moveDirection = Vector2.zero;
        }
    }

    public void Emote1(InputAction.CallbackContext context)
    {
        if (context.action.triggered)
        {
            print("Emote1");
        }
    }
    
    public void Emote2(InputAction.CallbackContext context)
    {
        if (context.action.triggered)
        {
            print("Emote2");
        }
    }
    
    public void Emote3(InputAction.CallbackContext context)
    {
        if (context.action.triggered)
        {
            print("Emote3");
        }
    }
    
    public void Emote4(InputAction.CallbackContext context)
    {
        if (context.action.triggered)
        {
            print("Emote4");
        }
    }

    public void ShootCharge(InputAction.CallbackContext context)
    {
        if (context.performed && Ammo > 0)
        {
            if (_lineRendererAnimation != null)
            {
                StopCoroutine(_lineRendererAnimation);
                _bulletLine.material.color = Color.cyan;
            }
            
            _bulletLine.SetPositions(new Vector3[] { transform.position, Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y)) });
            _bulletLine.enabled = true;
            _bulletCharging = true;
            _startBulletChargeFrame = Time.frameCount;
        }
    }
    
    public void ShootRelease(InputAction.CallbackContext context)
    {
        if (context.performed && _bulletCharging)
        {
            if (_chargingFor >= _bulletChargeTime)
            {
                Shoot();
            }
            else
            {
                _bulletLine.enabled = false;
            }
            
            _bulletCharging = false;
            _chargeReady = false;
            _chargingFor = 0;
        }
    }

    private void Shoot()
    {
        if (_lineRendererAnimation != null)
        {
            StopCoroutine(_lineRendererAnimation);
            _bulletLine.material.color = Color.cyan;
        }
        
        _lineRendererAnimation = StartCoroutine(FlashLineRenderer(0.1f, 3, Color.red, Color.cyan, true));

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x,
            Mouse.current.position.ReadValue().y));
        RaycastHit2D[] hits = Physics2D
            .RaycastAll(transform.position, mousePosition - new Vector2(transform.position.x, transform.position.y),
                Vector2.Distance(mousePosition, transform.position)).OrderBy(h => h.distance).ToArray();

        Collider2D thisCollider = transform.GetComponent<Collider2D>();

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider != thisCollider)
            {
                Player player = hit.transform.GetComponent<Player>();
                if (player != null)
                {
                    player.Kill();
                }
                else
                {
                    break;
                }
            }
        }

        Ammo--;
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (context.action.triggered)
        {
            print("Interact");
        }
    }

    public void Kill()
    {
        gameObject.GetComponent<ParticleSystem>().Play();
    }
    
    private IEnumerator MoveToPosition(Transform transform, Vector3 position, float timeToMove)
    {
        _moving = true;
        Vector3 currentPos = transform.position;
        float currentTime = 0f;
        while(currentTime < 1)
        {
            currentTime += Time.deltaTime / timeToMove;
            transform.position = Vector3.Lerp(currentPos, position, currentTime);
            yield return null;
        }

        if (_moveDirection != Vector2.zero)
        {
            StartCoroutine(MoveToPosition(transform, transform.position + new Vector3(_moveDirection.x, _moveDirection.y), _timeToMove));
        }
        else
        {
            _moving = false;
        }
    }

    private IEnumerator FlashLineRenderer(float flashTime, int flashCount, Color flashColor, Color baseColor, bool disableLineRenderer = false)
    {
        int count = 0;
        while (count < flashCount)
        {
            _bulletLine.material.color = flashColor;
            yield return new WaitForSeconds(flashTime);
            _bulletLine.material.color = baseColor;
            yield return new WaitForSeconds(flashTime);
            count++;
        }

        if (disableLineRenderer)
        {
            _bulletLine.enabled = false;
        }
    }
}
