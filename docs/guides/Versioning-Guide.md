# Guía de Versionado y Releases

Este documento explica cómo funciona el sistema de versionado automático en el proyecto AppCore.

## 🏗️ **Sistema de Versionado**

### **Herramientas Utilizadas**
- **MinVer**: Cálculo automático de versiones basado en Git tags
- **Versionado Semántico**: Formato `MAJOR.MINOR.PATCH[-PRERELEASE]`
- **GitHub Actions**: Automatización de builds y releases

### **Esquema de Ramas**
- **`main`**: Versiones de producción (ej: `1.2.3`)
- **`develop`**: Versiones preview (ej: `1.2.3-preview.45`)
- **Feature branches**: Heredan el versionado de la rama base

## 📦 **Proceso de Release**

### **1. Para Releases de Producción**
```bash
# 1. Asegurar que main está actualizado
git checkout main
git pull origin main

# 2. Crear tag de version 
git tag v1.2.3
git push origin v1.2.3

# 3. El CI/CD automáticamente:
#    - Calcula la versión
#    - Ejecuta tests
#    - Crea el paquete NuGet
#    - Publica a GitHub Packages
#    - Crea el GitHub Release
```

### **2. Para Prereleases (Develop)**
```bash
# Simplemente hacer push a develop
git checkout develop
git push origin develop

# El CI/CD automáticamente creará una version preview
# Ejemplo: 1.2.3-preview.45
```

## 🎯 **Tipos de Versiones**

### **Versiones de Producción**
- **Formato**: `1.2.3`
- **Trigger**: Tags en formato `v1.2.3`
- **Deploy**: GitHub Packages (Production environment)
- **Automático**: ✅ Release notes, ✅ GitHub Release

### **Versiones Preview**  
- **Formato**: `1.2.3-preview.N`
- **Trigger**: Push a `develop` branch
- **Deploy**: GitHub Packages (Preview environment)
- **Automático**: ✅ Package creation

## 📋 **Reglas de Versionado**

### **Incremento de Versiones**
- **MAJOR** (`1.0.0 → 2.0.0`): Breaking changes
- **MINOR** (`1.0.0 → 1.1.0`): Nuevas características (backward compatible)
- **PATCH** (`1.0.0 → 1.0.1`): Bug fixes (backward compatible)

### **Prereleases**
- **preview**: Versiones en desarrollo
- **alpha**: Características experimentales  
- **beta**: Versiones casi estables
- **rc**: Release candidates

## 🔧 **Comandos Útiles**

### **Verificar Versión Local**
```bash
# Instalar MinVer CLI
dotnet tool install --global minver-cli

# Ver versión actual
minver -t v

# Ver con detalles  
minver -t v -v d
```

### **Crear Tags Manualmente**
```bash
# Tag de release
git tag v1.2.3
git push origin v1.2.3

# Tag de prerelease
git tag v1.2.3-beta.1
git push origin v1.2.3-beta.1
```

## 📁 **Archivos de Configuración**

### **MinVer Config** (`minver.yaml`)
```yaml
minimum-major-minor=1.0
default-pre-release-identifiers=preview
tag-prefix=v
```

### **Project Config** (`AppCore.csproj`)
```xml
<MinVerTagPrefix>v</MinVerTagPrefix>
<MinVerDefaultPreReleaseIdentifiers>preview</MinVerDefaultPreReleaseIdentifiers>
```

## ⚡ **Automatización del CI/CD**

### **Jobs Ejecutados**
1. **Build & Validate**: Compila y ejecuta tests (JIT + AOT)
2. **Code Quality**: Análisis de calidad y seguridad  
3. **Package**: Crea paquetes NuGet versionados
4. **Deploy Preview**: Publica a GitHub Packages (develop)
5. **Deploy Production**: Publica a GitHub Packages (releases)
6. **Create Release**: Crea GitHub Release automático (main)
7. **Update Docs**: Actualiza documentación

### **Metadatos del Package**
- ✅ Commit SHA
- ✅ Build number  
- ✅ Timestamp
- ✅ CI/CD build flag
- ✅ Archivos de símbolos (.snupkg)

## 🚀 **Ejemplos de Uso**

### **Flujo Típico de Desarrollo**
```bash
# 1. Feature branch
git checkout -b feature/new-feature
# ... desarrollo ...
git push origin feature/new-feature

# 2. Merge a develop (preview)
git checkout develop
git merge feature/new-feature  
git push origin develop
# → Genera: 1.2.3-preview.123

# 3. Release a main
git checkout main
git merge develop
git tag v1.2.3
git push origin main v1.2.3
# → Genera: 1.2.3 + GitHub Release
```

## 📊 **Monitoreo**

### **Verificar Packages**
- [GitHub Packages](https://github.com/Harol-Reina/AppCore-Standalone/packages)
- [Releases](https://github.com/Harol-Reina/AppCore-Standalone/releases)
- [Actions](https://github.com/Harol-Reina/AppCore-Standalone/actions)

### **Logs de Build**
Todos los builds incluyen información detallada de versionado en los logs de GitHub Actions.