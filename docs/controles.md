# Controles Mobile

Este arquivo define o modelo pretendido de controles Android. Mantenha-o sincronizado com `AGENTS.md` e com os testes em `Assets/Tests/EditMode`.

## Posse Do Toque

Cada dedo tem uma função desde o momento em que toca a tela até terminar.

- Dedo de movimento: começa no joystick esquerdo ou na zona esquerda de movimento. Move apenas o jogador. Nunca gira a câmera.
- Dedo de câmera: começa fora das zonas de joystick, atirar, recarregar e outras UIs. Gira a câmera enquanto se move.
- Dedo de atirar: começa no botão de atirar. Apenas dispara. Nunca gira a câmera.
- Dedo de recarregar: começa no botão de recarregar. Apenas recarrega. Nunca gira a câmera.

## Comportamento Obrigatório

- Andar para frente com o joystick esquerdo não pode mudar yaw nem pitch da câmera.
- Andar para frente/trás também não pode adicionar roll, inclinação de head bob ou drift de mouse look no Android.
- Andar de lado com o joystick esquerdo não pode mudar yaw nem pitch da câmera.
- Input horizontal do joystick significa strafe esquerda/direita.
- Input vertical do joystick significa mover para frente/trás em relação ao corpo do jogador.
- Arrastar o dedo do joystick para longe do joystick ainda não pode girar a câmera.
- Segurar o joystick com um dedo e arrastar outro dedo válido na área de olhar deve permitir mover e olhar ao mesmo tempo.
- Pressionar ou arrastar sobre atirar/recarregar não pode girar a câmera.
- Atirar e recarregar devem ficar na região inferior direita para manter mais visibilidade do cenário no centro e no lado direito.
- Qualquer toque que não esteja sobre UI, botões ou joystick pode iniciar o olhar da câmera, incluindo espaço livre no lado esquerdo acima do joystick.
- A sensibilidade de toque da câmera é `MobileCameraRules.DefaultTouchSensitivity` (`0.144`), 20% acima da base anterior `0.12`. Mantenha a constante de regra, `CameraMobile.DefaultTouchSensitivity` e o valor da cena de gameplay em sincronia.
- A munição deve aparecer abaixo da vida. Disparar reduz o pente. Recarregar preenche o pente usando munição reserva e atualiza a HUD.
- Toques repetidos no botão de atirar devem disparar tiros repetidos quando a cadência da arma permitir, inclusive em armas semiautomáticas.

## Zonas De Tela Atuais

- Movimento é reservado pelo retângulo real do joystick. A zona reserva fica apenas na área inferior esquerda do joystick, não na metade esquerda inteira da tela.
- Atirar é reservado na zona inferior direita do botão de disparo.
- Recarregar é reservado na zona inferior direita do botão de recarga, à esquerda do botão de atirar.
- Os botões visuais de `Atirar` e `Recarregar` ficam 50% maiores que o layout antigo para facilitar toque em celular, mantendo espaço entre eles e sem voltar para o meio da tela.
- O olhar da câmera pode começar em qualquer outra parte da tela fora dos controles reservados.

Os valores normalizados exatos ficam em `Assets/Scripts Geral/Controles/Core/MobileTouchZones.cs` e são cobertos por testes.

## Testes De Regressão

Os testes EditMode em `Assets/Tests/EditMode` devem passar antes de enviar mudanças em input mobile, movimento, munição ou recarga.

A cobertura atual inclui:

- dedos do joystick arrastados para longe ainda não giram a câmera;
- dedos de atirar/recarregar nunca viram dedo de câmera;
- toques livres fora de UI giram a câmera;
- vetores de movimento ficam no plano do chão e limitam diagonais;
- Android desativa input desktop legado e head bob;
- disparar consome uma bala do pente, toques repetidos liberam o gate semiautomático e recarregar usa munição reserva;
- a cena de gameplay mantém a sensibilidade aumentada da câmera mobile.

## Relação Entre Toque E Combate

- Toques no botão de atirar apenas disparam e não podem iniciar rotação de câmera.
- Toques no botão de recarregar apenas recarregam e não podem iniciar rotação de câmera.
- Um tiro que acerta um monstro deve passar uma única vez pelo sistema de hitbox/zona de dano, para que disparos mobile e desktop produzam o mesmo comportamento de cabeça, peito e corpo.
