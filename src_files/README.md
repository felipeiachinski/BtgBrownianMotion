# BrownianSim (MAUI .NET 9 — Windows)

Starter pronto para simular **movimento browniano** em .NET MAUI (Windows) usando **GraphicsView/IDrawable** e arquitetura **MVVM**. Inclui projeto **Core** (net9.0) + **Testes** (xUnit).

## Uso
```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\setup.ps1 -ProjectName MinhaSimulacao
```

**Importante:** Informe **Volatilidade** e **Retorno médio** em **%** (ex.: 20 => 20%, 1 => 1%). O app converte internamente para fração decimal.

## Parâmetros
- Preço inicial, Volatilidade média (%), Retorno médio (%), Dias
- Simulações (1–10), grade e legenda

## Print
Atualize `docs/screenshot.png` após rodar o app.
