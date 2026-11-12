# Novidades

## Próxima versão

- Jogos e ports novos chegam sem atualizar o app: uma vez por dia, o Lotus Ports confere o catálogo oficial no GitHub e usa a versão nova se ela passar na validação.
- Cada port agora tem status (Jogável, Com alguns bugs, Em construção…) e a lista de problemas conhecidos, tirados do README e dos relatos dos jogadores no GitHub. A Página do jogo mostra os dois em "Sobre este port".

## 0.1.2-alpha (07/11/2026)

**Versão alpha, em pré-teste.** Pode ter falhas; mantenha cópias dos seus saves importantes.

- Visual do Catálogo e do Início refeito: filtros e busca com cantos arredondados de verdade, categorias em cartões com cor e contador, nomes de console inteiros.
- Mais capas, inclusive de títulos com o ano entre parênteses (como "Sonic the Hedgehog (2013)").
- Pasta de instalação organizada: os arquivos do programa ficam em `app\`; a pasta mostra só o atalho e o desinstalador.
- Instalador menor (63 MB): saíram componentes de IA do Windows App SDK que o app não usa.
- O instalador pede ao app para fechar normalmente antes de atualizar.
- Atualização automática ligada pelas Releases deste repositório.


## 0.1.1-alpha (07/11/2026)

- Arquivo do jogo entregue ao port: "Colocar na pasta do jogo" nos ports que leem o arquivo da própria pasta.
- A busca automática encontra jogos dentro de arquivos .zip, sem extrair nada que não seja reconhecido.
- Mais versões aceitas: Ship of Harkinian, 2 Ship 2 Harkinian e King Kong.
- Downloads: um por vez, com "Na fila", e a lista dos concluídos recentemente.
- Tempo jogado e número de partidas na página do jogo.
- "Novidades nos seus favoritos" no Início quando um jogo favorito ganha versão nova.
- Selo "Funciona no seu PC" com o FPS medido pelo Lotus.
- Imagens do port, tiradas do repositório oficial, na página do jogo.
- Mais capas no catálogo.


## 0.1.0-alpha (07/11/2026)

Primeira versão de teste do Lotus Ports.

### Jogar
- Catálogo com mais de 640 jogos e mais de 800 formas de jogar, organizadas pelo jogo, cada uma com a forma recomendada.
- Instalar, Jogar, Atualizar e Voltar para a versão anterior, sempre conferindo o arquivo com o publicado no GitHub.
- Atualizações automáticas dos ports instalados (uma vez por dia, nunca com o jogo aberto).
- "Resolver" quando um jogo não abre logo depois de atualizar.
- Instruções de uso em português para todos os ports.

### Arquivo do jogo
- Reconhece a sua cópia pela assinatura, inclusive dentro de um `.zip`. O `.zip` não é alterado; o arquivo do jogo é copiado para a pasta do Lotus Ports.
- A página do jogo mostra qual versão o port aceita (por exemplo "Aceita: EUA 1.0").

### Estilos e mods
- Original e Remasterizado com opções confirmadas no código de Zelda 64: Recompiled, Ship of Harkinian e 2 Ship 2 Harkinian, com backup e Desfazer.
- Mods em arquivo ou em pasta (também pelo `.zip` baixado) em 7 ports, sempre com backup.

### Visual
- Ícones próprios para cada console, sem logotipos de marca.
- Faixa de plataformas no Catálogo e na Biblioteca, seletores em pílula e animações leves.

### Integração com o Lotus
- Diagnóstico do PC no "Resolver", seus jogos do Lotus Ports em Meus Jogos no Lotus, sessões de jogo e medição de FPS com permissão.
- Uma janela só: abrir de novo (ou por link `lotusports://`) traz a janela aberta para a frente.

### Catálogo experimental
- Jogos de Nintendo Switch 2 marcados como **Experimental**: Pokémon Pokopia (duas formas de jogar), Donkey Kong Bananza e Super Mario Bros. Wonder.
