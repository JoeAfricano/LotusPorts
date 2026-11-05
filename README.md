# Lotus Ports

Aplicativo nativo para Windows, irmão do **Lotus**, que reúne os ports nativos para PC de jogos clássicos e transforma tudo em uma coisa só: **escolher um jogo e jogar**.

- Você escolhe o **jogo**, e o app recomenda a melhor forma de jogar.
- Um botão só: Instalar → Jogar → Atualizar e jogar → Resolver.
- Funciona sozinho. Com o Lotus instalado, ganha diagnóstico, desempenho e proteção de saves.

> O Lotus Ports não fornece, não baixa e não indica onde obter arquivos de jogos, BIOS ou chaves. Não é afiliado a fabricantes de consoles, publishers ou autores dos ports.

**Estado:** em desenvolvimento, Fase 0 (Fundação) concluída.

## Desenvolvimento

Requisitos: Windows 10 19041+ x64 e .NET SDK 10.0.401.

```
dotnet build LotusPorts.slnx -p:Platform=x64
dotnet test LotusPorts.slnx -p:Platform=x64
```

Documentação em `docs/`: especificação (`PROMPT_MESTRE.md`), `ARQUITETURA.md`, `UX.md`, `DEPENDENCIAS.md` e `PENDENCIAS.md`.
