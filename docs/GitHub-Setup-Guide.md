# Guía de Configuración de GitHub para AppCore

Esta guía proporciona los pasos detallados para configurar el proyecto AppCore en GitHub y asegurar que el pipeline CI/CD funcione correctamente.

## 📋 Requisitos Previos

- Cuenta de GitHub
- Git instalado localmente
- .NET 10.0.x SDK instalado
- **✅ GitHub Packages** (repositorio NuGet público oficial)
- **✅ GitHub Packages** (repositorio para publicación)
- **✅ NuGet.org** (fuente para dependencias públicas)

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

#### ✅ GitHub Packages (Repositorio Oficial - Automático)
- `GITHUB_TOKEN` se genera automáticamente por GitHub Actions
- **No requiere configuración manual**
- Tiene permisos para publicar en GitHub Packages
- Es el método **oficial y recomendado** para AppCore

#### ✅ NuGet.org (Fuente de Dependencias Públicas)
**IMPORTANTE:** Aunque AppCore se publica en GitHub Packages, **NuGet.org sigue siendo necesario** para descargar dependencias públicas (como `System.Text.Json`, `MediatR`, etc.), a menos que tengas configurado un proxy en tu feed privado.

Configura `nuget.config` para incluir ambas fuentes. No se requiere API Key de NuGet.org para el pipeline actual (ya que no publicamos allí).

### 2.3 Configurar Permisos de GitHub Packages

Para que el pipeline pueda publicar en GitHub Packages:

1. Ve a tu repositorio en GitHub
2. **Settings** > **Actions** > **General**
3. En la sección **"Workflow permissions"**:
   - ✅ Selecciona **"Read and write permissions"**
   - ✅ Marca **"Allow GitHub Actions to create and approve pull requests"**
4. Haz clic en **"Save"**

### 2.4 Configurar Visibilidad del Paquete (Público)

Para que el paquete sea accesible públicamente desde GitHub Packages:

1. Una vez publicado el primer paquete, ve a la página principal de tu repositorio
2. En la barra lateral derecha, busca la sección **"Packages"**
3. Haz clic en el paquete **AppCore**
4. Ve a **"Package settings"**
5. En la sección **"Danger Zone"** > **"Change package visibility"**:
   - Selecciona **"Public"**
   - Confirma el cambio

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

**Resultado:** El paquete se publica automáticamente en GitHub Packages con la versión del tag.

## 📦 Paso 10: Consumir el Paquete desde GitHub Packages

### 10.1 Configurar NuGet Source (Una vez por máquina)

Para consumir paquetes públicos de GitHub Packages, configura el source en tu sistema:

```bash
# Agregar GitHub Packages como source de NuGet
dotnet nuget add source https://nuget.pkg.github.com/TU_USUARIO/index.json \
  --name github \
  --username TU_USUARIO \
  --password TU_GITHUB_PAT \
  --store-password-in-clear-text
```

**Nota:** Necesitas un Personal Access Token (PAT) de GitHub con scope `read:packages`.

#### Crear Personal Access Token

1. Ve a GitHub > **Settings** > **Developer settings** > **Personal access tokens** > **Tokens (classic)**
2. Haz clic en **"Generate new token"** > **"Generate new token (classic)"**
3. Configuración:
   - **Note:** `AppCore Package Read`
   - **Expiration:** 90 días (o más)
   - **Scopes:** ✅ `read:packages`
4. Copia el token generado

### 10.2 Configurar en Proyecto (nuget.config)

Alternativamente, puedes configurar por proyecto creando un `nuget.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="github" value="https://nuget.pkg.github.com/TU_USUARIO/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="TU_USUARIO" />
      <add key="ClearTextPassword" value="TU_GITHUB_PAT" />
    </github>
  </packageSourceCredentials>
</configuration>
```

### 10.3 Instalar el Paquete

```bash
# Instalar AppCore desde GitHub Packages
dotnet add package AppCore --version 1.0.0 --source github

# O editar manualmente el .csproj
```

```xml
<ItemGroup>
  <PackageReference Include="AppCore" Version="1.0.0" />
</ItemGroup>
```

### 10.4 Usar en Docker / CI/CD

Para usar en ambientes de CI/CD o Docker:

```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Configurar GitHub Packages
ARG GITHUB_PAT
RUN dotnet nuget add source https://nuget.pkg.github.com/TU_USUARIO/index.json \
    --name github \
    --username TU_USUARIO \
    --password ${GITHUB_PAT} \
    --store-password-in-clear-text

# Restaurar y compilar
COPY . .
RUN dotnet restore
RUN dotnet build
```

```yaml
# GitHub Actions
- name: Restore packages
  run: dotnet restore
  env:
    NUGET_AUTH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

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

### Error: "Unable to load the service index for source" (GitHub Packages)
**Causa:** Autenticación incorrecta o token sin permisos.

**Solución:**
1. Verifica que tu PAT tenga el scope `read:packages`
2. Asegúrate de usar el username correcto de GitHub
3. Regenera el PAT si es necesario

```bash
# Listar sources configurados
dotnet nuget list source

# Remover y re-agregar el source
dotnet nuget remove source github
dotnet nuget add source https://nuget.pkg.github.com/TU_USUARIO/index.json \
  --name github \
  --username TU_USUARIO \
  --password NUEVO_PAT \
  --store-password-in-clear-text
```

### Error: "Package 'AppCore' is not found"
**Causa:** El paquete no está marcado como público en GitHub Packages.

**Solución:**
1. Ve a GitHub > tu repositorio > **Packages**
2. Selecciona el paquete **AppCore**
3. **Package settings** > **Change package visibility** > **Public**
4. Confirma el cambio

### Error de permisos en GitHub Actions para publicar
**Causa:** Workflow permissions no están configurados correctamente.

**Solución:**
1. Ve a **Settings** > **Actions** > **General**
2. En **"Workflow permissions"**, selecciona **"Read and write permissions"**
3. Guarda los cambios y re-ejecuta el workflow

## 📚 Recursos Adicionales

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [GitHub Packages for .NET](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry)
- [NuGet Package Publishing](https://docs.microsoft.com/en-us/nuget/create-packages/publish-a-package)
- [Personal Access Tokens](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/creating-a-personal-access-token)
- [NativeAOT Compatibility](./NativeAOT-Compatibility-Report.md)

## 🎯 Checklist Final

Antes de considerar la configuración completa, verifica:

- [ ] Repositorio creado en GitHub
- [ ] **Workflow permissions** configurados (Read and write)
- [ ] Environments configurados (`preview`, `production`)
- [ ] Branch protection rules activas
- [ ] URLs actualizadas en `AppCore.csproj`
- [ ] Primer push exitoso
- [ ] Pipeline CI/CD ejecutándose correctamente
- [ ] Paquete publicado en **GitHub Packages**
- [ ] **Package visibility** configurada como **Public**
- [ ] Tests pasando (Unit tests y SpecFlow)
- [ ] NativeAOT compilation exitosa
- [ ] Cobertura de código >= 80%
- [ ] Consumers pueden instalar el paquete desde GitHub Packages

---

## 📞 Soporte

Si encuentras problemas durante la configuración:

1. Revisa los logs del pipeline en la pestaña "Actions"
2. Verifica que todos los secrets estén configurados
3. Consulta la documentación de GitHub Actions
4. Revisa este documento para pasos faltantes

**¡Tu pipeline CI/CD está listo para funcionar! 🚀**