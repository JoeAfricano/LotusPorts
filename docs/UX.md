# UX do Lotus Ports

A arte conceitual enviada pelo José em 05/11/2026 é a referência visual e prevalece sobre o prompt mestre.

## Estrutura

- **Barra lateral:**
  - logo (pétala de lótus + "LOTUS / PORTS");
  - Biblioteca, Catálogo, Downloads, Integração com o Lotus;
  - separador;
  - Configurações, Ajuda;
  - no rodapé, o cartão de status do Lotus.
- **Biblioteca:**
  - hero com cena japonesa (lua, Fuji, torii, cerejeiras), estático e só vetorial;
  - título em duas linhas (a segunda em rosé), subtítulo e busca (`Ctrl+K` foca a busca de qualquer tela);
  - prateleiras e cartões de jogo na Fase 1.
- **Catálogo:** todos os jogos para descobrir, organizados por jogo, mais as fontes de catálogo importadas (seção 5.8).
- **Página do jogo:** botão "Voltar para a biblioteca" e conteúdo da seção 9.5, sem a seção Saves.
- **Configurações:** cartões Geral, Arquivos de jogo, Fontes de catálogo, Modo avançado e Sobre/Legal.

## Estados de tela

| Estado | Como aparece |
|---|---|
| Carregando | Esqueleto de cartões (`Bg.Surface`) |
| Vazio | `EmptyState`: pétala apagada, título, frase útil e selo opcional |
| Erro | `ErrorWindow`: mensagem simples, detalhes recolhidos e "Copiar detalhes" |
| Indisponível nesta fase | `EmptyState` com o selo "Em desenvolvimento" |

Nada aparece como pronto antes de existir. Por exemplo, o status do Lotus mostra "Em desenvolvimento", nunca "Conectado".

## Tokens (`Lotus.Design/LotusTheme.xaml`)

| Token | Valor | Uso |
|---|---|---|
| `Bg.Base` | `#0D0A0F` | fundo |
| `Bg.Sidebar` | `#110C13` | barra lateral |
| `Bg.Surface` | `#171219` | cartões |
| `Bg.Elevated` | `#221A26` | hover, cartões de escolha |
| `Stroke.Subtle` | `#33263A` | bordas |
| `Text.Primary` | `#F6F0F4` | texto principal |
| `Text.Secondary` | `#B3A6B4` | texto de apoio |
| `Accent.Rose` | `#F09ABD` | acento, segunda linha do título |
| `Accent.Pink` | `#F0508C` | ações, foco, item selecionado |
| `Accent.Magenta` | `#C42A78` | destaques, progresso |
| `Accent.Gradient` | `#F25A93 → #D23A7A` | botão principal, item de menu ativo |
| `State.Ok` | `#5FD38F` | somente quando verificado |
| `State.Warn` | `#F2C46D` | aviso |
| `State.Fail` | `#FF6B6B` | falha |
| `State.Unknown` | `#8C8799` | não verificado, em desenvolvimento |

Os valores partem da tabela do prompt, ajustados para a paleta ameixa da arte. Cada estado tem também uma variante `.Subtle`, de fundo translúcido, usada nos selos.

**Estilos:**
- tipografia: `Type.Display`, `Type.Title`, `Type.Subtitle`, `Type.Body`, `Type.Caption`;
- superfícies e escolhas: `Lotus.Card`, `Lotus.NavItem` (pílula rosé quando selecionado), `Lotus.ChoiceCard` (cartão com check, como o seletor Original/Remasterizado da arte).

**Controles:**
- `LotusPetal` (acesa ou apagada);
- `StateBadge` (sempre com texto e ícone, nunca só cor);
- `EmptyState`;
- `SakuraHero`.

## Regras

- Nenhuma cor solta nas telas (há um teste que verifica).
- Todo texto vem do `.resw`, em pt-BR e en-US com as mesmas chaves (há um teste que verifica).
- Animações são leves e só rodam enquanto há algo acontecendo. O hero é estático.
- `AutomationProperties.Name` vai em todo controle interativo.

## Fase 1: Biblioteca, Catálogo e Página do jogo

- **Biblioteca:**
  - hero, busca de jogos e ações e abas em pílula (Todos, Favoritos, Instalados e Não instalados, com contagem);
  - chips de plataforma e lista de gênero;
  - prateleiras Chegou!, Continuar jogando, Prontos para jogar, Para descobrir e Em breve; cada uma só aparece se tiver jogos;
  - "Ainda em construção" só aparece em Em breve, e Chegou! nunca marca tudo como novo na primeira abertura.
- **Cartão (`GameCard`):** capa tipográfica gerada (`GameCover`, cor de assinatura por plataforma, nunca capa oficial), selo de status em texto, selo Em revisão/Fonte externa/Adicionado por você, coração de favorito e linha "Plataforma · estado".
- **Página do jogo:**
  - capa, título e selos;
  - port recomendado com o motivo;
  - botão Instalar desativado, com "Em desenvolvimento";
  - blocos Arquivo do jogo, Dependências e Lotus em "Não verificado";
  - Sobre este port, só com fatos do catálogo (os hashes aparecem só no modo avançado);
  - Outras formas de jogar, com a origem de cada port.
- **Catálogo:** grade de todos os jogos com filtro, fontes de catálogo (oficial validado e importadas com estado OK/Aviso/Não verificado) e, no modo avançado, adicionar port por link e exportar.
- **Tokens novos:** `Color.Platform.*`. **Estilos novos:** `Lotus.Pill` e `Lotus.CardButton`.