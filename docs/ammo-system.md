# Sistema De Munição E Recarga

A arma ativa (`Weapon`) é dona da munição do pente.

- `CurrentAmmo` é a quantidade no pente exibida na HUD.
- `AmmoCapacity` é o tamanho máximo do pente.
- `InventarioJogador.balasNoBolso` é a munição reserva. O jogador começa com 300 balas reservas para que a recarga possa ser validada imediatamente no Android.
- `ControleRecarregamento.TentarRecarregar()` transfere apenas a quantidade que falta no pente, usando a reserva.
- `MobileAmmoHud` mostra munição abaixo da vida e consulta a arma ativa junto com a reserva do inventário.

## Regras

- Disparar reduz `CurrentAmmo`, exceto quando a arma declara munição infinita.
- Cada toque mobile no botão de atirar deve ser tratado como um novo toque semiautomático, permitindo disparos repetidos.
- Recarregar um pente cheio não consome munição reserva.
- Recarregar com menos reserva do que a quantidade faltante carrega apenas a reserva disponível.
- No Android, armas do jogador não devem recarregar gratuitamente de forma automática quando ficam vazias.
