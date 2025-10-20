using System;
using System.IO;
using System.Text;
using Microsoft.Data.SqlClient;

namespace CSVImporter
    {
        class Program
        {
            // CONFIGURACIÓN
            static string connectionString = "Server=DESKTOP-7AO57SE\\SQLEXPRESS;Database=PortafolioDB;Integrated Security=true;TrustServerCertificate=true;";
            static string csvFilePath = @"C:\proyectos\importadorCSV\productos.csv";
            static string logFilePath = @"C:\proyectos\importadorCSV\importacion_log.txt";

            // Contadores para el resumen
            static int registrosNuevos = 0;
            static int registrosActualizados = 0;
            static int registrosIgnorados = 0;
            static int registrosConError = 0;

            static void Main(string[] args)
            {
                // Iniciar el archivo de log
                IniciarLog();

                Console.WriteLine("===========================================");
                Console.WriteLine("  IMPORTADOR CSV A SQL SERVER v2.0");
                Console.WriteLine("  Con validación de duplicados y logging");
                Console.WriteLine("===========================================\n");

                EscribirLog("=== INICIO DE IMPORTACIÓN ===");
                EscribirLog($"Fecha y hora: {DateTime.Now}");
                EscribirLog($"Archivo CSV: {csvFilePath}");

                try
                {
                    // Verificar que el archivo CSV existe
                    if (!File.Exists(csvFilePath))
                    {
                        string error = $"ERROR: No se encontró el archivo: {csvFilePath}";
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"❌ {error}");
                        Console.ResetColor();
                        EscribirLog(error);
                        Console.WriteLine("\nPresiona cualquier tecla para salir...");
                        Console.ReadKey();
                        return;
                    }

                    Console.WriteLine($"📄 Archivo encontrado: {csvFilePath}");
                    EscribirLog("✓ Archivo CSV encontrado");

                    Console.WriteLine($"📊 Conectando a la base de datos...\n");

                    // Probar conexión
                    using (SqlConnection testConnection = new SqlConnection(connectionString))
                    {
                        testConnection.Open();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("✓ Conexión exitosa a la base de datos");
                        Console.ResetColor();
                        EscribirLog("✓ Conexión a base de datos exitosa");
                    }

                    // Leer el archivo CSV
                    string[] lines = File.ReadAllLines(csvFilePath);

                    if (lines.Length <= 1)
                    {
                        string advertencia = "ADVERTENCIA: El archivo CSV está vacío o solo tiene encabezados";
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"⚠ {advertencia}");
                        Console.ResetColor();
                        EscribirLog(advertencia);
                        Console.WriteLine("\nPresiona cualquier tecla para salir...");
                        Console.ReadKey();
                        return;
                    }

                    Console.WriteLine($"\n📋 Total de registros a procesar: {lines.Length - 1}");
                    EscribirLog($"Total de registros en CSV: {lines.Length - 1}");
                    Console.WriteLine("\n▶ Iniciando importación con validación de duplicados...\n");

                    // Procesar cada línea (saltando el encabezado)
                    for (int i = 1; i < lines.Length; i++)
                    {
                        string line = lines[i];

                        try
                        {
                            // Dividir la línea por comas
                            string[] campos = line.Split(',');

                            if (campos.Length != 5)
                            {
                                throw new Exception($"La línea no tiene 5 campos esperados");
                            }

                            // Extraer los valores
                            string codigo = campos[0].Trim();
                            string nombre = campos[1].Trim();
                            decimal precio = decimal.Parse(campos[2].Trim());
                            int stock = int.Parse(campos[3].Trim());
                            string categoria = campos[4].Trim();

                            // Validar si el producto ya existe
                            ProcesarProducto(codigo, nombre, precio, stock, categoria, i);
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = $"Error en línea {i}: {ex.Message}";
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"✗ {errorMsg}");
                            Console.ResetColor();
                            EscribirLog($"ERROR - {errorMsg}");
                            registrosConError++;
                        }
                    }

                    // Resumen final
                    MostrarResumen();
                }
                catch (Exception ex)
                {
                    string errorCritico = $"ERROR CRÍTICO: {ex.Message}";
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n❌ {errorCritico}");
                    Console.ResetColor();
                    EscribirLog(errorCritico);
                }

                EscribirLog("=== FIN DE IMPORTACIÓN ===\n");
                Console.WriteLine($"\n📄 Log guardado en: {logFilePath}");
                Console.WriteLine("\nPresiona cualquier tecla para salir...");
                Console.ReadKey();
            }

            static void ProcesarProducto(string codigo, string nombre, decimal precio, int stock, string categoria, int numeroLinea)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Verificar si el producto existe
                    string queryVerificar = "SELECT Id, Precio FROM Productos WHERE Codigo = @Codigo";
                    using (SqlCommand cmdVerificar = new SqlCommand(queryVerificar, connection))
                    {
                        cmdVerificar.Parameters.AddWithValue("@Codigo", codigo);

                        using (SqlDataReader reader = cmdVerificar.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // El producto YA EXISTE
                                int productoId = reader.GetInt32(0);
                                decimal precioActual = reader.GetDecimal(1);
                                reader.Close();

                                // Comparar precios
                                if (precio > precioActual)
                                {
                                    // El precio nuevo es MAYOR - Actualizar
                                    ActualizarProducto(connection, productoId, codigo, nombre, precio, stock, categoria, precioActual);
                                }
                                else
                                {
                                    // El precio nuevo es MENOR o IGUAL - Ignorar
                                    string msg = $"Registro {numeroLinea}: {codigo} - Precio actual (${precioActual}) es mayor o igual. NO se actualiza.";
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine($"⊘ {msg}");
                                    Console.ResetColor();
                                    EscribirLog($"IGNORADO - {msg}");
                                    registrosIgnorados++;
                                }
                            }
                            else
                            {
                                // El producto NO EXISTE - Insertar
                                reader.Close();
                                InsertarProducto(connection, codigo, nombre, precio, stock, categoria, numeroLinea);
                            }
                        }
                    }
                }
            }

            static void InsertarProducto(SqlConnection connection, string codigo, string nombre, decimal precio, int stock, string categoria, int numeroLinea)
            {
                string queryInsertar = @"INSERT INTO Productos (Codigo, Nombre, Precio, Stock, Categoria) 
                                    VALUES (@Codigo, @Nombre, @Precio, @Stock, @Categoria)";

                using (SqlCommand cmdInsertar = new SqlCommand(queryInsertar, connection))
                {
                    cmdInsertar.Parameters.AddWithValue("@Codigo", codigo);
                    cmdInsertar.Parameters.AddWithValue("@Nombre", nombre);
                    cmdInsertar.Parameters.AddWithValue("@Precio", precio);
                    cmdInsertar.Parameters.AddWithValue("@Stock", stock);
                    cmdInsertar.Parameters.AddWithValue("@Categoria", categoria);

                    cmdInsertar.ExecuteNonQuery();
                }

                string msg = $"Registro {numeroLinea}: {codigo} - {nombre} (${precio}) insertado correctamente";
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ {msg}");
                Console.ResetColor();
                EscribirLog($"NUEVO - {msg}");
                registrosNuevos++;
            }

            static void ActualizarProducto(SqlConnection connection, int id, string codigo, string nombre, decimal precioNuevo, int stock, string categoria, decimal precioAnterior)
            {
                string queryActualizar = @"UPDATE Productos 
                                      SET Nombre = @Nombre, 
                                          Precio = @Precio, 
                                          Stock = @Stock, 
                                          Categoria = @Categoria,
                                          FechaImportacion = GETDATE()
                                      WHERE Id = @Id";

                using (SqlCommand cmdActualizar = new SqlCommand(queryActualizar, connection))
                {
                    cmdActualizar.Parameters.AddWithValue("@Id", id);
                    cmdActualizar.Parameters.AddWithValue("@Nombre", nombre);
                    cmdActualizar.Parameters.AddWithValue("@Precio", precioNuevo);
                    cmdActualizar.Parameters.AddWithValue("@Stock", stock);
                    cmdActualizar.Parameters.AddWithValue("@Categoria", categoria);

                    cmdActualizar.ExecuteNonQuery();
                }

                string msg = $"{codigo} - Precio actualizado: ${precioAnterior} → ${precioNuevo}";
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"↑ {msg}");
                Console.ResetColor();
                EscribirLog($"ACTUALIZADO - {msg}");
                registrosActualizados++;
            }

            static void IniciarLog()
            {
                try
                {
                    // Crear o limpiar el archivo de log
                    File.WriteAllText(logFilePath, "");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Advertencia: No se pudo crear el archivo de log: {ex.Message}");
                }
            }

            static void EscribirLog(string mensaje)
            {
                try
                {
                    string lineaLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {mensaje}";
                    File.AppendAllText(logFilePath, lineaLog + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Advertencia: No se pudo escribir en el log: {ex.Message}");
                }
            }

            static void MostrarResumen()
            {
                Console.WriteLine("\n===========================================");
                Console.WriteLine("  RESUMEN DE IMPORTACIÓN");
                Console.WriteLine("===========================================");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✓ Productos nuevos insertados: {registrosNuevos}");
                Console.ResetColor();

                if (registrosActualizados > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"↑ Productos actualizados (precio mayor): {registrosActualizados}");
                    Console.ResetColor();
                }

                if (registrosIgnorados > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"⊘ Productos ignorados (precio menor/igual): {registrosIgnorados}");
                    Console.ResetColor();
                }

                if (registrosConError > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"✗ Registros con error: {registrosConError}");
                    Console.ResetColor();
                }

                Console.WriteLine("===========================================");

                // Escribir resumen en el log
                EscribirLog("--- RESUMEN ---");
                EscribirLog($"Productos nuevos: {registrosNuevos}");
                EscribirLog($"Productos actualizados: {registrosActualizados}");
                EscribirLog($"Productos ignorados: {registrosIgnorados}");
                EscribirLog($"Registros con error: {registrosConError}");
            }
        }
    }