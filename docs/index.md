# AppCore Documentation

Documentacion oficial de **OrionSoft.AppCore**, libreria base para desarrollo de aplicaciones .NET 10 con arquitectura limpia y soporte NativeAOT.

## Guias

*   [Release Process](guides/Release-Process.md) — Proceso de publicacion via GitHub Actions
*   [Versioning Guide](guides/Versioning-Guide.md) — Versionado semantico con MinVer

## API Reference

La referencia de API se genera automaticamente con DocFX a partir del codigo fuente. Ejecutar:

```bash
dotnet tool install -g docfx
docfx docs/docfx.json --serve
```
