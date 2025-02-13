using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;

namespace ConexionADatos
{
    internal class BBDD
    {
        // Ruta de la base de datos almacenada en el directorio de datos de la aplicación
        private static readonly string DbPath = Path.Combine(FileSystem.AppDataDirectory, "empleados.db");

        // Cadena de conexión que apunta a la base de datos
        private static readonly string ConnectionString = $"Data Source={DbPath}";

        // Constructor de la clase que crea las tablas de la base de datos
        public BBDD()
        {
            CreateTables();  // Llama al método para crear las tablas si no existen
            //InitializeDepartamentos();  // Puede usarse para agregar datos predeterminados de departamentos
            //InitializeEmpleados();  // Puede usarse para agregar datos predeterminados de empleados
        }

        // Método para crear las tablas en la base de datos si no existen
        private void CreateTables()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                // Definición de la tabla Usuario
                string createUsuarioTable = @"
                    CREATE TABLE IF NOT EXISTS Usuario (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        User TEXT NOT NULL,
                        Password TEXT NOT NULL
                    )";

                // Definición de la tabla Departamento
                string createDepartamentosTable = @"
                    CREATE TABLE IF NOT EXISTS Departamento (
                        DeptNo INTEGER PRIMARY KEY AUTOINCREMENT,
                        Nombre TEXT NOT NULL,
                        Localizacion TEXT NOT NULL
                    )";

                // Definición de la tabla Empleado
                string createEmpleadosTable = @"
                    CREATE TABLE IF NOT EXISTS Empleado (
                        EMP_NO INTEGER PRIMARY KEY AUTOINCREMENT,
                        Apellido TEXT NOT NULL,
                        Oficio TEXT NOT NULL,
                        Salario REAL NOT NULL,
                        Comision REAL,
                        FechaAlta TEXT NOT NULL,
                        DEPT_NO INTEGER,
                        FOREIGN KEY (DEPT_NO) REFERENCES Departamento(DeptNo) ON DELETE SET NULL
                    )";

                // Ejecuta las consultas SQL para crear las tablas
                ExecuteQuery(createUsuarioTable, connection);
                ExecuteQuery(createDepartamentosTable, connection);
                ExecuteQuery(createEmpleadosTable, connection);
            }
        }

        // Método para insertar departamentos predeterminados si no existen
        private void InitializeDepartamentos()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                // Verifica si la tabla Departamento tiene datos
                var checkQuery = "SELECT COUNT(*) FROM Departamento";
                using (var command = new SqliteCommand(checkQuery, connection))
                {
                    var count = Convert.ToInt32(command.ExecuteScalar());

                    // Si la tabla está vacía, inserta algunos departamentos predeterminados
                    if (count == 0)
                    {
                        string insertDepartamento1 = "INSERT INTO Departamento (Nombre, Localizacion) VALUES ('Recursos Humanos', 'Madrid')";
                        string insertDepartamento2 = "INSERT INTO Departamento (Nombre, Localizacion) VALUES ('Tecnología', 'Barcelona')";
                        string insertDepartamento3 = "INSERT INTO Departamento (Nombre, Localizacion) VALUES ('Marketing', 'Valencia')";

                        ExecuteQuery(insertDepartamento1, connection);
                        ExecuteQuery(insertDepartamento2, connection);
                        ExecuteQuery(insertDepartamento3, connection);
                    }
                }
            }
        }

        // Método para insertar un nuevo empleado en la base de datos
        public void InsertarEmpleado(string apellido, string oficio, decimal salario, decimal? comision, DateTime fechaAlta, int deptNo)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                // Consulta SQL para insertar un empleado
                var query = @"
                INSERT INTO Empleado (Apellido, Oficio, Salario, Comision, FechaAlta, DEPT_NO)
                VALUES (@Apellido, @Oficio, @Salario, @Comision, @FechaAlta, @DeptNo)";

                using (var command = new SqliteCommand(query, connection))
                {
                    // Agregar los parámetros a la consulta SQL
                    command.Parameters.AddWithValue("@Apellido", apellido);
                    command.Parameters.AddWithValue("@Oficio", oficio);
                    command.Parameters.AddWithValue("@Salario", salario);
                    command.Parameters.AddWithValue("@Comision", comision ?? (object)DBNull.Value);  // Si la comisión es nula, usamos DBNull
                    command.Parameters.AddWithValue("@FechaAlta", fechaAlta.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@DeptNo", deptNo);

                    // Ejecuta la consulta
                    command.ExecuteNonQuery();
                }
            }
        }

        // Método para actualizar los datos de un empleado existente
        public void ActualizarEmpleado(string id, string apellido, string oficio, decimal salario, decimal? comision, DateTime fechaAlta, int deptNo)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                // Consulta SQL para actualizar los datos del empleado
                var query = @"
                    UPDATE Empleado
                    SET 
                        Apellido = @Apellido,
                        Oficio = @Oficio,
                        Salario = @Salario,
                        Comision = @Comision,
                        FechaAlta = @FechaAlta,
                        DEPT_NO = @DeptNo
                    WHERE EMP_NO = @Id";

                using (var command = new SqliteCommand(query, connection))
                {
                    // Agregar los parámetros a la consulta SQL
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Apellido", apellido);
                    command.Parameters.AddWithValue("@Oficio", oficio);
                    command.Parameters.AddWithValue("@Salario", salario);
                    command.Parameters.AddWithValue("@Comision", comision ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@FechaAlta", fechaAlta.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@DeptNo", deptNo);

                    // Ejecuta la consulta
                    command.ExecuteNonQuery();
                }
            }
        }

        // Método para borrar un empleado de la base de datos
        public void BorrarEmpleado(string id)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                // Consulta SQL para eliminar un empleado
                var query = @"
                    DELETE FROM Empleado
                    WHERE EMP_NO = @Id";

                using (var command = new SqliteCommand(query, connection))
                {
                    // Agregar el parámetro a la consulta SQL
                    command.Parameters.AddWithValue("@Id", id);

                    // Ejecuta la consulta
                    command.ExecuteNonQuery();
                }
            }
        }

        // Método para obtener todos los departamentos de la base de datos
        public List<Departamento> GetDepartamentos()
        {
            var departamentos = new List<Departamento>();

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var query = "SELECT DeptNo, Nombre, Localizacion FROM Departamento";
                using (var command = new SqliteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Agrega cada departamento a la lista
                            departamentos.Add(new Departamento
                            {
                                DeptNo = reader.GetInt32(0),
                                Nombre = reader.GetString(1),
                                Localizacion = reader.GetString(2)
                            });
                        }
                    }
                }
            }

            return departamentos;
        }

        // Método para obtener las localizaciones de un departamento por nombre
        public List<Localizacion> GetLocalizaciones(string Nombre)
        {
            var localizaciones = new List<Localizacion>();

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                // Consulta SQL para obtener las localizaciones de un departamento
                var query = "SELECT DeptNo, Localizacion FROM Departamento WHERE Nombre = @Nombre";

                using (var command = new SqliteCommand(query, connection))
                {
                    // Se añade el parámetro para el nombre del departamento
                    command.Parameters.AddWithValue("@Nombre", Nombre);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Agrega cada localización a la lista
                            localizaciones.Add(new Localizacion
                            {
                                DeptNo = reader.GetInt32(0),
                                Nombre = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            return localizaciones;
        }

        // Método para obtener los empleados de un departamento por su número de departamento
        public List<Empleado> GetEmpleadosByLocalidad(int deptNo)
        {
            var empleados = new List<Empleado>();

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var query = "SELECT EMP_NO, Apellido, Oficio, Salario, Comision, DATE(FechaAlta) FROM Empleado WHERE DEPT_NO = @DeptNo";
                using (var command = new SqliteCommand(query, connection))
                {
                    // Añadir el parámetro de Departamento
                    command.Parameters.AddWithValue("@DeptNo", deptNo);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Agregar cada empleado a la lista
                            empleados.Add(new Empleado
                            {
                                EMP_NO = reader.GetInt32(0),
                                Apellido = reader.GetString(1),
                                Oficio = reader.GetString(2),
                                Salario = reader.GetDecimal(3),
                                Comision = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                                FechaAlta = DateTime.Parse(reader.GetString(5)).Date
                            });
                        }
                    }
                }
            }

            return empleados;
        }

        // Método para obtener empleados por un valor específico en una columna
        public List<Empleado> GetEmpleadosByValores(string campo, string valor)
        {
            var empleados = new List<Empleado>();

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                var query = $"SELECT EMP_NO, Apellido, Oficio, Salario, Comision, DATE(FechaAlta) FROM Empleado WHERE {campo} = @valor";
                using (var command = new SqliteCommand(query, connection))
                {
                    // Agregar el parámetro para el valor buscado
                    command.Parameters.AddWithValue("@valor", valor);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Agregar cada empleado que coincida con la búsqueda
                            empleados.Add(new Empleado
                            {
                                EMP_NO = reader.GetInt32(0),
                                Apellido = reader.GetString(1),
                                Oficio = reader.GetString(2),
                                Salario = reader.GetDecimal(3),
                                Comision = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                                FechaAlta = DateTime.Parse(reader.GetString(5)).Date
                            });
                        }
                    }
                }
            }

            return empleados;
        }

        // Método para obtener valores únicos de una columna en la tabla Empleado
        public List<string> ObtenerValoresColumna(string columna)
        {
            List<string> valores = new List<string>();

            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                string query = $"SELECT DISTINCT {columna} FROM Empleado"; // Consulta SQL para obtener valores únicos

                using (var command = new SqliteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            valores.Add(reader[0]?.ToString() ?? "");  // Agregar valores a la lista
                        }
                    }
                }

                connection.Close();
            }

            return valores.Where(v => !string.IsNullOrEmpty(v)).ToList(); // Filtrar valores vacíos
        }

        // Ejecuta una consulta SQL no vinculada a resultados, como INSERT, UPDATE o DELETE
        private void ExecuteQuery(string query, SqliteConnection connection)
        {
            using (var command = new SqliteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    // Definición de la clase Departamento
    public class Departamento
    {
        public int DeptNo { get; set; }
        public string Nombre { get; set; }
        public string Localizacion { get; set; }
    }

    // Definición de la clase Localizacion
    public class Localizacion
    {
        public int DeptNo { get; set; }
        public string Nombre { get; set; }
    }

    // Definición de la clase Empleado
    public class Empleado
    {
        public int EMP_NO { get; set; }
        public string Apellido { get; set; }
        public string Oficio { get; set; }
        public decimal Salario { get; set; }
        public decimal? Comision { get; set; }
        public DateTime FechaAlta { get; set; }
    }
}
