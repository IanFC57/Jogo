# Identidade Visual, Tipografia E Áudio

Este documento registra o padrão atual de apresentação do jogo no Android.

## Ícone Android

- O ícone principal fica em `Assets/AppIcon/AsylumHorrorIcon.png`.
- O ícone deve comunicar terror hospitalar/asilo abandonado, com leitura clara em tamanhos pequenos.
- Não use texto dentro do ícone Android. Texto fica ilegível em launchers e não escala bem.
- A aplicação do ícone é feita por `Assets/Editor/AndroidBrandingAndTypography.cs`, que configura ícones Android legados, redondos e adaptativos.
- Depois de alterar o ícone, rode o método de Editor `AndroidBrandingAndTypography.Apply` ou gere a APK por `Tools/Android/Build APK Igual Ao Script`; o preflight Android também aplica o ícone antes de builds pelo `Build Player`.

## Fontes No Canvas

- As cenas de build devem usar `TextMeshProUGUI` em vez de `UnityEngine.UI.Text`.
- A fonte padrão atual é `Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset`.
- Textos de botões devem usar auto size e peso em negrito para manter legibilidade em celulares.
- Na tela inicial, os botões funcionais são exceção: devem usar o mesmo tamanho fixo de fonte, sem autosizing, para que `Sair` não fique maior que `Começar jogo`.
- O botão duplicado sem ação `Button (Legacy) (1)` deve permanecer inativo para não aparecer em telas Android largas.
- A tela de encerramento deve comunicar conclusão de fase com título curto, subtítulo atmosférico e três ações claras: `Jogar novamente`, `Menu inicial` e `Sair do jogo`.
- Textos de HUD, como vida, aviso e munição, devem manter `raycastTarget = false` para não bloquear input mobile.
- Ao adicionar novos textos, prefira TextMeshPro desde o início e valide em tela pequena.

## Áudio De Monstros

- Os sons de monstro devem parecer de uma fera orgânica de terror, não de criatura alienígena ou de outra dimensão.
- Evite timbres sci-fi, vocoder, tons metálicos, flanger forte, formantes sintéticos, palavras humanas, música e chiado de fundo.
- `MonsterAmbientGrowl.wav` deve ser um loop de perseguição com respiração pesada, irregular e rosnados guturais secos.
- `MonsterAttackGrowl.wav` deve ser curto, agressivo e imediato, como mordida ou golpe.
- `MonsterDeathGrowl.wav` deve ser uma queda curta com rosnado final e expiração áspera.
- Ao morrer, o inimigo deve parar o loop de perseguição e qualquer som de ataque antes de tocar o one-shot de morte.
