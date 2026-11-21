# Compilar RolMaster Windows 1.0

1. Crea un repositorio de GitHub y sube **el contenido** de esta carpeta a la raíz, incluyendo `.github/workflows/windows.yml`.
2. En GitHub abre **Actions** y selecciona **Compilar RolMaster Windows 1.0**.
3. Pulsa **Run workflow**. Espera a que termine.
4. Si el trabajo es correcto, abre su ejecución y descarga el artefacto **RolMaster-Windows-1.0-win-x64**.
5. Descomprime el artefacto y ejecuta `RolMaster.exe` en Windows 10/11 de 64 bits.

**Si falla:** abre el paso marcado en rojo y copia el mensaje de error completo para corregirlo. El paquete fuente no contiene un EXE ya compilado. GitHub Actions necesita acceso a Internet para obtener .NET y NuGet. El ejecutable generado es una compilación de prueba sin firma digital; Windows SmartScreen podría mostrar un aviso.

Los datos se almacenan en `%LOCALAPPDATA%\RolMaster\rolmaster.db` y no se incluyen en la carpeta del ejecutable. Haz una copia de seguridad antes de actualizar.
