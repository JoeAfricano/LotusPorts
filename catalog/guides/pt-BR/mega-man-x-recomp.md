<!-- source: https://github.com/mstan/MegaManXSNESRecomp/blob/HEAD/README.md | fingerprint: f2aecbf505839510 -->

1. Baixe o [ZIP para Windows ou o AppImage para Linux](https://github.com/mstan/MegaManXSNESRecomp/releases/latest) mais recente. No Windows, extraia o ZIP; no Linux, torne o AppImage executável.

2. Abra o launcher e escolha o seu próprio arquivo do **Mega Man X (USA) (Rev 1)** (`.sfc` ou `.smc`). Arquivos com ou sem cabeçalho funcionam.

3. Configure o teclado ou o controle em **Controls**.

4. Ligue os recursos que quiser em **Mods** e clique em **Play**.

Para o Zero ou o co-op, escolha em Mods o seu próprio arquivo americano do **Mega Man X3**. O mod de armas do X3 usa a mesma escolha. As armas do X2 precisam do seu arquivo americano do **Mega Man X2**. Os recursos são extraídos sozinhos no seu PC.

Para a **armadura do X2 / X3 (experimental)**, ligue em **Mods**, escolha o jogo de origem no menu **Armor** e indique o arquivo americano desse jogo. O X começa com a aparência básica e o dash no chão do jogo de origem. As cápsulas do X1 liberam as partes correspondentes; as quatro partes normais do X3 continuam sendo a armadura padrão, e a cápsula do Hadouken também dá a Gold Armor e as melhorias dela. O mod vem desligado e passou por testes iniciais.

Escolha **Modern** em **Zero behavior** em qualquer um dos mods do Zero para combate com sabre e movimentos aéreos extras. **X3 Behavior** é o padrão. **Add Zero** e **X / Zero Co-op** são alternativas; ligar um desliga o outro.

Todos os estilos do Zero quebram blocos com a cabeça e os pés desde o começo, inclusive no co-op. O X ainda precisa das melhorias de armadura correspondentes.

Para tela larga, ligue **Widescreen** em Mods e escolha a proporção. **Display Aspect** em Settings controla a proporção dos pixels. O jogo em rede usa a visão original ou 16:9, 21:9 ou 32:9 fixos.

Guias: [Zero](https://github.com/mstan/MegaManXSNESRecomp/blob/HEAD/docs/zero-0.0.1.md), [armas do X2/X3](https://github.com/mstan/MegaManXSNESRecomp/blob/HEAD/docs/x-weapons-port.md), [saves por senha](https://github.com/mstan/MegaManXSNESRecomp/blob/HEAD/docs/password-saves.md) e [jogo em rede](https://github.com/mstan/MegaManXSNESRecomp/blob/HEAD/docs/netplay.md). Nenhum arquivo de jogo ou recurso extraído vem nos downloads.

Para o truque de speedrun do Storm Eagle e saves de treino, veja o [guia do Magic Carpet](https://github.com/mstan/MegaManXSNESRecomp/blob/HEAD/docs/magic-carpet.md).

### Qual arquivo?

O jogo precisa da cópia do cartucho **v1.1 (Rev 1)**. A versão original v1.0 tem código diferente e é recusada. As somas são do arquivo sem o cabeçalho de copiadora de 512 bytes; o launcher remove o cabeçalho antes de conferir.

- Uso · Nome No-Intro · SHA-256 · SHA-1 · CRC32

- Jogo · Mega Man X (USA) (Rev 1) · `b8f70a6e7fb93819f79693578887e2c11e196bdf1ac6ddc7cb924b1ad0be2d32` · `c65216760ba99178100a10d98457cf11496c2097` · `DED53C64`

- Zero, co-op, armas/armadura do X3 · Mega Man X3 (USA) · `65b03268afac296330e8ff8d60dd0825879e13ed658b37713c034a3bd074f1d7` · `b226f7ec59283b05c1e276e2f433893f45027cac` · `FA0FE671`

- Armas/armadura do X2 · Mega Man X2 (USA) · `f3246755f608a1e1dc9c848b61da3b824c7853b29b3be40df6fc7f2793a887ed` · `637079014421563283cded6aeaa0604597b2e33c` · `947B0355`

Se o launcher recusar um arquivo, ele diz qual cópia você escolheu e o que precisa. Por exemplo: "This is Mega Man X (USA) v1.0. Mega Man X needs Mega Man X (USA) v1.1 (Rev 1) (SHA-256 b8f70a6e…)". Ele reconhece a v1.0, as versões europeia e japonesa, as extrações da Legacy Collection e da Mega Man X Collection, e X2/X3 escolhidos no lugar errado. Um arquivo desconhecido é descrito pelo cabeçalho interno, que também mostra se ele foi modificado. Arquivos ZIP e 7z também são identificados, então extraia o arquivo antes. **Copy details** copia as somas do seu arquivo para um relato de erro.

Não escolha um arquivo com patch para MSU-1, tradução ou hack. Música MSU-1, Zero e co-op são acrescentados por **Mods**.
