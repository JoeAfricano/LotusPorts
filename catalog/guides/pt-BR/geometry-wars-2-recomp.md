<!-- source: https://github.com/saulob/geometry-wars-retro-evolved-2-recompiled/blob/HEAD/README.md | fingerprint: ebbaa46b53f6d59d -->

1. Baixe o **GeometryWarsRetroEvolved2-v0.6.1-windows-x64.zip** na [versão mais recente](https://github.com/saulob/geometry-wars-retro-evolved-2-recompiled/releases/latest) e extraia num lugar onde dê para gravar.

2. Abra o **geometry_wars_retro_evolved_2.exe**. O launcher aparece.

3. Na página **Play**, clique em **Install from package...** e escolha o seu pacote. É um arquivo só, com um nome hexadecimal longo e sem extensão. Num disco do Xbox 360, ele fica em `Content\0000000000000000\584108FF\000D0000\`.

4. Espere aparecer **Ready to play**. Os arquivos do jogo são copiados para a pasta `game` ao lado do programa. O próprio arquivo de pacote não é copiado.

5. Clique em **PLAY**.

**Nenhum arquivo do jogo vem** na versão.

**Reinstall from package...** repara os arquivos do jogo e mantém configurações e saves.

### Saves e configurações

Configurações, perfil, pontuações, conquistas, saves e progresso, e o cache de shaders ficam numa pasta portátil `user_data` ao lado do programa (ao lado do AppImage no Linux), junto com `game` e `logs`. Ela é criada na primeira vez que precisar e não vem no download. Copiar a pasta inteira do aplicativo para outro PC leva os seus dados junto. `--user_data_root=<folder>` ainda muda o local.

**Vindo da v0.1.0:** a v0.1.0 guardava esses dados em `Documentos\geometry_wars_retro_evolved_2` no Windows e em `~/.local/share/geometry_wars_retro_evolved_2` (ou `$XDG_DATA_HOME`) no Linux. A primeira abertura da v0.2.0 move tudo para `user_data` sozinha: renomeando quando dá, senão com uma cópia conferida, e a pasta antiga só é removida depois que isso dá certo. Se a mudança falhar, a pasta antiga fica intacta e é usada, e a mudança é tentada de novo na próxima abertura. Se os dois lugares já tiverem dados, a `user_data` é usada e a antiga fica como está, nunca misturada. Se a pasta do programa for somente leitura, o local antigo é usado naquela vez. Detalhes no [docs/user-data.md](https://github.com/saulob/geometry-wars-retro-evolved-2-recompiled/blob/HEAD/docs/user-data.md).

### SmartScreen do Windows

O Windows pode avisar na primeira vez, porque a versão não tem assinatura digital. Se você baixou da página de versões deste repositório, escolha **Mais informações** e depois **Executar assim mesmo**.
