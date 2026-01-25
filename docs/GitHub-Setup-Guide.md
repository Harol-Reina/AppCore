# Guía de Configuración de GitHub para AppCore

Esta guía proporciona los pasos detallados para configurar el proyecto AppCore en GitHub y asegurar que el pipeline CI/CD funcione correctamente.

## 📋 Requisitos Previos

- Cuenta de GitHub
- Git instalado localmente
- .NET 10.0.x SDK instalado
- Acceso a NuGet.org (opcional, para publicación)

## 🚀 Paso 1: Creación del Repositorio en GitHub

### 1.1 Crear el Repositorio

1. Ve a [GitHub](https://github.com) e inicia sesión
2. Haz clic en el botón **"New"** (Nuevo) o **"Create repository"**
3. Configura el repositorio:
   - **Repository name:** `AppCore`
   - **Description:** `Clean Architecture foundation library providing common patterns, utilities, and abstractions for .NET applications.`
   - **Visibility:** Público o Privado (según tu preferencia)
   - **⚠️ NO** inicializes con README, .gitignore o License (ya los tienes localmente)

4. Haz clic en **"Create repository"**

### 1.2 Configurar el Repositorio Local

```bash
# Navega a tu directorio del proyecto
cd /media/Data/Source/OrionSoft/AppCore-Standalone

# Inicializar Git si no está inicializado
git init

# Agregar el remote origin
git remote add origin https://github.com/TU_USUARIO/AppCore.git

# Verificar la configuración del remote
git remote -v
```

## ⚙️ Paso 2: Configuración de Secrets y Variables

El pipeline CI/CD requiere varios secrets y variables de entorno configurados en GitHub.

### 2.1 Acceder a la Configuración de Secrets

1. Ve a tu repositorio en GitHub
2. Haz clic en **"Settings"** (Configuración)
3. En el menú lateral, selecciona **"Secrets and variables"** > **"Actions"**

### 2.2 Configurar Secrets Requeridos

#### Para publicación en NuGet.org (Producción):
1. Haz clic en **"New repository secret"**
2. Crear el secret:
   - **Name:** `NUGET_API_KEY`
   - **Secret:** Tu API key de NuGet.org
   - Haz clic en **"Add secret"**

#### Para GitHub Packages (Automático):
- `GITHUB_TOKEN` se genera automáticamente (no es necesario configurarlo)

### 2.3 Obtener API Key de NuGet.org

1. Ve a [NuGet.org](https://www.nuget.org)
2. Inicia sesión con tu cuenta
3. Ve a tu perfil > **"API Keys"**
4. Haz clic en **"Create"** para crear una nueva API key:
   - **Key Name:** `AppCore-GitHub-Actions`
   - **Package Owner:** Tu usuario
   - **Scopes:** `Push` y `Push new packages and package versions`
   - **Packages:** Selecciona el patrón `AppCore*` o déjalo en blanco para todos
5. Copia la API key generada y úsala en el paso 2.2

## 🔧 Paso 3: Configuración de Environments

El pipeline usa dos environments diferentes para staging y producción.

### 3.1 Configurar Environment "preview"

1. En GitHub, ve a **Settings** > **Environments**
2. Haz clic en **"New environment"**
3. Nombre: `preview`
4. Configuración opcional:
   - **Protection rules:** Ninguna (para desarrollo)
   - **Environment secrets:** Ninguno adicional requerido

### 3.2 Configurar Environment "production"

1. Crear environment: `production`
2. **Protection rules recomendadas:**
   - ✅ **Required reviewers:** Agrégarte a ti mismo
   - ✅ **Wait timer:** 10 minutos (opcional)
   - ✅ **Deployment branches:** Solo `main`
3. **Environment secrets:**
   - Aquí puedes agregar secrets específicos de producción si es necesario

## 📄 Paso 4: Configurar GitHub Pages (Opcional)

Para documentación automática:

1. Ve a **Settings** > **Pages**
2. **Source:** Deploy from a branch
3. **Branch:** `gh-pages` / `/ (root)`
4. Haz clic en **"Save"**

## 🛡️ Paso 5: Configurar Branch Protection Rules

### 5.1 Proteger la rama main

1. Ve a **Settings** > **Branches**
2. Haz clic en **"Add rule"**
3. **Branch name pattern:** `main`
4. Configurar protecciones:
   - ✅ **Require a pull request before merging**
   - ✅ **Require status checks to pass before merging**
   - ✅ **Require branches to be up to date before merging**
   - ✅ **Status checks:** Seleccionar:
     - `Build and Test`
     - `Code Quality Analysis`
   - ✅ **Restrict pushes that create files larger than 100 MB**

### 5.2 Configurar rama develop (Opcional)

Repetir el proceso anterior para la rama `develop` si planeas usarla.

## 📦 Paso 6: Verificar Configuración del Proyecto

### 6.1 Verificar URLs en AppCore.csproj

Asegúrate de que las URLs en el archivo `src/AppCore/AppCore.csproj` sean correctas:

```xml
<PackageProjectUrl>https://github.com/TU_USUARIO/AppCore</PackageProjectUrl>
<RepositoryUrl>https://github.com/TU_USUARIO/AppCore.git</RepositoryUrl>
```

**⚠️ Reemplaza `TU_USUARIO` con tu usuario real de GitHub**

### 6.2 Verificar sonar-project.properties

Si usas SonarCloud, actualizar `sonar-project.properties`:

```properties
sonar.organization=TU_ORGANIZACION_SONAR
```

## 🚀 Paso 7: Primer Push y Validación

### 7.1 Realizar el Primer Commit

```bash
# Agregar todos los archivos
git add .

# Crear el commit inicial
git commit -m "feat: initial AppCore project setup

- Clean Architecture foundation library
- CI/CD pipeline with GitHub Actions
- Unit tests and SpecFlow BDD tests
- Code coverage and quality analysis
- NuGet package creation"

# Crear y cambiar a la rama main
git branch -M main

# Push inicial
git push -u origin main
```

### 7.2 Crear Rama Develop

```bash
# Crear rama develop
git checkout -b develop

# Push de la rama develop
git push -u origin develop
```

## ✅ Paso 8: Verificar que el Pipeline Funcione

### 8.1 Verificar Actions

1. Ve a tu repositorio en GitHub
2. Haz clic en la pestaña **"Actions"**
3. Deberías ver el workflow **"CI/CD Pipeline"** ejecutándose o completado

### 8.2 Verificar Jobs Ejecutados

El pipeline debe ejecutar estos jobs exitosamente:

- ✅ **Build and Test** - Construye y ejecuta pruebas
- ✅ **Code Quality Analysis** - Análisis de calidad de código
- ✅ **Create Package** - Crea el paquete NuGet
- ✅ **Update Documentation** - Actualiza documentación (solo en main)

### 8.3 Verificar Artifacts

Después de una ejecución exitosa, deberías ver estos artifacts:

- `test-results` - Resultados de pruebas
- `coverage-report` - Reporte de cobertura
- `nuget-packages` - Paquetes .nupkg generados
- `symbol-packages` - Paquetes de símbolos .snupkg

## 🔄 Paso 9: Workflow de Desarrollo

### 9.1 Para Desarrollo (rama develop)

```bash
# Trabajar en develop
git checkout develop
git pull origin develop

# Hacer cambios...
git add .
git commit -m "feat: new feature description"
git push origin develop
```

**Resultado:** Se ejecuta el pipeline y se publica en GitHub Packages como preview.

### 9.2 Para Producción (rama main)

```bash
# Crear Pull Request desde develop a main
# Revisar y aprobar el PR
# Merge del PR a main
```

**Resultado:** Se ejecuta el pipeline completo y actualiza documentación.

### 9.3 Para Releases

```bash
# Desde main, crear un tag de release
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0

# Crear release en GitHub UI
```

**Resultado:** Se publica automáticamente en NuGet.org.

## 🐛 Solución de Problemas Comunes

### Error: "remote origin already exists"
```bash
git remote remove origin
git remote add origin https://github.com/TU_USUARIO/AppCore.git
```

### Error: "Coverage threshold not met"
- El pipeline requiere al menos 80% de cobertura de código
- Revisa el reporte de cobertura en los artifacts
- Agrega más pruebas unitarias si es necesario

### Error: "NUGET_API_KEY not found"
- Verifica que hayas configurado el secret correctamente
- Asegúrate de que el environment "production" está configurado

### Error de permisos en GitHub Packages
```bash
# Configurar autenticación para GitHub Packages localmente
dotnet nuget add source --username TU_USUARIO --password TU_PAT --store-password-in-clear-text --name github "https://nuget.pkg.github.com/TU_USUARIO/index.json"
```

## 📚 Recursos Adicionales

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [NuGet Package Publishing](https://docs.microsoft.com/en-us/nuget/create-packages/publish-a-package)
- [GitHub Packages for .NET](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)
- [SonarCloud Integration](https://sonarcloud.io/documentation/)

## 🎯 Checklist Final

Antes de considerar la configuración completa, verifica:

- [ ] Repositorio creado en GitHub
- [ ] Secrets configurados (`NUGET_API_KEY`)
- [ ] Environments configurados (`preview`, `production`)
- [ ] Branch protection rules activas
- [ ] URLs actualizadas en `AppCore.csproj`
- [ ] Primer push exitoso
- [ ] Pipeline CI/CD ejecutándose correctamente
- [ ] Artifacts generados correctamente
- [ ] Tests pasando (Unit tests y SpecFlow)
- [ ] Cobertura de código >= 80%

---

## 📞 Soporte

Si encuentras problemas durante la configuración:

1. Revisa los logs del pipeline en la pestaña "Actions"
2. Verifica que todos los secrets estén configurados
3. Consulta la documentación de GitHub Actions
4. Revisa este documento para pasos faltantes

**¡Tu pipeline CI/CD está listo para funcionar! 🚀**