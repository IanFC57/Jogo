# Relatório De Validação Android

Último pacote validado:

- Aparelho: `RQ8RB09CM6D`, Samsung `SM-G780G`, Android 13.
- APK: `Builds/Android/AsylumHorror-39.apk`.
- Pacote instalado: `com.IanHeitor.AsylumHorror`.
- Versão instalada: `versionCode=39`, `versionName=1.0.39`.
- Horário de atualização no aparelho: `2026-05-20 17:28:45`.

## Comportamentos Validados

- O app abre no aparelho Android físico.
- O toque no menu principal inicia o jogo por `MobileTouchInputBridge`.
- A UI de gameplay carrega com joystick, `Recarregar`, `Atirar`, vida e munição abaixo da vida.
- Testes EditMode passaram: 76 testes executados, 76 aprovados.
- Build Android v39 foi concluído sem `warning CS`, sem `error CS`, sem referência antiga de Code Coverage e sem falha de build. O log ainda pode mostrar mensagens externas de licença do Unity Hub antes da compilação.
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
