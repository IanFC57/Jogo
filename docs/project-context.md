# Contexto Do Projeto

O projeto é um jogo de terror Unity para Android. A prioridade atual é jogabilidade confiável em celulares Android reais, com input mobile previsível, HUD legível, combate funcional e boa performance.

## Sistemas Runtime Importantes

- Movimento do jogador: joystick mobile esquerdo por `MovimentoMobile`.
- Rotação da câmera: arrasto direto na tela por `CameraMobile`.
- Despacho de toques: `MobileTouchInputBridge`.
- Configuração runtime Android: `MobileTouchBootstrap` e `AndroidRuntimePerformance`.
- Armas e munição: `Assets/Personagem/Scripts/Weapon.cs`, `ControleCarregamento`, `InventarioJogador` e `MobileAmmoHud`.
- Combate inimigo: `GeradorDeInimigos`, `EnemyHealth`, `EnemyHitbox` e regras puras em `Assets/Scripts Geral/Controles/Core`.
- Colisão de cenário e portas: `GameplayCollisionTuningRules`, `GameplaySceneCollisionAndDoorTuner`, prefab `Asylum` e cena `JogoComMenu`.
- Identidade visual e tipografia Android: `Assets/Editor/AndroidBrandingAndTypography.cs`, `Assets/AppIcon/AsylumHorrorIcon.png` e TextMeshPro nas cenas de build.

## Direção Mobile Atual

- Prefira áreas explícitas de toque e posse de dedo em vez da simulação padrão de mouse da Unity.
- Mantenha `Input.multiTouchEnabled = true`.
- No Android, mantenha `Input.simulateMouseWithTouches = false`.
- O olhar da câmera mobile é permitido em qualquer região livre da tela fora do joystick e dos botões.
- A sensibilidade de toque da câmera mobile é `MobileCameraRules.DefaultTouchSensitivity` (`0.144`), cerca de 20% acima do valor antigo `0.12`. A constante padrão deve permanecer sincronizada com a serialização da cena de jogo.
- A HUD de munição é criada abaixo da vida e deve atualizar após disparo, recarga, troca de arma e coleta de munição.
- A vida do jogador regenera 10 HP a cada 10 segundos enquanto o jogador está vivo, abaixo da vida máxima e sem ter recebido dano nos últimos 10 segundos.
- Monstros são ajustados para o dano de referência da pistola: 1 headshot, 3 tiros no peito ou 6 tiros no corpo/geral. O primeiro monstro nasce imediatamente; depois o spawn roda a cada 15 segundos enquanto estiver abaixo do limite de vivos.
- Headshots tocam o feedback de headshot do projeto. Não inclua áudio de locutor de terceiros protegido por direitos autorais; use clipe licenciado ou o fallback original `HEADSHOT` incluído.
- Monstros usam áudio de proximidade com timbre de fera orgânica: o loop de perseguição aumenta de volume com a proximidade, o ataque toca um rosnado curto separado e a morte para todos os sons locais antes de tocar um rosnado final.
- O spawn inimigo usa apenas candidatos validados na NavMesh: caminho completo até o jogador, pelo menos 14 metros de distância, fora da visão atual da câmera com margem de segurança e sem sobreposição com a área final bloqueada.
- `EnemySpawnExclusionZone` marca áreas onde monstros nunca podem nascer. A porta/sala final de fuga deve permanecer excluída porque inimigos não podem abrir essa porta de conclusão.
- Portas com controlador de abertura e trigger ativo, incluindo suas folhas e molduras/batentes próximos, não devem bloquear o fluxo do jogador ou dos inimigos. Portas e molduras sem trigger de abertura continuam sólidas e entram no bake da NavMesh. A porta final continua sólida. Objetos grandes de cena podem ser empurrados, decoração pequena não deve prender o jogador e portas não devem brilhar por reflection probe.
- Não valide comportamento mobile final apenas em emulador quando houver celular físico conectado.

## Notas De Build

- `Assets/Editor/AndroidBuildConsistency.cs` centraliza package id, IL2CPP, stripping, arquiteturas ARMv7/ARM64, APK em vez de AAB, orientação landscape, ícone e tipografia.
- `Assets/Editor/AndroidBuild.cs` gera APKs versionadas em `Builds/Android` e também aparece no menu `Tools/Android/Build APK Igual Ao Script`.
- O `Build Player` normal do Unity recebe o mesmo preflight Android antes de empacotar. Assim, quando a pessoa gera APK pelo Editor, as configurações críticas ficam alinhadas ao script.
- O preflight Android também aplica o tuning de colisão/portas antes de gerar a APK, separando portas abríveis de portas bloqueantes; então builds pelo Editor e pelo script saem com o mesmo estado de cena.
- O aparelho usado nos testes recentes é `RQ8RB09CM6D`.
- Um resultado limpo significa ausência de erros e avisos C# no log atual de build/teste. Ruídos de licença, pacote ou inicialização da Unity devem ser analisados separadamente antes de alterar dependências.
