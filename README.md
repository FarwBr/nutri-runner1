# Nutri Runner

Jogo de terceira pessoa sobre obesidade e nutrição, feito em Unity 6 para a disciplina
de Desenvolvimento de Jogos da Univates.

Alex sai do hospital com o colesterol e a glicemia nos limites. Em três etapas ele
reconstrói o corpo: **come direito**, **ganha condicionamento** e **compete**. No fim,
ergue a taça.

A mecânica central cabe numa linha de código:

```csharp
velocidade = Mathf.Lerp(velocidadeInicial, velocidadeFinal, gm.Leveza);
```

Essa linha está na nuvem de cheiro que persegue o jogador. Cada alimento saudável deixa
o Alex mais leve, e mais leve é mais rápido — só que a nuvem lê o peso dele. **Melhorar
é o que te salva e é o que aperta o jogo.** A recompensa e a ameaça saem da mesma variável.

---

## Como rodar

1. Unity Hub → novo projeto **3D (Built-In Render Pipeline)**, Unity 2021 LTS ou mais novo.
   Evite URP/HDRP.
2. Clone este repositório por cima da pasta do projeto, ou copie `Assets/NutriRunner`
   para dentro do seu `Assets/`.
3. Coloque os modelos `.fbx` em **qualquer pasta dentro de `Assets/`** (veja abaixo).
4. Menu **Tools → Nutri Runner → Construir Cena Demo**.
5. **Ctrl+S** e **Play**.

### Controles

| Tecla | Ação |
|---|---|
| **W A S D** | andar (direção relativa à câmera) |
| **Mouse** | girar a câmera |
| **Shift** | correr |
| **Espaço** | pular, e avançar o diálogo |
| **ESC** | soltar o cursor |
| **R** | reiniciar depois do fim |

### Gerar o executável

**Tools → Nutri Runner → Gerar Executavel (Windows)**. O script confere que a cena está
salva, aplica as configurações certas (janela 1280×720, redimensionável, roda em segundo
plano) e compila. Para entregar, zipe a **pasta inteira** — o `.exe` não roda sem o
`_Data` do lado.

---

## Modelos 3D

O jogo **roda sem nenhum asset externo**: sem modelo, ele monta tudo com primitivas.
Com os modelos, fica como na apresentação.

O único modelo versionado aqui é a **pista de atletismo** (`pista_atletismo.obj` +
`.mtl`), convertida de um `.glb` porque a Unity não lê glTF sem pacote externo. Os
demais são pacotes CC0 que você baixa:

| De onde | Arquivos |
|---|---|
| [Quaternius](https://quaternius.com) | `Mannequin_Medium`, `Rig_Medium_General`, `Rig_Medium_MovementBasic`, `Rig_Medium_Simulation` |
| [KayKit](https://kaylousberg.com) / [Kenney](https://kenney.nl) | `building_A` a `building_H`, `base`, `bush`, `road_straight`, `streetlight`, `bench`, `tree`, `watertower` |
| [Kenney](https://kenney.nl) | `Apple`, `Bottle1`, `ChickenLeg` |
| qualquer um dos kits | a pasta `Textures` com o `colormap.png` |

**Podem ficar em qualquer pasta dentro de `Assets/`** — o construtor indexa o projeto
inteiro e procura por nome. O lugar recomendado é `Assets/Modelos`, **fora** de
`Assets/NutriRunner`, para apagar essa pasta nunca levar seus `.fbx` junto.

Se faltar algo, o Console lista exatamente o quê na primeira linha ao construir a cena.

---

## Mapa do repositório

```
Assets/NutriRunner/
├── Editor/          só no Editor, não entra no executável
│   ├── DemoSceneBuilder.cs     menus, constantes do mapa, Construir(), Verificar()
│   ├── Builder.Paleta.cs       materiais, texturas, busca de modelos
│   ├── Builder.Cidade.cs       terreno, avenidas, praça, prédios, marcos
│   ├── Builder.Estadio.cs      pista, pódio, auras das duas provas
│   ├── Builder.Personagens.cs  Avatar, Animators, Alex, NPCs, rivais, taça
│   ├── AferirPista.cs          28 testes da geometria da pista
│   └── Publicador.cs           gera o executável em um clique
├── Scripts/         25 componentes de runtime
└── Modelos/         os .fbx e as texturas (só a pista vem versionada)
docs/
└── guia-tecnico.md  o guia completo: balanceamento, assets, solução de problemas
```

Os cinco arquivos `Builder.*` são **`partial class` do mesmo tipo**: nada mudou de escopo
na separação, e cada parte enxerga as outras como quando era um arquivo só.

### A regra que mantém o runtime legível

O `GameManager` é o único lugar onde o estado mora, e **nenhum grupo conhece o outro**.
A nuvem de cheiro não sabe que existe uma pista; a pista não sabe que existiu uma nuvem.
Os dois perguntam o peso ao `GameManager`.

---

## As três etapas

**1 — A dieta.** Comece no hospital, junte 10 alimentos saudáveis fugindo das nuvens de
cheiro. A cada 3 alimentos a pizzaria solta mais uma nuvem, até 3 extras. Ser pego sobe
colesterol e glicemia **e engorda**.

**2 — O treino de rua.** O treinador corre junto como pacer. 6 auras amarelas
obrigatórias contra o cronômetro, e 5 azuis fora do caminho que custam tempo e dão
condicionamento.

**3 — O campeonato.** 400 m contra 4 rivais numa pista de atletismo oficial reduzida pela
metade. O peso define seu fôlego base; o condicionamento soma até +18 s e +8% de
velocidade. Sair do asfalto **congela o progresso** e desclassifica depois de 5 s
acumulados fora.

---

## Decisões técnicas que valem conhecer

**A cidade inteira é gerada por código.** `DemoSceneBuilder` monta ~980 objetos com uma
semente fixa (`Random.InitState(20260929)`), então o mapa sai idêntico todas as vezes.
Reprodutível, auditável em constantes com nome, e com verificação automática de colisão
entre peças.

**Nenhum arquivo de áudio.** Música, vozes, zumbido da nuvem e passos são sintetizados em
tempo de execução a 22 kHz no `AudioDirector`.

**A pista é uma cápsula, não uma elipse.** Duas retas de 84,39 m fechadas por semicírculos
de 36,50 m — a forma real de uma pista de 400 m, reduzida pela metade. O progresso é
medido em **metros de arco**, não em ângulo: medir ângulo dava 100 m de crédito por 36,8 m
andados a quem passasse rente ao centro. Os 28 testes do `AferirPista` guardam isso.

**Feito para rodar sem GPU dedicada**, que era requisito da disciplina: sombreamento por
vértice, nenhuma sombra em tempo real, MSAA e HDR desligados em código, todo o cenário em
batching estático, um único Avatar para os 9 personagens.

---

## Créditos

Modelos 3D de bibliotecas **CC0**: [Kenney](https://kenney.nl),
[KayKit City Builder Bits](https://kaylousberg.com),
[Quaternius](https://quaternius.com) (Universal Animation Library e Universal Base
Characters).

Cena procedural, lógica de jogo, áudio, interface e todo o código: escritos para este
trabalho.
