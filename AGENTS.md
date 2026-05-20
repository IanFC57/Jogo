# Contexto Para Agentes

Este projeto Unity é um FPS de terror com prioridade absoluta para Android real. Ao trabalhar neste repositório, preserve o contrato de controles mobile antes de mudar jogabilidade, interface ou combate.

## Contrato De Controles Mobile

- O joystick esquerdo serve apenas para movimento.
- Um toque que começa no joystick esquerdo ou na zona reserva inferior esquerda nunca pode girar a câmera, mesmo que esse dedo depois seja arrastado para frente, para os lados ou para o centro da tela.
- A rotação da câmera é controlada por qualquer toque que comece fora do joystick, do botão de atirar, do botão de recarregar e de qualquer outra UI mobile ativa.
- A sensibilidade mobile padrão da câmera é `MobileCameraRules.DefaultTouchSensitivity` (`0.144`), 20% acima do valor antigo `0.12`. Mantenha a cena de jogo serializada com o mesmo valor.
- Toques nos botões de atirar e recarregar nunca podem girar a câmera.
- Os botões de atirar e recarregar ficam na região inferior direita para preservar a visibilidade do cenário no centro e no lado direito da tela.
- Os botões visuais de atirar e recarregar devem permanecer 50% maiores que o layout antigo, com espaço suficiente entre eles e sem bloquear o centro da tela.
- O jogador deve conseguir andar com o joystick e olhar ao redor com um segundo dedo ao mesmo tempo.
- O eixo horizontal do joystick faz strafe para esquerda/direita. O eixo vertical move para frente/trás em relação ao yaw atual do jogador.
- Os auxiliares de movimento, mouse look e head bob do pacote Easy Weapons devem ficar desativados no Android. Pitch e roll da câmera mobile não podem vir de head bob, eixos de mouse ou movimento do joystick.
- A HUD mobile deve mostrar a munição abaixo da vida. Recarregar precisa atualizar esse número usando a munição reserva do jogador.
- A vida do jogador regenera 10 HP a cada 10 segundos apenas depois de o jogador passar 10 segundos completos sem receber dano.
- Não reative rotação de câmera a partir de joystick virtual sem atualizar intencionalmente os testes e esta documentação.

## Expectativas De Validação

- Rode os testes EditMode de input mobile depois de tocar em câmera, joystick, UI ou ponte de toque.
- Rode os testes EditMode de combate inimigo depois de tocar em vida de monstro, hitboxes, roteamento de dano da arma ou tempo de spawn.
- Para comportamento Android, gere uma APK nova com `versionCode` novo antes de testar no aparelho.
- Builds Android feitos pelo Editor e pelo script devem passar por `AndroidBuildConsistency`: package id, IL2CPP, arquiteturas, APK, ícone e tipografia precisam sair iguais.
- O menu `Tools/Android/Build APK Igual Ao Script` usa o mesmo método do build em linha de comando. O `Build Player` normal do Unity também recebe o preflight Android antes de empacotar.
- Ao testar em um Android físico, feche o app antigo, instale a APK nova, confirme `versionCode`/`versionName` e só então abra o jogo.
- O emulador não substitui a validação final quando há celular físico conectado.

## Contrato De Combate Inimigo

- Monstros nascem um por vez a cada 15 segundos enquanto o limite de vivos não foi atingido.
- O primeiro monstro deve nascer imediatamente quando a cena de jogo começa. Depois disso, o ciclo continua a cada 15 segundos.
- A vida do monstro é balanceada contra o dano de referência da pistola: 1 tiro na cabeça, 3 tiros no peito ou 6 tiros no corpo/geral.
- As hitboxes do inimigo são donas das zonas de dano. Cabeça, peito e corpo não podem receber dano duplicado por `ChangeHealth` e `Damage` no mesmo disparo.
- Headshots devem acionar o feedback de headshot do projeto. Não inclua nem recrie áudio de locutor de terceiros protegido por direitos autorais; use um clipe licenciado ou o clipe original `HEADSHOT` incluído no projeto.
- Monstros devem emitir um loop original de perseguição com som de fera/ghoul cujo volume aumenta conforme se aproximam do jogador, tocar um rosnado de ataque distinto quando o ataque conecta e tocar um som curto de morte ao morrer.
- Inimigos mortos ou retornados ao pool devem parar imediatamente todos os `AudioSource` locais de monstro.
- Spawns repetidos no Android devem usar pooling ou outro caminho com alocação controlada, evitando `Instantiate`/`Destroy` recorrentes durante combate.
- A morte do monstro deve notificar o gerador exatamente uma vez para reduzir a contagem de vivos e permitir o próximo spawn de 15 segundos.
- Candidatos de spawn precisam estar na NavMesh, ter caminho completo até o jogador, ficar fora do campo de visão da câmera com margem de segurança, respeitar a distância mínima e ficar fora de `EnemySpawnExclusionZone`.
- A porta/sala final de fuga é uma área de conclusão trancada. Inimigos não podem nascer perto dela nem dentro dela, e essa porta trancada não deve ser tratada como porta que inimigos conseguem abrir.
- Portas normais de navegação podem abrir para jogador e inimigos, permitindo que monstros cruzem passagens jogáveis.

## Arquivos Principais

- `Assets/Scripts Geral/Controles/CameraMobile.cs`: aplica rotação da câmera.
- `Assets/Scripts Geral/Controles/Core/MobileCameraRules.cs`: concentra constantes de câmera mobile, incluindo sensibilidade.
- `Assets/Scripts Geral/Controles/Core/MobileCameraTouchPolicy.cs`: define a regra testável de qual dedo pode girar a câmera.
- `Assets/Scripts Geral/Controles/Core/MobileTouchZones.cs`: zonas normalizadas compartilhadas para joystick, atirar, recarregar e câmera.
- `Assets/Scripts Geral/Controles/MobileTouchInputBridge.cs`: despacha toques mobile para UI Unity e componentes de joystick.
- `Assets/Scripts Geral/Controles/MovimentoMobile.cs`: aplica movimento do joystick por vetor local limitado.
- `Assets/Scripts Geral/Controles/MobileInputRuntimeProbe.cs`: probe de development build usado para validar Android físico via logcat.
- `Assets/Scripts Geral/Scripts Jogador/MobileAmmoHud.cs`: cria e atualiza a UI de munição abaixo da vida.
- `Assets/Scripts Geral/Scripts Jogador/PlayerHealth.cs`: controla vida do jogador, dano, HUD, recarregamento da cena ao morrer e regeneração de 10 HP/10 s sem dano.
- `Assets/Scripts Geral/Scripts Jogador/HeadshotAudioFeedback.cs`: toca o som configurável de headshot e o fallback original `HEADSHOT`.
- `Assets/Scripts Geral/Scripts Jogador/HeadshotAudioFeedbackRuntimeProbe.cs`: registra no logcat a disponibilidade do clipe de headshot em development builds.
- `Assets/Scripts Geral/Scripts Jogador/ControleCarregamento.cs`: recarrega a arma ativa usando munição reserva.
- `Assets/Personagem/Scripts/Weapon.cs`: controla munição do pente, consumo ao disparar e estado de recarga.
- `Assets/Scripts Geral/ScriptsCorredor/GeradorDeInimigos.cs`: controla tempo de spawn, limite de vivos, posicionamento seguro em NavMesh e pooling.
- `Assets/Scripts Geral/ScriptsCorredor/MonsterFollow.cs`: controla perseguição, recuperação de rota, abertura de portas, dano de ataque, áudio de proximidade, áudio de ataque, áudio de morte e desligamento de som quando o monstro é desativado ou morto.
- `Assets/Scripts Geral/ScriptsCorredor/EnemyHealth.cs`: controla vida do monstro, hitboxes, zonas de dano e notificação de morte.
- `Assets/Scripts Geral/ScriptsCorredor/EnemyHitbox.cs`: roteia tiros para a zona correta de dano do monstro.
- `Assets/Editor/AndroidBuildConsistency.cs`: centraliza as configurações Android compartilhadas entre build por script e build pelo Editor.
- `Assets/Editor/AndroidBuild.cs`: gera APKs versionadas e expõe `Tools/Android/Build APK Igual Ao Script`.
- `Assets/Editor/AndroidBrandingAndTypography.cs`: aplica ícone Android e padroniza textos de Canvas com TextMeshPro.
- A tela inicial deve manter os botões funcionais com a mesma fonte, mesmo tamanho fixo e sem autosizing; não reative o botão duplicado sem ação `Button (Legacy) (1)`.
- A tela final deve manter título de fase concluída, subtítulo atmosférico e botões `Jogar novamente`, `Menu inicial` e `Sair do jogo`.
- `Assets/Tests/EditMode`: testes de contrato para toque mobile, vetores de movimento, regras runtime, munição, recarga, dano inimigo, áudio de monstro e spawn.

## Regras De Trabalho

- Mantenha correções bem focadas. Não reverta mudanças de cena, textura ou configuração sem pedido explícito do usuário.
- Prefira políticas determinísticas de input com testes em vez de checagens improvisadas dentro de loops por frame.
- Não adicione comportamento novo de controle mobile, munição, combate, UI, áudio ou spawn sem documentar em `AGENTS.md` e no arquivo relevante dentro de `docs/`.
- Não mude números de dano inimigo, cadência de spawn ou zonas de toque sem atualizar testes EditMode correspondentes.
- Todos os arquivos Markdown do projeto devem permanecer em português do Brasil.
