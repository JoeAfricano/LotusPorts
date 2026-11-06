<!-- source: https://github.com/power-pudu/egg-of-time/blob/HEAD/README.md | fingerprint: 4f828969cf1eab7f -->

> **O Windows pode bloquear a primeira abertura** com uma tela azul ou vermelha "O Windows protegeu o computador". O Egg of Time não tem um certificado pago, então o SmartScreen ainda não o reconhece. Clique em **Mais informações** e depois em **Executar assim mesmo**. O Windows lembra da sua escolha para esta versão.

1. Baixe o `EggOfTime-<version>-windows-x64.zip` mais recente na [página de versões (Releases)](https://github.com/power-pudu/egg-of-time/releases).

2. Extraia numa pasta onde dê para gravar, como `Documentos\Egg of Time`. Evite `Arquivos de Programas`.

3. Abra o **`Egg of Time.exe`** e escolha o seu arquivo do jogo (um `.nds`) quando pedir.

4. Espere a montagem da primeira abertura. O Egg of Time traduz o jogo do seu arquivo para código nativo. Isso leva alguns minutos e mostra uma barra de progresso. Depois, abre em uns dois segundos.

> Nada é instalado nem baixado: as ferramentas da montagem (dsd, Python, clang) vêm na pasta `kit\` do zip. Se a montagem falhar, o `game\build.log` traz os detalhes.

**Onde ficam os seus arquivos.** Tudo fica na pasta do Egg of Time:

- O quê · Onde

- O seu arquivo do jogo (copiado na primeira abertura) · `rom\CT.nds`

- Saves do jogo · `rom\CT.sav`

- Estado salvo (F5 / F7) · `rom\CT.state`

- Configurações · `eggoftime.ini`, `controls.ini`

- Mods · `mods\cutscenes\`, `mods\music\`, `mods\translation\`

> **Atualizar:** extraia a versão nova por cima da pasta antiga. O seu arquivo do jogo, os saves e as configurações são mantidos. Se a versão nova mudou o recompilador, o jogo é montado de novo na primeira abertura.
