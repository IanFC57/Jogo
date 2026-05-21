# Colisao, Portas E Objetos Do Cenario

Este arquivo define o contrato de gameplay para colisao do cenario, portas, objetos empurraveis e aparencia visual das portas. Mantenha-o sincronizado com `AGENTS.md`, `docs/project-context.md`, `Assets/Scripts Geral/Controles/Core/GameplayCollisionTuningRules.cs` e `Assets/Editor/GameplaySceneCollisionAndDoorTuner.cs`.

## Objetivo

O jogador deve conseguir atravessar o mapa sem ficar preso em excesso nos obstaculos, especialmente em celular. O cenario ainda precisa parecer fisico e perigoso: objetos pequenos nao podem travar o movimento, portas realmente abríveis nao podem bloquear o fluxo de jogador ou inimigos, e portas falsas/sem trigger devem continuar funcionando como parede ou limite de sala.

## Regras De Colisao

- Paredes, chao, teto, escadas e a porta final de conclusao continuam solidos.
- Apenas folhas e molduras/batentes associados a uma porta com controlador de abertura e trigger ativo ficam sem collider solido para permitir passagem fluida de jogador e inimigos.
- Portas e molduras/batentes sem trigger de abertura nao podem ficar sem collider solido e devem participar do bake da NavMesh.
- Sensores, triggers e scripts de porta continuam funcionando quando existirem; a regra remove apenas colisao solida das partes que pertencem a uma porta abrível.
- Eixos de animacao de porta (`eixoDaPorta`, `eixoEsquerdo`, `eixoDireito` ou equivalente) devem apontar para a folha/eixo da porta, nunca para moldura, batente, sensor ou trigger.
- A porta/sala final continua bloqueando fisicamente e continua fora das areas validas de spawn inimigo.
- Cadeiras, bancos, caixas, macas pequenas e objetos similares viram objetos empurraveis com `Rigidbody`, `BoxCollider` simples e material fisico de baixo atrito.
- Canecas, pratos, papeis, posters, luminarias pequenas, dutos finos e decoracoes leves nao devem bloquear o jogador.
- Objetos empurraveis e decoracoes liberadas devem ser ignorados no bake da NavMesh para nao criar buracos ou caminhos quebrados.

## Regras Visuais Das Portas

- Portas e materiais com nome de porta devem ser foscos, escuros o suficiente e sem emissao.
- Renderers de portas comuns nao devem usar reflection probe, porque isso fazia a porta parecer iluminada quando o jogador olhava para ela.
- Reflection probes da cena de gameplay devem ficar com intensidade maxima `0.8`.
- Frames/batentes de portas abríveis continuam visiveis e podem ser passaveis. Frames/batentes de portas sem trigger continuam solidos e entram no bake da NavMesh.

## Ferramentas

- `Tools/Gameplay/Apply Collision And Door Tuning` aplica o tuning de colisao, fisica e aparencia no prefab `Asylum` e na cena `JogoComMenu`.
- `Tools/Gameplay/Apply Collision Tuning And Rebuild NavMesh` aplica o tuning e depois reconstrui/valida a NavMesh pelo diagnostico de inimigos.
- O build Android por script e o build pelo Editor executam o mesmo tuning durante o preflight, garantindo que a APK gerada por qualquer caminho use as mesmas regras.

## Testes Obrigatorios

Depois de alterar prefab de cenario, cena de gameplay, materiais de porta, ferramentas de build ou NavMesh, rode a suite EditMode.

A cobertura atual valida:

- portas e molduras sem trigger mantem colliders solidos quando existirem e continuam no bake da NavMesh;
- folhas e molduras/batentes de portas com trigger de abertura nao mantem colliders solidos ativos;
- eixos de abertura de portas nunca apontam para molduras, batentes, sensores ou triggers;
- decoracoes pequenas nao mantem colliders solidos ativos;
- objetos empurraveis usam `Rigidbody`, `BoxCollider`, gravidade, massa adequada e nao ficam estaticos;
- objetos empurraveis e decoracoes liberadas sao ignorados no bake da NavMesh;
- materiais de porta ficam foscos, nao emissivos e sem brilho excessivo;
- portas abríveis da cena nao bloqueiam passagem nem usam reflection probe;
- portas sem trigger de abertura continuam bloqueando;
- porta final continua bloqueando fisicamente;
- reflection probes ficam dentro do limite de intensidade.
