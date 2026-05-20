using UnityEngine;

public class ControleRecarregamento : MonoBehaviour
{
    [Header("Inventario do jogador")]
    public InventarioJogador inventario;

    [Header("Arma equipada")]
    public GameObject armaEquipada;

    [Header("Configuracao antiga")]
    public int tamanhoDoPente = 12;

    public void TentarRecarregar()
    {
        if (inventario == null || armaEquipada == null)
        {
            Debug.LogWarning("Recarga sem inventario ou arma configurada.");
            return;
        }

        Weapon weapon = armaEquipada.GetComponent<Weapon>();
        if (weapon == null)
        {
            Debug.LogWarning("A arma equipada nao tem o componente Weapon.");
            return;
        }

        if (!weapon.NeedsAmmo)
        {
            Debug.Log("O pente ja esta cheio.");
            return;
        }

        if (inventario.balasNoBolso <= 0)
        {
            Debug.Log("Sem balas no inventario. Vasculhe os armarios.");
            return;
        }

        int consumedAmmo = weapon.ReloadFromReserve(inventario.balasNoBolso);
        if (consumedAmmo > 0)
        {
            inventario.balasNoBolso -= consumedAmmo;
            Debug.Log("Recarregou " + consumedAmmo + " bala(s). Reserva: " + inventario.balasNoBolso);
        }
        else
        {
            Debug.Log("Nao foi possivel recarregar agora.");
        }
    }
}