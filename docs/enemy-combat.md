# Combate Inimigo

O combate dos monstros é balanceado em torno do tiro de referência da pistola.

## Perfil De Dano

- Dano de referência do tiro: `45`.
- Vida máxima do monstro: `45 * 6 = 270`.
- Multiplicador corpo/geral: `1x`, então 6 tiros no corpo matam.
- Multiplicador peito: `2x`, então 3 tiros no peito matam.
- Multiplicador cabeça: `6x`, então 1 headshot mata.

A matemática fica em `EnemyDamageRules`. A vida runtime do monstro e o roteamento de dano ficam em `EnemyHealth` e `EnemyHitbox`.

## Roteamento De Acerto

`Weapon` verifica se o raycast acertou uma `EnemyHitbox` antes de usar as mensagens legadas `ChangeHealth`/`Damage` do Easy Weapons. Isso impede que um único tiro cause dano duplicado.

Se nenhuma hitbox específica for atingida, mas o collider pertence a um `EnemyHealth`, o tiro é tratado como acerto no corpo.

Headshots acionam `HeadshotAudioFeedback`. O projeto deve usar clipe licenciado para qualquer som de locutor de terceiros. Se nenhum clipe for configurado, o runtime carrega o fallback original `HEADSHOT` em `Resources/Audio/HeadshotAnnouncer`, com fallback procedural apenas se esse asset estiver indisponível.

## Áudio Dos Monstros

`MonsterFollow` é dono do áudio dos monstros. Cada monstro tem um loop ambiente de perseguição com timbre de fera/ghoul, e o volume é calculado por `MonsterAudioRules`: silencioso quando longe, audível em média distância e mais alto perto do jogador. Ataques tocam um rosnado separado, curto e mais agressivo quando o monstro está em alcance de ataque.

Os clipes padrão são assets originais em `Resources/Audio`:

- `MonsterAmbientGrowl`: loop de perseguição de 6 a 10 segundos com respiração pesada, irregular e rosnados secos de fera.
- `MonsterAttackGrowl`: ataque imediato de 1 a 2 segundos, com inspiração rouca rápida e rosnado/mordida gutural.
- `MonsterDeathGrowl`: morte curta, com queda de energia e expiração áspera.

Evite timbres alienígenas, sci-fi, vozes humanas, palavras, música, efeitos cartunescos e chiado de fundo. O som deve parecer orgânico, decadente, animal e próximo de uma fera de filme de terror.

Quando um inimigo morre ou é desativado pelo pool, `MonsterFollow` deve parar imediatamente todos os `AudioSource` locais de loop/ataque. O som de morte toca como one-shot separado na posição do inimigo, para terminar mesmo depois que o loop foi interrompido.

## Spawn

- Intervalo de spawn: 15 segundos.
- Primeiro spawn: imediato quando a cena de jogo começa.
- Limite padrão de vivos: 3 monstros.
- O gerador usa pool para evitar alocação e destruição repetidas durante o gameplay Android.
- A morte notifica o gerador uma vez, reduz a contagem de vivos e devolve o monstro ao pool após o atraso de morte.
- Candidatos de spawn são amostrados contra a NavMesh antes da ativação do monstro.
- Um spawn válido precisa ter caminho completo até o jogador, ficar pelo menos 14 metros distante, estar fora do campo de visão da câmera com margem de segurança e não sobrepor `EnemySpawnExclusionZone`.
- A porta/sala final de fuga é excluída do spawn. Inimigos não conseguem abrir essa porta de conclusão trancada, então não podem aparecer perto dela nem dentro dela.
- Portas normais da rota jogável aceitam jogador e inimigos para que o monstro consiga cruzar passagens enquanto persegue o jogador.

## Testes

Os testes EditMode em `Assets/Tests/EditMode` cobrem contagem de tiros necessária, roteamento de headshot, áudio de proximidade de monstro, regras de spawn, exclusão da área final, validade da NavMesh da cena, posicionamento fora da câmera e candidatos repetidos de spawn validado. Atualize esses testes sempre que o perfil de combate ou o contrato de spawn mudar.
