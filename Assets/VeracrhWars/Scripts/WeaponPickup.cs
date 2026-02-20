using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [SerializeField] private WeaponType weaponType;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        pc.PickupWeapon(weaponType);
        Destroy(gameObject);
    }
}