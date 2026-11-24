# Nutri Runner — versão final

Alex sai do hospital com o colesterol e a glicemia nos limites. Em três etapas ele
reconstrói o corpo: **come direito**, **ganha condicionamento** e **compete**. No fim,
ergue a taça.

Terceira pessoa, mundo aberto, **WASD** + **mouse**. Construído para rodar em notebook
sem GPU dedicada: sombreamento por vértice, zero sombra em tempo real, zero arquivo de
mídia (áudio todo gerado por código).

---

## Como montar (~5 minutos)

1. **Projeto novo** no Unity Hub: template **3D (Built-In Render Pipeline)**.
   Unity 2021 LTS ou mais novo. Evite URP/HDRP.
2. Se já existe um `Assets/NutriRunner` de uma versão anterior, **apague as subpastas
   `Scripts` e `Editor`** antes de colar as novas. Sobrescrever deixa scripts velhos
   para trás, e um único arquivo desatualizado derruba a compilação — e com ela o menu
   `Tools`. **Não apague `Modelos`**: é onde estão os seus `.fbx` e as texturas.
3. Arraste a pasta **`NutriRunner`** para dentro de `Assets/`, deixando o Windows
   mesclar. **Os modelos podem ficar em qualquer pasta dentro de `Assets/`** — o
   construtor procura por nome no projeto inteiro. O melhor lugar é `Assets/Modelos`,
   FORA de `NutriRunner`, para apagar essa pasta nunca mais levar seus `.fbx` junto. A subpasta `Editor` precisa manter esse nome (é o que mantém o construtor
   fora da build).
4. Coloque os `.fbx` em `Assets/NutriRunner/Modelos/` (lista abaixo). Os dois arquivos
   `pista_atletismo.obj` e `pista_atletismo.mtl` já vêm no pacote e vão para essa
   mesma pasta — **os dois juntos**, porque é o `.mtl` que carrega as cores.
5. Espere compilar — Console sem erros vermelhos.
6. Menu do Unity: **Tools → Nutri Runner → Construir Cena Demo**.
   Existe também **(modo leve, sem luz)** no mesmo menu, para máquina mais fraca, e
   **Aferir Matemática da Pista**, que roda 28 testes da geometria no Console.
7. **Ctrl+S** e **Play**.

> Os scripts funcionam com **qualquer** configuração de `Active Input Handling`
> (antigo, novo ou ambos) — `Entrada.cs` resolve isso sozinho.
> Se algo não responder, rode **Tools → Nutri Runner → Verificar Cena** e olhe o Console:
> ele lista o que falta, quantos personagens têm esqueleto e se a taça está no lugar.

### Controles
| Tecla | Ação |
|---|---|
| **W A S D** | andar (direção relativa à câmera) |
| **Mouse** | girar a câmera em volta do Alex |
| **Scroll** | aproximar / afastar |
| **Shift** | correr |
| **Espaço** | pular, e **avançar o diálogo** (clique esquerdo também serve) |
| **ESC** | soltar o cursor — clique para travar de novo |
| **R** | reiniciar depois do fim |

---

## As três etapas

### Etapa 1 — A dieta
Começa no **hospital**, a oeste. O **Dr. Moreira** e a **Dra. Helena** conversam com o
Alex — as vozes são sílabas sintetizadas, no espírito do aldeão do Minecraft: você entende
o tom, não as palavras. Dali ele sai para juntar **10 alimentos saudáveis** espalhados
pela cidade.

Só existe comida saudável no mapa. O vilão é a **nuvem de cheiro** que sai da pizzaria e
persegue o Alex **atravessando paredes** — não há esconderijo, só distância.

**A engrenagem central:** cada alimento deixa o Alex mais leve, e mais leve é mais rápido.
Só que a nuvem lê o peso dele — **quanto mais leve você fica, mais rápido o cheiro corre.**

O que aperta conforme você avança:

- **O cheiro se multiplica.** A cada 3 alimentos a pizzaria solta mais uma nuvem, até 3
  extras. No fim são quatro perseguidores vindo de ângulos diferentes, e correr reto deixa
  de resolver — você precisa ler o mapa.
- **As nuvens antecipam.** Elas miram onde você *vai estar*, cortando a curva. Isso acaba
  com a manobra de dar a volta por cima delas.
- **Teto de segurança.** A nuvem principal nunca passa de 85% da sua velocidade de corrida;
  as extras chegam a 95%. Fugir de *uma* é sempre possível. De três ao mesmo tempo, não.

**Dano e recuperação:** encostar numa nuvem sobe colesterol e glicemia, **e engorda o
Alex** (1,1 kg por segundo de contato) — tematicamente é ele cedendo ao cheiro. **Correr
limpo abaixa colesterol e glicemia** — é assim que se recupera de um susto. Ficar parado
não recupera nada. Derrota: colesterol ou glicemia no máximo.

Engordar tem duas consequências que se cancelam de propósito: deixa a nuvem **mais lenta**
(a velocidade dela segue a leveza do Alex), o que evita a espiral de morte, mas deixa o
Alex **sem fôlego na prova final** — veja a etapa 3. Fugir mal aqui custa o pódio lá.

### Etapa 2 — A corrida de rua
Coletados os 10, o marcador aponta a **LARGADA**, a nordeste. Lá o **treinador** manda o
Alex correr pela cidade para ganhar condicionamento. Cruzar a faixa começa a prova: as
nuvens se dispersam e o adversário passa a ser o cronômetro. **6 auras amarelas** em
posições fixas, uma ativa por vez; atravessar cada uma dá **+7 s** sobre os 75 s iniciais.

A aura é uma coluna de luz de 9 m de altura e 2,6 m de raio — alta o bastante para ser
vista por cima dos prédios, larga o bastante para atravessar correndo sem ter que mirar.
São três cilindros transparentes encaixados mais um anel no chão: quatro malhas, nenhuma
partícula e nenhuma luz extra. Ela respira (engorda e estica em ciclos de 0,35 Hz), e cada
aura tem uma fase própria para duas visíveis ao mesmo tempo não pulsarem em sincronia — em
sincronia o olho lê as duas como uma animação só, em vez de dois alvos.

**Voce nao corre sozinho.** O treinador larga junto e vai na frente, como pacer. Se
voce ficar mais de 26 m atras, ele para e espera — e cobra. Nao da para vence-lo nem
perde-lo: a pressao e social, nao mais um numero descendo. (A histerese de 26 m para
parar e 16 m para retomar existe para ele nao piscar entre andar e parar quando voce
oscila em volta da distancia.)

**E tem uma decisao a cada trecho.** Alem das 6 auras amarelas obrigatorias, ha **5 auras
azuis** plantadas fora da linha mais curta. Cada uma custa uns 3 segundos de desvio e da
**condicionamento**, que vira folego e velocidade na prova final. O relogio deixa de ser
so ameaca e vira moeda: voce gasta tempo para comprar preparo.

**O que liga as duas etapas:** o peso. Quem coletou bem chega leve e corre a ~8 m/s; quem
raspou chega pesado e corre a ~4 m/s, com o mesmo tempo de prova. Não existe bônus
escondido premiando a etapa 1 — a recompensa é a própria velocidade.

### Etapa 3 — O campeonato
O **estádio de atletismo** fica ao sul. Antes da largada, o treinador e o nutricionista
comentam que chegou o momento pelo qual ele batalhou. Então são **2 voltas** (400 m)
contra **4 rivais** (Bruno, Carla, Diego e Elisa, em ritmos de 6,4 a 7,9 m/s), cada um
na sua raia. Eles fazem os 400 m em 63, 58, 54 e 51 s.

A pista é a de verdade: **cápsula**, não elipse — duas retas paralelas fechadas por dois
semicírculos, que é a forma de uma pista de 400 m. Reduzida pela metade para caber na
cidade, a volta dá **200 m exatos**, com 8 raias de 0,61 m entre o meio-fio (18,25 m do
eixo) e a borda externa (23,13 m).

**O juiz de pista.** O progresso é medido em **metros de arco** ao longo do circuito, e
quem decide se o Alex está na pista é a distância dele até o eixo da reta central: tem que
estar entre o raio interno e o externo. Fora do asfalto, **o progresso congela**, um aviso
âmbar pisca na tela e, ao voltar, a contagem é religada no ponto de reentrada — o trecho
andado por fora simplesmente não existe para a prova. Quem insiste é **desclassificado**
depois de 5 s acumulados fora. Correr no sentido errado também dispara aviso.

**O condicionamento, que vem da etapa 2.** As auras azuis coletadas (peso 0,65) e o tempo
que sobrou no relogio (peso 0,35) formam um numero de 0 a 1 que da ate **+18 s de folego**
e **+8% de velocidade** na prova. As duas coisas: so folego nao bastava, porque quem fechou
a dieta perfeita ja tinha folego de sobra para os 400 m e o treino nao mudava nada
justamente para quem jogou bem.

**O fôlego, e é aqui que a dieta cobra a conta.** Nas duas primeiras etapas correr é
livre. Na prova, o Alex tem fôlego — segundos de sprint contínuo — e **quanto ele tem
depende do peso**: 60 s no peso ideal, 22 s sem ter feito a dieta. A prova de 400 m leva
uns 51 s correndo, então quem chegou leve sustenta o sprint até a fita e quem chegou pesado
tem que alternar sprint e trote. Zerado, o Alex cai para a caminhada até recuperar um terço
(a histerese existe para ele não piscar entre correr e andar a cada frame).

O efeito no resultado, simulado contra os ritmos dos quatro rivais:

| Itens coletados | Fôlego | Tempo da prova | Colocação |
|---|---|---|---|
| 0 a 4 | 22–35 s | 70–93 s | 5º |
| 5 | 39 s | 67 s | 4º |
| 6 | 42 s | 63 s | 3º |
| 7 a 8 | 45–49 s | 58–60 s | 2º |
| 9 a 10 | 52–55 s | 52–55 s | **1º ou 2º, conforme o treino** |

Era isso que faltava: antes, as três etapas eram uma sequência: o peso decidia velocidade
na etapa 1 e na 2, e não valia nada na 3. Agora a dieta decide o lugar no pódio.

Por que o anti-trapaça importa: a versão anterior media **ângulo** em volta do centro, e ângulo tem
um furo. Neste traçado, passar reto pelo miolo de uma reta à outra renderia **100 m de
crédito por 37 m andados** — meia volta de graça. O que segurava isso eram as muretas
físicas. Agora não há mureta nenhuma: você *pode* cortar pelo gramado, só não ganha nada
com isso, e perde o tempo do desvio.

### O final
Ao vencer, o Alex **sobe ao pódio e ergue a taça**, em três tempos: o controle é desligado
e a pose de comemorar (`Cheering`) acende já no começo, enquanto ele desliza 0,7 s até o
degrau do ouro — com o corpo em festa, o deslize lê como corte de cena, e não como estátua
escorregando. Chegando lá, meio segundo de espera. Então a taça aparece **pendurada no osso
da mão direita**, que o Avatar Humanoid entrega pronto, de modo que ela acompanha a animação
sem nenhum encaixe manual.

A câmera baixa e passa a orbitar sozinha a 18°/s, e o relatório final espera 4,5 s para não
cobrir a cena. O relatório fecha o arco mostrando o fôlego que aquele peso rendeu.

---

## Onde fica cada coisa no código

| Arquivo | O que tem dentro |
|---|---|
| `Editor/DemoSceneBuilder.cs` | Menus, constantes do mapa, `Construir()` e `Verificar()` |
| `Editor/Builder.Paleta.cs` | Materiais, texturas, busca de modelos, ajudantes de malha |
| `Editor/Builder.Cidade.cs` | Terreno, avenidas, praça, prédios, hospital, pizzaria, largada |
| `Editor/Builder.Estadio.cs` | Pista, pódio, limpeza do terreno, auras das duas provas |
| `Editor/Builder.Personagens.cs` | Avatar, Animators, Alex, NPCs, rivais, taça |
| `Editor/AferirPista.cs` | Os 28 testes da geometria da pista |
| `Scripts/` | 25 componentes de runtime |

Os cinco arquivos `Builder.*` são **`partial class` do mesmo tipo**: nada mudou de escopo
na separação e cada parte enxerga as outras como quando era um arquivo só. A divisão é
para **achar as coisas**, não para isolar — um erro de compilação em qualquer um deles
ainda derruba o menu `Tools` inteiro, porque é tudo o mesmo assembly.

## Modelos 3D

O jogo funciona **sem nenhum asset externo** — sem modelo, ele monta tudo com primitivas.
Com os modelos, fica como na apresentação. Coloque os `.fbx` em
`Assets/NutriRunner/Modelos/`:

| Grupo | Arquivos |
|---|---|
| **Pista** | `pista_atletismo.obj` + `pista_atletismo.mtl` (**já vêm no pacote**) |
| **Personagem** | `Mannequin_Medium.fbx` |
| **Animações** | `Rig_Medium_General.fbx`, `Rig_Medium_MovementBasic.fbx`, **`Rig_Medium_Simulation.fbx`** |
| **Alimentos** | `Apple.fbx`, `Bottle1.fbx`, `ChickenLeg.fbx` |
| **Prédios** | `building_A.fbx` … `building_H.fbx`, `base.fbx`, `bush.fbx` |
| **Ruas e mobiliário** | `road_straight.fbx`, `streetlight.fbx`, `street_lantern.fbx`, `trafficlight_B.fbx`, `firehydrant.fbx`, `trash_A.fbx`, `trash_B.fbx`, `dumpster.fbx`, `watertower.fbx` |
| **Parque** | `bench.fbx`, `tree.fbx`, `tree_large.fbx`, `flower_A.fbx`, `hedge_corner.fbx`, `bird.fbx` |

**`Rig_Medium_Simulation.fbx` é novo nesta versão.** É dele que vêm os clipes `Cheering`
(Alex erguendo a taça) e `Waving` (os NPCs acenando). Sem ele o jogo roda, mas o final
fica sem a pose — o Console avisa, e o `Verificar Cena` também.

Copie junto a pasta **`Textures`** do pack (com o `colormap.png`) para dentro de
`Modelos/`. Sem ela os prédios ficam brancos; o Console avisa se faltar. Ao reconstruir,
o Console imprime `Modelo aplicado: Apple` para cada um que encontrou.

### A pista, e por que ela é `.obj` e não `.glb`
A Unity **não lê `.glb`** sem instalar um pacote (glTFast ou UnityGLTF); `.obj` ela lê
nativamente. Então o modelo foi convertido, e três coisas foram ajustadas na conversão:

1. **Vértices deduplicados** — 16.068 repetidos viraram 5.113, para 5.356 triângulos.
2. **Winding conferido** — as malhas são planos de espessura zero, e um plano com a
   normal apontando para baixo fica invisível visto de cima. Todos já vinham corretos.
3. **Grupos preservados** — `gramado`, `meio_fio`, `pista`, `area_externa`,
   `linhas_brancas`, `chegada_preto` viram submeshes, e o construtor repinta cada um com
   a paleta do projeto casando pelo nome. Se o `.mtl` não importar e tudo virar
   `Default-Material`, ele cai no nome do grupo — o resultado é o mesmo.

O modelo é escalado **só em X e Z** (0,5), nunca em Y. As camadas estão empilhadas com
1 cm entre elas (asfalto 0, linhas 1 cm, meio-fio 5 cm); reduzir o Y junto deixaria 5 mm
entre elas e daria briga de profundidade na tela. A pista inteira ainda sobe 3 cm, porque
o chão da cidade tem o topo exatamente em y = 0.

A escala é **medida em tempo de construção** (`ARQ_COMPRIMENTO / bounds.x`), não fixa, o
que deixa o resultado igual com qualquer configuração de importação.

**Se o `.obj` não estiver na pasta, o jogo não quebra:** o construtor desenha a mesma
cápsula com primitivas, com meio-fio e as 8 raias. Muda o acabamento, não a geometria —
quem manda nas regras é o componente `PistaAtletismo`, nunca o visual.

### Como o personagem é montado
O construtor configura o importador dos quatro FBX como rig **Humanoid**, gera um único
**Avatar** a partir do `Mannequin_Medium` e manda os rigs de animação usarem esse mesmo
avatar como `sourceAvatar`. Daí cria dois controladores por código:

- **`Alex.controller`** — blend tree de locomoção por velocidade + pulo + `Comemorando`.
- **`Npc.controller`** — blend tree de locomoção + `Acenando` + `Comemorando`.

**Todos os NPCs usam o mesmo molde do Alex.** `MontarHumanoide()` instancia o mesmo
`Mannequin_Medium`, mede o modelo com `Renderer.bounds` e reescala para a altura alvo
(1,74 m nos NPCs, 1,72 m no Alex — a do collider), e pinta cada um com um material de
roupa próprio. Médicos, treinador e nutricionista ficam parados ou acenando; os quatro
rivais da pista recebem a velocidade real deles no blend tree, então as pernas batem com
os metros por segundo que cada um corre de verdade.

Medir o modelo em tempo de construção é de propósito: deixa o código imune à escala de
importação do FBX. Se faltar o modelo, cada personagem volta para o boneco de primitivas
com o `AnimadorSimples`, sem quebrar nada.

Se o Unity não conseguir gerar o Avatar Humanoid sozinho, o Console avisa — aí é abrir o
FBX, aba **Rig**, escolher **Humanoid** e reconstruir.

### Por que os materiais do FBX são ignorados
Os modelos vêm do Blender com cores em espaço linear, que saem escuras no Unity, e a
`Bottle1` original é uma garrafa verde de cerveja. O construtor repinta cada submesh com a
paleta do projeto, casando pelo **nome do material original** (`DarkRed`, `BottleCap`,
`Beige`…) — assim funciona mesmo que a ordem dos submeshes mude.

Escalas aplicadas, medidas direto dos arquivos:

| Modelo | Tamanho original | Escala | Ajuste |
|---|---|---|---|
| Apple | 0,80 × 0,85 × 0,80 m | 1,00 | — |
| Bottle1 | 0,45 × 1,56 × 0,45 m | 0,55 | pivô na base, desce 0,43 m |
| ChickenLeg | 1,40 × 0,58 × 0,58 m | 0,62 | inclinada 25° |

---

## O mapa

Cidade de **190 × 190 m**, com o miolo aberto de propósito: toda a massa construída fica
na borda, porque prédio no meio esconde alimento e cega o jogador durante a fuga.

- **Praça circular no centro** (raio 24 m): calçadão, anel de gramado, quatro caminhos
  radiais e uma fonte. Em volta, canteiros, árvores, bancos e postes.
- **Avenida em anel** (raio 30 m) contornando a praça, com faixa tracejada acompanhando a
  curva e faixa de pedestre nas quatro saídas. Mais quatro avenidas radiais.
- **Hospital** a oeste, em (−40, 0). Onde a história começa.
- **Pizzaria** a noroeste, em (−38, 38). Onde nasce o cheiro.
- **Largada** a nordeste, em (38, 38). Ponto de encontro com o treinador.
- **Estádio de atletismo** ao sul, centro em (0, −67). Ocupa 91,5 × 49,3 m de asfalto
  (102 × 54 m com a área cinza em volta): reta de 42,2 m, meio-fio a 18,25 m do eixo,
  borda externa a 23,13 m. Pórtico de entrada e arquibancada ao norte, de frente para a
  reta de chegada, e o **pódio** de três degraus (ouro, prata, bronze) no gramado interno.
  Folga de 3,4 m até o muro sul do mapa e 7,4 m até a avenida em anel.
- **Três fileiras de prédios** na borda, em raios de ~58, ~72 e ~86 m.
- **Campo aberto** entre a praça e a borda: só mobiliário baixo, nada acima da cintura,
  para que qualquer alimento seja visível de longe.

As 6 auras da corrida de rua ficam em norte (0, 40), leste (40, 8), sudeste (30, −30),
sudoeste (−30, −30), oeste (−40, −20) e a chegada de volta na largada. O construtor roda
um `Aferir()` que avisa no Console se alguma cair dentro do hospital, da pizzaria ou do
estádio — foi assim que os três encavalamentos da versão anterior apareceram.

**A reserva do estádio vale para tudo, não só para prédios.** O teste `Reservado(p, raio)`
recebe metade do tamanho da peça, porque olhar só o centro deixa passar o que invade pela
borda: um prédio de 14 m centrado a 6 m da reserva ainda entra 1 m nela. Arbustos, lixeiras,
hidrantes e caixas d'água também consultam esse teste — antes não consultavam, e era por
isso que apareciam uns sete deles plantados no meio da pista.

---

## Onde ajustar o balanceamento

Tudo no Inspector, sem recompilar.

**Objeto `NuvemDeCheiro` → `NuvemCheiro`** — *é aqui que se ajusta a dificuldade do jogo
inteiro:*
- `velocidadeInicial` (3,0) — velocidade com o Alex pesado.
- `velocidadeFinal` (5,9) — com ele no peso ideal. Se ficar impossível na apresentação,
  baixe para 5,5.
- `antecipacao` (0,55) — o quanto ela mira na frente do jogador.
- `raioDano` (2,8) — o quão perto precisa chegar para machucar.
- `aceleracaoPorTempo` (0) — deixe em 0 para a velocidade depender só do peso.

**Objeto `Sistemas` → `GameManager`:**
- `pesoInicial` (78) e `pesoMinimo` (30) — a faixa que a nuvem usa para calcular velocidade.
- `danoColesterol` (9) e `danoDiabetes` (7,5) — dano por segundo dentro da nuvem.
- `queimaCorrendo` (3,4) — o quanto correr limpo recupera.
- `itensPorNuvemExtra` (3) e `maximoNuvensExtras` (3) — o ritmo em que o cheiro se multiplica.

**Objeto `ItemSpawner`:** `quantidadeBons` (10) define o tamanho da etapa 1. 10 dá uns 2 min.

**Objeto `Corrida`:** `tempoLimite` (75) e `bonusPorCheckpoint` (7).

**Objeto `Sistemas` → `GameManager`, condicionamento:** `bonusMaximoDeFolego` (18 s),
`ganhoDeVelocidade` (0,08) e `pesoDasAuras` (0,65 — quanto as azuis pesam contra o tempo
restante). Zere os dois primeiros para desligar a ligacao entre a etapa 2 e a etapa 3.

**Objeto `Sistemas` → `Dialogo`:** as falas dos três diálogos estão em arrays no
Inspector, já preenchidas com o texto padrão. Reescreva qualquer linha ali — inclusive
com o jogo rodando — sem recompilar. Array vazio volta ao texto escrito em código.

**Objeto `TreinadorPacer`:** `velocidade` (5,6 m/s), `distanciaParaEsperar` (26 m),
`distanciaParaRetomar` (16 m) e `intervaloDeFala` (6 s).

**Objetos `Portico1..6` → `Checkpoint`:** `respiracao` (0,09), `pulsoDeAltura` (0,05) e
`ritmo` (0,35 Hz) controlam a pulsação da aura. O tamanho dela são as constantes
`AURA_RAIO` (2,6 m) e `AURA_ALTURA` (9 m) no topo do `DemoSceneBuilder.cs`, e a cor são os
materiais `mAura` e `mAuraForte`, logo acima de `mCheiro`.

**Objeto `PistaAtletismo` → `CircuitoOlimpico`:** `voltas` (2), `contagem` (3 s) e o juiz:
- `toleranciaFora` (5 s) — tempo acumulado fora do asfalto antes de desclassificar.
  Suba para 8 se achar duro demais, ou baixe para 2 se quiser rigor de prova oficial.
- `recuperaTolerancia` (0,4) — com que rapidez esse tempo se recupera correndo direito.
- `intervaloAviso` (1,2 s) — frequência do aviso de fora de pista.

**Objeto `PistaAtletismo` → `PistaAtletismo`** (a geometria; mexer aqui muda as regras,
não o desenho): `reta`, `raioInterno`, `raioExterno`, `raioMedida` e `folga` (0,4 m de
tolerância para cada lado, para quem pisa na linha não ser punido). Selecione o objeto na
cena e os três contornos aparecem desenhados na viewport — ciano o asfalto, amarelo a
raia de medição, vermelho o eixo das retas.

**Objeto `Sistemas` → `GameManager`, fôlego da prova:** `folegoLeve` (60 s) e
`folegoPesado` (22 s) são os extremos; `recuperaLeve` (11 s) e `recuperaPesado` (22 s), a
recuperação; `folegoParaRetomar` (0,33) a histerese. E `ganhoDePesoNoCheiro` (1,1 kg/s) é o
que liga a etapa 1 à etapa 3 — zere para desligar essa ligação.

**Objeto `Alex` → `ThirdPersonController`:** `velCaminhada`, `velCorrida`, `forcaPulo`.
→ `AnimadorPersonagem`: `passada` (1,75 m entre pisadas) e `suavidade`.
→ `Comemoracao`: `deslocamento`, `rotacao` e `escala` ajustam o encaixe da taça na mão,
`esperaAntesDeErguer` (0,45 s) o tempo até ela aparecer, `tempoDeSubida` (0,7 s) o deslize
até o pódio, e `podio` o marcador de destino (vazio = comemora onde parou).
→ `AnimadorPersonagem`: `passada` (1,75 m entre pisadas).

**Main Camera → `OrbitCamera`:** `sensibilidade` (baixe para 2 se o mouse estiver rápido
demais na máquina da apresentação), `distancia`, `inverterY`, e `giroDaVitoria` /
`distanciaDaVitoria` / `alturaDaVitoria` para o enquadramento do final.

**Objeto `Sistemas` → `HUDController`:** `esperaDoRelatorio` (4,5 s) — quanto a tela de
relatório espera para não cobrir a comemoração.

Os valores que cada alimento tira de peso estão no `switch` do método `Coletar`, em
`GameManager.cs`.

---

## Som (nenhum arquivo de áudio no projeto)

`AudioDirector.cs` sintetiza tudo a 22 kHz, por código:

- **Música** — onda quadrada, acelera conforme a etapa avança.
- **Coleta e dano** — bleeps subindo e descendo.
- **Zumbido da nuvem** — 3 senoides desafinadas entre si mais ruído filtrado. As
  frequências são múltiplos de 0,5 Hz de propósito: assim o buffer de 2 s fecha o ciclo
  exatamente e o loop não estala.
- **Vozes** — sílabas com envelope e altura variável por personagem. É o que faz o Dr.
  Moreira soar diferente da Dra. Helena sem nenhuma gravação.
- **Passos** — baque grave caindo de 150 para 55 Hz somado a um estalo de ruído. Disparam
  **por distância andada**, não por tempo: assim acompanham sozinhos a troca de andar para
  correr, sem evento de animação.

---

## Cuidados com recursos (já implementados)

- Sombreamento **por vértice** (`Legacy Shaders/VertexLit`) com a luz em `ForceVertex`:
  a iluminação é calculada uma vez por vértice, não por pixel. Dá profundidade às faces
  custando quase nada. O "modo leve" do menu troca tudo por `Unlit/Color`.
- Todo o cenário marcado como **BatchingStatic**: o Unity junta num só draw call os
  objetos que dividem material. Um material por textura de kit, então os ~700 objetos
  colapsam para bem poucas chamadas de desenho.
- **Um único Avatar** e dois controladores para todos os humanos da cena.
- Sombras, MSAA, HDR, luzes por pixel e reflection probes desligados no `PerformanceSetup`.
- Névoa linear escondendo a borda do mapa — profundidade de graça.
- Pool fixo de itens: nada é instanciado ou destruído durante o jogo.
- Áudio a 22 kHz gerado por código: zero arquivo de mídia.

Se travar na máquina da apresentação: rode a build em janela **1280×720** (não fullscreen)
e baixe `quantidadeBons` para 8.

---

## Gerar o executável

1. **File → Build Settings**.
2. **Add Open Scenes** (a cena precisa estar salva).
3. Platform **Windows, Mac, Linux** → Target **Windows**, Architecture **x86_64**.
4. **Player Settings → Resolution and Presentation**: Fullscreen Mode = **Windowed**,
   Default 1280 × 720. Em **Other Settings**, Color Space = **Gamma** (mais leve).
5. **Build**, escolha uma pasta vazia.
6. Para entregar, zipe a pasta **inteira** — o `.exe` não roda sem o `_Data` ao lado.

A pasta `Editor/` não entra na build; o construtor de cena fica só no Editor.

---

## Se algo der errado

- **O menu `Tools` desapareceu**: há erro de compilação. Qualquer erro vermelho mata
  todos os menus customizados. Causa mais comum: script de uma versão anterior que ficou
  na pasta. Apague `Assets/NutriRunner` e cole de novo.
- **Tudo rosa/magenta**: o projeto é URP. Troque para Built-In, ou abra
  `Assets/NutriRunner/Materials` e mude os shaders para `Universal Render Pipeline/Unlit`.
- **Nada se move / câmera parada**: rode `Verificar Cena`. Os suspeitos são mais de uma
  câmera na cena (a errada renderiza) ou erro vermelho no Console — qualquer exceção em
  `Update` congela tudo em silêncio.
- **O final não anima**: falta `Rig_Medium_Simulation.fbx` em `Modelos/`. A taça aparece
  acima da cabeça como fallback, mas sem a pose.
- **A pista está toda vermelha, sem gramado nem raias**: o `.mtl` não foi junto com o
  `.obj`. Os dois têm que estar lado a lado em `Modelos/`.
- **A prova não começa quando piso na pista**: o gatilho cobre a reta norte inteira, a
  que fica virada para a cidade. Se você entrou por uma curva ou pela reta de trás, siga
  até a reta de chegada. E confira no `Verificar Cena` que o `CircuitoOlimpico` tem a
  referência da pista.
- **Diz "FORA DAS RAIAS" mesmo eu estando no asfalto**: provavelmente o centro da pista
  no componente `PistaAtletismo` não bate com o lugar onde o modelo foi desenhado.
  Reconstrua a cena pelo menu — o construtor acerta os dois de uma vez.
- **A taça fica torta ou atravessando a mão**: ajuste `deslocamento` e `rotacao` no
  componente `Comemoracao` do Alex, com o jogo rodando. Não precisa recompilar.
- **Personagens brancos ou sem cor**: falta a pasta `Textures` dentro de `Modelos/`.
- **Prédios de costas para a praça**: troque `GIRO_FACHADA` de 180 para 0 no topo do
  `DemoSceneBuilder.cs`.
- **Cursor preso**: ESC solta, clique esquerdo trava de novo.
- **Quer reconstruir**: rodar o menu de novo apaga o `NutriRunner` antigo e monta tudo
  outra vez com o mesmo layout — a semente do Random é fixa (`20260929`).

---

## Créditos dos assets

Todos **CC0**:
- **Kenney** — kits de cidade, alimentos, mobiliário urbano (kenney.nl)
- **KayKit City Builder Bits** — ruas, prédios, parque (kaylousberg.com)
- **Quaternius Universal Animation Library** e **Universal Base Characters** — o
  `Mannequin_Medium` e todos os clipes de animação (quaternius.com)

Código, cenário procedural, áudio e design: feitos para este projeto.
