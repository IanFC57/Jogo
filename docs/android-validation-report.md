# Relatório De Validação Android

Último pacote validado:

- Aparelho: `RQ8RB09CM6D`, Samsung `SM-G780G`, Android 13.
- APK: `Builds/Android/AsylumHorror-49.apk`.
- Pacote instalado: `com.IanHeitor.AsylumHorror`.
- Versão instalada: `versionCode=49`, `versionName=1.0.49`.
- Horário de atualização no aparelho: `2026-05-20 23:22:13`.

## Comportamentos Validados

- O app abre no aparelho Android físico.
- A APK v49 foi instalada no aparelho físico com `versionCode=49` e `versionName=1.0.49`.
- O toque no menu principal inicia o jogo por `MobileTouchInputBridge`.
- A UI de gameplay carrega com joystick, `Recarregar`, `Atirar`, vida e munição abaixo da vida.
- Testes EditMode da v49 passaram: 90 testes executados, 90 aprovados.
- Build Android v49 foi concluído sem `warning CS`, sem `error CS`, sem referência antiga de Code Coverage e sem falha de build. O log ainda pode mostrar mensagens externas de licença do Unity Hub antes da compilação.
- A validação física Android usou probes runtime via logcat.
- Movimento pelo joystick não alterou yaw, pitch nem roll da câmera.
- O olhar por área livre alterou a câmera.
- Atirar e recarregar ficaram na região inferior direita e não giraram a câmera.
- Toques repetidos de atirar consumiram munição do pente.
- Recarregar restaurou o pente usando munição reserva.
- A HUD em TextMeshPro ficou legível no aparelho: vida e munição no canto superior esquerdo, objetivo no topo e botões menores no canto inferior direito sem sobreposição de texto.
- O ícone Android foi aplicado pelo pipeline `AndroidBrandingAndTypography`.
- O primeiro monstro nasceu e os logs confirmaram áudio ambiente, áudio de ataque e áudio de morte.
- O processo do jogo não registrou crash, `NullReferenceException`, `MissingMethodException` ou erro fatal de Unity no log do app.

## Observações De Log

- Logs de Unity Hub, licença e pacotes podem aparecer no início do Editor e são externos aos scripts do projeto.
- Linhas de inicialização Android/Unity podem aparecer antes do gameplay. Avalie apenas como problema se estiverem associadas ao PID do jogo ou a exceções reais do app.
- Capturas da validação v39: `Screenshots/asylum-v39-ui-branding-fonts-beast-audio.png`, `Screenshots/asylum-v39-after-fire.png` e `Screenshots/asylum-v39-after-reload.png`.
- Logs da validação v39: `Logs/device-v39-ui-branding-fonts-beast-audio.log` e `Logs/device-v39-after-fire-reload.log`.

## Validação De Paridade De Build

- Em `2026-05-20`, o build por script gerou `Builds/Android/AsylumHorror-41.apk` com sucesso.
- O log `Logs/build-v41-editor-build-parity-script.log` confirmou o preflight `AndroidBuildConsistency` antes do empacotamento.
- A checagem por `aapt dump badging` confirmou `package=com.IanHeitor.AsylumHorror`, `versionCode=41`, `versionName=1.0.41`, `sdkVersion=26`, `targetSdkVersion=33` e código nativo `arm64-v8a`/`armeabi-v7a`.
- O log limpo da v41 não registrou `warning CS`, `error CS`, `Fatal Error`, `BuildFailedException`, `BUILD FAILED` nem resíduos antigos de Code Coverage.

## Validação Da Tela Inicial

- Em `2026-05-20`, a APK `Builds/Android/AsylumHorror-42.apk` foi instalada no aparelho `RQ8RB09CM6D`.
- Versão instalada confirmada: `versionCode=42`, `versionName=1.0.42`, `lastUpdateTime=2026-05-20 18:20:38`.
- A captura `Screenshots/asylum-v42-menu-typography.png` confirmou a tela inicial com `Começar jogo` e `Sair` usando o mesmo padrão de fonte, sem o botão duplicado sem ação.
- O log `Logs/device-v42-menu-typography.log` confirmou que o app permaneceu rodando sem `FATAL EXCEPTION`, crash, `NullReferenceException` ou `MissingMethodException` do processo do jogo.

## Validação Da v43

- Em `2026-05-20`, a APK `Builds/Android/AsylumHorror-43.apk` foi gerada com `versionCode=43` e `versionName=1.0.43`.
- A APK v43 foi instalada no aparelho `RQ8RB09CM6D`, com `lastUpdateTime=2026-05-20 20:09:49`.
- A tela de encerramento mantém apenas os botões Unity `Jogar novamente`, `Menu inicial` e `Sair do jogo` sobre a arte de fundo.
- Os botões mobile `Atirar` e `Recarregar` foram aumentados em 50% e continuam na região inferior direita.
- Testes EditMode v43: 81 testes executados, 81 aprovados.
- O build Android v43 foi concluído sem `warning CS`, sem `error CS`, sem falha Gradle e sem resíduos antigos de Code Coverage.

## Validação Da v46

- Em `2026-05-20`, a APK `Builds/Android/AsylumHorror-46.apk` foi gerada com `versionCode=46` e `versionName=1.0.46`.
- A APK v46 foi instalada no aparelho `RQ8RB09CM6D`, Samsung `SM-G780G`, Android 13, com `lastUpdateTime=2026-05-20 22:20:25`.
- O `aapt dump badging` confirmou `package=com.IanHeitor.AsylumHorror`, `sdkVersion=23`, `targetSdkVersion=33` e código nativo `arm64-v8a`/`armeabi-v7a`.
- O build executou o `GameplaySceneCollisionAndDoorTuner` no caminho do script e no preflight do Editor, confirmando que a APK por script e pelo Editor recebem o mesmo tuning.
- Testes EditMode v46: 89 testes executados, 89 aprovados.
- O build Android v46 foi concluído sem `warning CS`, sem `error CS`, sem `BuildFailedException`, sem `BUILD FAILED` e sem `Fatal Error`.
- O logcat confirmou inicialização Unity da versão `1.0.46` no aparelho físico, sem `FATAL EXCEPTION`, `NullReferenceException`, `MissingMethodException` ou `Fatal signal` do processo do jogo.
- O aparelho estava com a tela bloqueada durante a abertura remota; por isso a Activity ficou atrás do `Bouncer`, mas o processo do jogo iniciou com PID ativo e sem crash.

## Validação Da v48

- Em `2026-05-20`, a APK `Builds/Android/AsylumHorror-48.apk` foi gerada com `versionCode=48` e `versionName=1.0.48`.
- A APK v48 foi instalada no aparelho `RQ8RB09CM6D`, Samsung `SM-G780G`, Android 13, com `lastUpdateTime=2026-05-20 22:56:09`.
- O `aapt dump badging` confirmou `package=com.IanHeitor.AsylumHorror`, `sdkVersion=23`, `targetSdkVersion=33` e código nativo `arm64-v8a`/`armeabi-v7a`.
- A tela final agora deixa ativos apenas os botões Unity, em uma fileira horizontal baixa, com fundo transparente seguindo o padrão da tela inicial.
- Folhas e molduras/batentes de portas comuns ficam sem colliders sólidos e são ignorados no bake da NavMesh; a porta final continua bloqueando.
- O tuning removeu 33 colliders sólidos adicionais de molduras comuns, adicionou 34 modificadores de NavMesh e manteve os pontos fixos de spawn com caminho completo até o jogador.
- Testes EditMode v48: 89 testes executados, 89 aprovados.
- O build Android v48 foi concluído sem `warning CS`, sem `error CS`, sem `BuildFailedException`, sem `BUILD FAILED` e sem `Fatal Error`.

## Validação Da v49

- Em `2026-05-20`, a APK `Builds/Android/AsylumHorror-49.apk` foi gerada com `versionCode=49` e `versionName=1.0.49`.
- A APK v49 foi instalada no aparelho `RQ8RB09CM6D`, Samsung `SM-G780G`, Android 13, com `lastUpdateTime=2026-05-20 23:22:13`.
- O `aapt dump badging` confirmou `package=com.IanHeitor.AsylumHorror`, `sdkVersion=23`, `targetSdkVersion=33` e código nativo `arm64-v8a`/`armeabi-v7a`.
- O tuning de portas passou a separar partes abríveis de partes bloqueantes: apenas portas com controlador de abertura e trigger ativo ficam passáveis; portas e molduras sem trigger tiveram colliders/NavMesh restaurados.
- O preflight do build confirmou `doorParts=openable:16/blocking:297`, sem ajustes pendentes de collider ou NavMesh.
- Testes EditMode v49: 90 testes executados, 90 aprovados.
- O build Android v49 foi concluído sem `warning CS`, sem `error CS`, sem `BuildFailedException`, sem `BUILD FAILED` e sem `Fatal Error`.
- O smoke test no aparelho abriu `com.unity3d.player.UnityPlayerGameActivity`; o processo `com.IanHeitor.AsylumHorror` ficou ativo e o logcat não registrou `FATAL EXCEPTION`, `NullReferenceException`, `MissingMethodException` ou `Fatal signal` do processo do jogo.
