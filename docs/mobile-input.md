# Arquitetura De Input Mobile

O input mobile é dividido em três camadas principais: zonas de toque, política de câmera e ponte runtime.

## 1. Zonas

`Assets/Scripts Geral/Controles/Core/MobileTouchZones.cs` define áreas normalizadas da tela para movimento, botões inferiores direitos de atirar/recarregar e olhar da câmera. As regras consideram coordenadas de tela inteira e `safeArea`, para funcionar de forma consistente em aparelhos com recorte, bordas arredondadas ou barras de sistema.

A zona reserva de movimento é limitada intencionalmente à região inferior esquerda do joystick. Não reserve o lado esquerdo inteiro: o espaço livre acima do joystick deve continuar disponível para olhar com a câmera.

As zonas reservas de atirar e recarregar ficam apenas na região inferior direita. O espaço direito central deve continuar disponível para ver o cenário e girar a câmera.

## 2. Política De Câmera

`Assets/Scripts Geral/Controles/Core/MobileCameraTouchPolicy.cs` é a fonte de verdade para escolher o dedo da câmera. Ela é independente de `Input.GetTouch`, permitindo testes EditMode sem aparelho real.

Regras:

- Se um toque começa em movimento, atirar, recarregar ou UI, esse dedo fica bloqueado para câmera até terminar.
- Apenas um dedo de câmera fica ativo por vez.
- Um segundo dedo pode virar o dedo de câmera enquanto o dedo de movimento continua bloqueado.
- Mover um dedo bloqueado retorna delta zero de câmera.
- Qualquer toque que começa fora de joystick, botões e UI pode virar dedo de câmera, incluindo área livre do lado esquerdo acima do joystick.

## 3. Ponte Runtime

`MobileTouchInputBridge` lê toques Android e os envia para botões Unity e componentes de joystick. `CameraMobile` lê os toques separadamente, mas delega decisões de posse para `MobileCameraTouchPolicy`.

Essa separação evita o bug clássico de FPS mobile em que o dedo do joystick também gira a câmera.

## 4. Movimento

`MovimentoMobile` lê o joystick esquerdo visível e delega a matemática do vetor para `MobileMovementVector`.

- Input horizontal vira strafe local por `transform.right`.
- Input vertical vira movimento local para frente/trás por `transform.forward`.
- Input diagonal é limitado para não deixar a diagonal mais rápida que o movimento reto.
- O vetor de movimento é projetado no plano do chão para impedir que pitch da câmera afete o deslocamento do jogador.

## 5. Scripts Desktop Legados

O prefab do jogador ainda contém componentes de exemplo do Easy Weapons para movimento desktop, mouse look e head bob. No Android eles não devem rodar:

- `FirstPersonCharacter` é desativado para não sobrescrever o movimento mobile do rigidbody.
- `MouseRotator` é desativado para impedir que toques virem input de mouse look desktop.
- `FirstPersonHeadBob` é desativado para impedir que andar incline ou role a câmera.
