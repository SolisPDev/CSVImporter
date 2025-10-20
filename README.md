# 📊 CSV Importer - Importador de CSV a SQL Server

Aplicación de consola en C# .NET que importa datos desde archivos CSV a SQL Server con validación de duplicados, comparación de precios y sistema de logging completo.

## 🎯 Características

- ✅ **Importación masiva** de datos desde archivos CSV
- ✅ **Validación de duplicados** por código de producto
- ✅ **Comparación inteligente de precios** - Mantiene siempre el precio más alto
- ✅ **Sistema de logging completo** - Registro detallado de todas las operaciones
- ✅ **Manejo robusto de errores** - Continúa la importación aunque haya errores
- ✅ **Parámetros SQL seguros** - Protección contra SQL Injection
- ✅ **Resumen detallado** - Estadísticas de productos nuevos, actualizados e ignorados

## 🛠️ Tecnologías Utilizadas

- **C# .NET 6/8** - Lenguaje y framework principal
- **Microsoft.Data.SqlClient** - Conexión a SQL Server
- **SQL Server** - Base de datos
- **Visual Studio 2022** - IDE de desarrollo

## 📋 Requisitos Previos

- Visual Studio 2022 o superior
- .NET 6.0 SDK o superior
- SQL Server (LocalDB, Express o cualquier edición)
- Permisos de escritura en el sistema de archivos para logs

## 🚀 Instalación y Configuración

### 1. Clonar el repositorio
```bash
git clone https://github.com/SolisPDev/CSVImporter.git
cd CSVImporter
```

### 2. Crear la base de datos

Ejecuta este script en SQL Server Management Studio:
```sql
CREATE DATABASE PortfolioDB;
GO

USE PortfolioDB;
GO

CREATE TABLE Productos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(50) NOT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    Precio DECIMAL(18,2) NOT NULL,
    Stock INT NOT NULL,
    Categoria NVARCHAR(100),
    FechaImportacion DATETIME DEFAULT GETDATE()
);
GO
```

### 3. Configurar la cadena de conexión

Edita el archivo `Program.cs` y ajusta la cadena de conexión según tu servidor:
```csharp
static string connectionString = "Server=localhost;Database=PortfolioDB;Integrated Security=true;TrustServerCertificate=true;";
```

### 4. Preparar el archivo CSV

Crea un archivo CSV con el siguiente formato:
```csv
Codigo,Nombre,Precio,Stock,Categoria
PROD001,Laptop Dell Inspiron,15999.99,10,Electrónica
PROD002,Mouse Logitech,299.50,50,Accesorios
```

Guárdalo en: `C:\proyectos\productos.csv` o ajusta la ruta en `Program.cs`

### 5. Compilar y ejecutar
```bash
dotnet restore
dotnet build
dotnet run
```

O simplemente presiona **F5** en Visual Studio.

## 📖 Uso

1. Prepara tu archivo CSV con los productos
2. Ejecuta la aplicación
3. El programa procesará automáticamente el archivo y mostrará el progreso en consola
4. Revisa el resumen final y el archivo de log generado

### Lógica de Validación de Duplicados

- **Si el producto NO existe**: Se inserta como nuevo registro
- **Si el producto existe y el precio nuevo es MAYOR**: Se actualiza con el precio más alto
- **Si el producto existe y el precio nuevo es MENOR o IGUAL**: Se ignora (mantiene el precio actual)

## 📄 Archivo de Log

El sistema genera automáticamente un archivo `importacion_log.txt` que contiene:

- Fecha y hora de cada operación
- Productos insertados, actualizados e ignorados
- Errores encontrados durante el proceso
- Resumen estadístico final

Ejemplo de log:
```
[2025-10-20 14:30:15] === INICIO DE IMPORTACIÓN ===
[2025-10-20 14:30:15] ✓ Archivo CSV encontrado
[2025-10-20 14:30:16] NUEVO - Registro 1: PROD001 - Laptop Dell Inspiron ($15999.99) insertado correctamente
[2025-10-20 14:30:16] ACTUALIZADO - PROD002 - Precio actualizado: $299.50 → $399.00
```

## 🎨 Capturas de Pantalla

### Ejecución Exitosa
![image](https://github.com/user-attachments/assets/ejemplo-ejecucion.png)

### Validación de Duplicados
![image](https://github.com/user-attachments/assets/ejemplo-duplicados.png)

## 🔧 Personalización

Puedes personalizar fácilmente:

- **Ruta del CSV**: Modifica `csvFilePath` en `Program.cs`
- **Ruta del log**: Modifica `logFilePath` en `Program.cs`
- **Estructura de la tabla**: Ajusta el script SQL y el código según tus necesidades
- **Formato del CSV**: Adapta el método `Split()` para otros delimitadores

## 🐛 Solución de Problemas

### Error: "No se encontró el archivo CSV"
- Verifica que la ruta del archivo sea correcta
- Asegúrate de que el archivo tenga extensión `.csv`

### Error de conexión a SQL Server
- Verifica que SQL Server esté ejecutándose
- Confirma que la cadena de conexión sea correcta
- Verifica los permisos de usuario

### Error: "La línea no tiene 5 campos"
- Revisa que tu CSV tenga exactamente 5 columnas
- Verifica que no haya comas adicionales en los datos

## 📚 Aprendizajes y Mejores Prácticas

Este proyecto demuestra:

- ✅ Uso de **parámetros SQL** para prevenir SQL Injection
- ✅ Manejo apropiado de **conexiones** con `using` statements
- ✅ **Logging** profesional para debugging y auditoría
- ✅ **Validación de datos** antes de operaciones críticas
- ✅ **Separación de responsabilidades** en métodos específicos
- ✅ **Feedback visual** con colores en consola para mejor UX

## 🚀 Futuras Mejoras

- [ ] Soporte para múltiples formatos de archivos (Excel, JSON)
- [ ] Interfaz gráfica (WPF/WinForms)
- [ ] Configuración mediante archivo `appsettings.json`
- [ ] Procesamiento asíncrono para archivos grandes
- [ ] Exportación de reportes en PDF
- [ ] API REST para importación remota

## 👨‍💻 Autor

**Tu Nombre**
- LinkedIn: [tu-perfil](https://linkedin.com/in/antoniosolisp/)
- GitHub: [tu-usuario](https://github.com/SolisPDev)
- Email: tu-email@ejemplo.com

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para más detalles.

## 🙏 Agradecimientos

Proyecto desarrollado como parte de mi portafolio profesional de transición hacia tecnologías modernas C# .NET.

---

⭐ Si este proyecto te fue útil, no olvides darle una estrella en GitHub!
```

Guarda el archivo (Ctrl + S)

---

### **Parte B: Crear archivo .gitignore**

**Instrucciones:**

1. En Visual Studio, clic derecho en el proyecto → **Add** → **New Item**
2. Selecciona **Text File**
3. Nombre: `.gitignore`
4. Copia este contenido:
```
## Visual Studio
.vs/
bin/
obj/
*.user
*.suo
*.userosscache
*.sln.docstates

## Build results
[Dd]ebug/
[Rr]elease/
x64/
x86/
[Aa]rm/
[Aa]rm64/
bld/
[Bb]in/
[Oo]bj/

## Archivos locales (no subir al repositorio)
productos.csv
importacion_log.txt
*.log

## NuGet
packages/
*.nupkg