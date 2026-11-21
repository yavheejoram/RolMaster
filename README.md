# RolMaster Windows 1.0 — revisión de integración

Proyecto C# WPF (.NET 10) y SQLite, sin Flutter.

## Cambios de esta revisión
- Corregida la colocación del editor de registros en la segunda columna de la ventana.
- Editor de personajes con raza, clase, nivel, atributos, inventario, vínculos e historial mediante controles WPF (en lugar de editar JSON manualmente).
- Cuadrícula del mapa con dimensiones fijas para permitir desplazamiento en el lienzo.
- GitHub Actions verifica archivos, restaura paquetes, compila y publica un ejecutable autónomo para Windows x64.

## Compilar
En Windows 10/11 con .NET 10 SDK:

```powershell
dotnet restore RolMaster.csproj
dotnet build RolMaster.csproj -c Release
dotnet publish RolMaster.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

Alternativa: subir los archivos a GitHub y ejecutar el flujo `Compilar RolMaster Windows 1.0` en Actions. Descargar el artefacto al terminar.

## Limitaciones conocidas
No se ha ejecutado la compilación ni las pruebas Windows en este entorno. El editor de personajes usa un modelo básico común, no reproduce las reglas completas de cada juego. Los vínculos se almacenan como texto y no como referencias verificadas. La exportación de copias solo incluye registros JSON y no imágenes. El editor de mapas es una cuadrícula simple sin edificios ni capas privadas. Antes de probar importaciones, exporta una copia de tus datos.

Los datos se almacenan en `%LOCALAPPDATA%\RolMaster\rolmaster.db`.
