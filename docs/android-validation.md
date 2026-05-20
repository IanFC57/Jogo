# Validação Android

Use um aparelho Android físico para a validação final de controles, HUD, áudio e combate.

## Validação De Inimigos No Aparelho

- Um monstro deve nascer imediatamente após a cena de gameplay começar.
- Spawns posteriores devem manter a cadência de 15 segundos enquanto o limite de vivos não for atingido.
- Monstros devem nascer fora da visão atual do jogador e pelo menos 14 metros longe.
- Monstros não podem nascer perto da porta/sala final de fuga.
- Monstros devem atravessar portas normais, mas não tratar a porta final trancada como rota abrível.
- O áudio ambiente do monstro deve soar como rosnado/respiração de fera de terror, não como criatura alienígena, voz humana, música ou efeito cartunesco.
- O áudio ambiente deve ficar mais alto conforme o monstro se aproxima.
- O ataque deve tocar um rosnado curto e distinto do loop de perseguição.
- Quando o monstro morre, o áudio de perseguição/ataque deve parar imediatamente.
- A morte deve tocar um rosnado final curto.

## Validação Do Jogador No Aparelho

- Após receber dano, a HUD de vida deve esperar 10 segundos sem novo dano e então recuperar 10 HP a cada 10 segundos até o máximo.
- Headshots devem tocar o feedback configurado ou o fallback original `HEADSHOT` incluído no projeto.
- Segurar o joystick para frente não pode girar, inclinar ou rolar a câmera.
- Segurar o joystick para os lados não pode girar, inclinar ou rolar a câmera.
- Um segundo dedo em área livre deve girar a câmera enquanto o primeiro dedo continua movendo pelo joystick.
- Atirar e recarregar não podem girar a câmera.
- A munição abaixo da vida deve diminuir ao disparar e aumentar corretamente ao recarregar usando reserva.

## Build

O build Android é centralizado em `AndroidBuildConsistency`. O menu `Tools/Android/Build APK Igual Ao Script` chama o mesmo método usado em linha de comando e gera a APK em `Builds/Android`. O `Build Player` normal do Unity também executa o mesmo preflight antes de empacotar Android, incluindo package id, IL2CPP, arquiteturas, APK em vez de AAB, ícone e tipografia.

Gere a APK com o helper do projeto:

```powershell
$env:ANDROID_VERSION_CODE='<PROXIMO_VERSION_CODE>'
$env:ANDROID_DEVELOPMENT_BUILD='1'
& 'C:\Program Files\Unity\Hub\Editor\2023.2.19f1\Editor\Unity.exe' `
  -batchmode -quit `
  -projectPath 'C:\Users\Leo_s\Documents\Jogo' `
  -executeMethod AndroidBuild.BuildApk `
  -logFile 'C:\Users\Leo_s\Documents\Jogo\Logs\build-<PROXIMO_VERSION_CODE>.log'
```

## Instalação E Confirmação

```powershell
$adb='C:\Program Files\Unity\Hub\Editor\2023.2.19f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe'
& $adb -s RQ8RB09CM6D shell am force-stop com.IanHeitor.AsylumHorror
& $adb -s RQ8RB09CM6D install -r -d 'C:\Users\Leo_s\Documents\Jogo\Builds\Android\AsylumHorror-<PROXIMO_VERSION_CODE>.apk'
& $adb -s RQ8RB09CM6D shell dumpsys package com.IanHeitor.AsylumHorror | Select-String 'versionCode|versionName|lastUpdateTime'
```

## Checklist Manual De Controles

- Segurar o joystick esquerdo para frente: a câmera não deve mover.
- Segurar o joystick esquerdo para trás: a câmera não deve mover.
- Segurar o joystick esquerdo para os lados: a câmera não deve mover.
- Segurar o joystick para frente/trás deve mover o jogador para frente/trás.
- Segurar o joystick para esquerda/direita deve fazer strafe.
- Segurar o joystick e arrastar outro dedo na área de olhar: a câmera deve mover.
- Arrastar uma área livre acima do joystick: a câmera deve mover.
- Pressionar atirar: a câmera não deve mover.
- Pressionar atirar: a munição abaixo da vida deve diminuir.
- Pressionar recarregar: a câmera não deve mover.
- Pressionar recarregar com munição reserva: a munição do pente deve subir e a reserva deve cair.
- Arrastar sobre atirar/recarregar: a câmera não deve mover.
- Aguardar pelo menos 45 segundos na cena de gameplay: monstros devem seguir a cadência de 15 segundos e aparecer fora da visão atual, sem surgir na frente da câmera.
- Matar um monstro e continuar jogando: a contagem de vivos deve liberar uma vaga, permitindo spawn futuro no próximo tick de 15 segundos.
- Validar dano com a pistola de referência: cabeça mata em 1 tiro, peito em 3 tiros e corpo/geral em 6 tiros.
