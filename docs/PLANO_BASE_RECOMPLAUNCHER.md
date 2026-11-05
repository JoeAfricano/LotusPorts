# Plano: o Lotus Ports com o RecompLauncher como base

Decisões do José em 05/11/2026:
- o [RecompLauncher](https://github.com/elripalda/RecompLauncher) (GPL-3.0, Rust/Tauri) é a base de funcionamento;
- a cara é do Lotus e o jeito é o nosso: interface intuitiva, otimizada, simples e organizada;
- a arte vem do pacote premium do José.

## Como usamos a base

- **Reimplementação, sem cópia de código.** Reproduzimos telas, fluxos e comportamento em C#/WinUI na arquitetura que já temos. Lemos o código-fonte público para entender o comportamento; não descompilamos o instalador nem copiamos trechos, e assim o Lotus Ports não fica preso à GPL.
- **Catálogo: descobrir e verificar.** A lista pública do recomplauncher.com serve só para descobrir repositórios e qual jogo cada um porta.
  - O `CatalogScout` confere tudo no GitHub: release, arquivo para Windows e executável.
  - Textos, capas e notas de terceiros não são copiados; as descrições são nossas.
  - Os mapeamentos ficam em `catalog/research/recomplauncher-map.json`, com crédito à fonte.
- **Formato `recomplauncher.json`.** Muitos desenvolvedores publicam esse arquivo na própria release, com o arquivo do jogo pedido, a checagem, o programa a abrir, saves e configurações. O Lotus Ports vai **ler esse arquivo** para fazer o setup fácil (interoperabilidade com o formato aberto). É a fonte mais confiável de setup, porque vem do autor do port.

## O que entra (sem depender de servidor)

| Função do RecompLauncher | No Lotus Ports | Fase |
|---|---|---|
| Discover (seções: lançamento, populares, atualizados, recém-chegados, por console, categorias) | **Início**, com prateleiras na mesma lógica e dados reais (estrelas, datas e downloads lidos do GitHub) | próxima |
| Uma entrada por jogo, várias versões | Já temos: port recomendado + "Outras formas de jogar", com troca de versão | 1 (feito) e 2 |
| Instalar e atualizar em um clique, conferindo o checksum do GitHub | Orquestrador "Jogar", com SHA-256 do GitHub e versões lado a lado | 2 |
| Easy Setup (disco/pasta do jogo) | Setup fácil lendo o `recomplauncher.json` da release; vínculo do arquivo do jogo | 3 |
| Big Picture | **Modo TV**, com as mesmas telas ampliadas e controle | 7 |
| Looks (efeitos de tela por jogo) | Efeitos de tela como opção avançada por jogo | depois |
| Saves com backup antes de atualizar | **Fica com o Lotus** (decisão anterior) | integração |

## O que fica fora

Contas, nuvem, telemetria e diagnósticos enviados, "Get it on my PC", relatórios de jogadores, betas por servidor e plugins de terceiros. Os motivos:
- as regras do projeto proíbem telemetria e envio de dados;
- todas essas funções dependem do servidor deles.

## Identidade visual

Pacote `Lotus_Ports_Premium_Assets_Profissionais_FHD.zip`:
- **Fundos:** os cenários Santuário Lunar e Jardim Cibernético entram como fundo do hero, com gradiente de leitura.
- **Marca e ícones:** a marca em SVG, o ícone do app e os 59 ícones próprios.
- **Ilustrações:** as dos estados vazio, offline, arquivo ausente e atualização.
- **Mockups:** Biblioteca, Página do jogo, Estados e Primeiro uso orientam a densidade e a hierarquia. Os números e jogos dos mockups são fictícios e nunca viram dados.
