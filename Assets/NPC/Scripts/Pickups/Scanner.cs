using System.Collections;
using System.Linq;
using NPC.Scripts.Characters;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NPC.Scripts.Pickups
{
    public class Scanner : BasePickup
    {
        private LineRenderer _scanLine;

        [SerializeField, Range(1f, 5f), Space(10)] private float scanLineDuration = 3f;

        private void Start()
        {
            SetupLineRenderer();
        }
        private void SetupLineRenderer()
        {
            _scanLine = gameObject.AddComponent<LineRenderer>();
            _scanLine.positionCount = 2;
            _scanLine.startWidth = 0.1f;
            _scanLine.material = new Material(Shader.Find("Unlit/Color"));
            _scanLine.material.color = Color.yellow;
            _scanLine.alignment = LineAlignment.TransformZ;
        }

        public override void Pickup(Player player)
        {
            base.Pickup(player);
            player.PickupInventoryItem(this, pickupSprite);
            
        }
        
        public override void UseEquipment()
        {
            base.UseEquipment();
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y));
            Vector3 position = AccessingPlayer.transform.position;
            RaycastHit2D[] hits = Physics2D.RaycastAll(position, mousePosition - new Vector2(position.x, position.y), Vector2.Distance(mousePosition, position)).OrderBy(h => h.distance).ToArray();
            Collider2D thisCollider = AccessingPlayer.GetComponent<Collider2D>();
            StartCoroutine(ScanLine(mousePosition));
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider != thisCollider)
                {
                    Character character = hit.transform.GetComponent<Character>();
                    if (character != null)
                    {
                        character.Scanned();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private IEnumerator ScanLine(Vector2 endPos)
        {
            _scanLine.enabled = true;
            _scanLine.SetPosition(0, AccessingPlayer.transform.position);
            _scanLine.SetPosition(1, endPos);
            yield return new WaitForSeconds(scanLineDuration);
            _scanLine.enabled = false;
        }
    }
}
