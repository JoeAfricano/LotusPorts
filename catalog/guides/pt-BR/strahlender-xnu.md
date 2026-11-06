<!-- source: https://github.com/Veemyu/strahlender-xnu/blob/HEAD/README.md | fingerprint: 38698c30ca4bedca -->

- **Instalador (o mais fácil):** baixe o [instalador mais recente](https://github.com/Veemyu/strahlender-xnu/releases/latest) para o seu sistema. Ele guia você pelas chaves, arquivos do jogo e compilação. O Python vem junto. Veja os [detalhes do instalador](https://github.com/Veemyu/strahlender-xnu/blob/HEAD/installer/README.md).

- **Linha de comando:** `./install.command` no macOS (clique duas vezes no Finder) ou `./install-linux.sh` no Linux confere as suas ferramentas, pede os arquivos do jogo e compila.

- **Guia de compilação:** [docs/BUILD.md](https://github.com/Veemyu/strahlender-xnu/blob/HEAD/docs/BUILD.md) (em alemão: [docs/BUILD.de.md](https://github.com/Veemyu/strahlender-xnu/blob/HEAD/docs/BUILD.de.md)).

Do que você precisa: a sua própria cópia do jogo (XCI e, se quiser, o NSP de atualização) e o seu `prod.keys`, tirado do seu próprio console. Os apps para iPhone e iPad são compilados num Mac com Xcode e assinados com o seu próprio Apple ID.

**Export IPA** coloca os dados do jogo da sua própria cópia dentro do IPA, para uma ferramenta de instalação lateral como o iloader não precisar de mais nada. O IPA é grande (cerca de 5,3 GB, e precisa desse espaço livre), é só para os seus aparelhos e nunca deve ser compartilhado: ele traz dados do jogo da sua cópia, e este projeto não distribui nenhum. Se faltar espaço, a exportação para e diz quanto precisa; ela nunca troca sozinha para uma exportação sem os dados. Para exportar sem os dados de propósito, use "Export without game data" no instalador (0.1.4 ou mais novo) ou `--ios-export-no-embed`; esse IPA precisa da pasta **Game Library** ([detalhes](https://github.com/Veemyu/strahlender-xnu/blob/HEAD/installer/docs/ios-delivery.md#an-ipa-alone-does-not-run)).
