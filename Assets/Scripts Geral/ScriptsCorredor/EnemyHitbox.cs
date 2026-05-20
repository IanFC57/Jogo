using UnityEngine;

public sealed class EnemyHitbox : MonoBehaviour
{
    [SerializeField] private EnemyHitZone zona = EnemyHitZone.Body;
    private EnemyHealth dono;

    public EnemyHitZone Zona => zona;

    public void Configure(EnemyHealth novoDono, EnemyHitZone novaZona)
    {
        dono = novoDono;
        zona = novaZona;
    }

    public void SetOwner(EnemyHealth novoDono)
    {
        dono = novoDono;
    }

    public void ApplyWeaponDamage(float rawWeaponDamage)
    {
        EnemyHealth alvo = dono != null ? dono : GetComponentInParent<EnemyHealth>();
        if (alvo == null)
            return;

        alvo.ApplyWeaponHit(rawWeaponDamage, zona);
    }
}
