using Microsoft.Data.Sqlite;

namespace ConexionADatos
{
    // La clase Login maneja la funcionalidad de inicio de sesión y registro de usuarios
    public partial class Login : ContentPage
    {
        // Cadena de conexión a la base de datos
        private static readonly string DbPath = Path.Combine(FileSystem.AppDataDirectory, "empleados.db");
        private static readonly string ConnectionString = $"Data Source={DbPath}";

        // Constructor de la página que inicializa los componentes y crea la base de datos si no existe
        public Login()
        {
            InitializeComponent();
            CreateTables();
        }

        private void CreateTables()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                string createUsuarioTable = @"
                    CREATE TABLE IF NOT EXISTS Usuario (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        User TEXT NOT NULL,
                        Password TEXT NOT NULL
                    )";

                ExecuteQuery(createUsuarioTable, connection);
            }
        }

        private void ExecuteQuery(string query, SqliteConnection connection)
        {
            using (var command = new SqliteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        // Método que maneja el evento de login, validando el usuario y la contraseña
        private async void LoginButton(object sender, EventArgs e)
        {
            bool credencialesValidas = false;

            // Verificar si los campos de usuario y contraseña no están vacíos
            if (usuarioLogin.Text != "" && passwordLogin.Text != "")
            {
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT Password FROM Usuario
                        WHERE User = @Usuario;";

                    using (var command = new SqliteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Usuario", usuarioLogin.Text);
                        using (var reader = command.ExecuteReader())
                        {
                            // Verificar si el usuario existe
                            if (reader.Read())
                            {
                                string passwordAlmacenada = reader["Password"]?.ToString();

                                // Validar si la contraseña ingresada coincide con la almacenada
                                if (passwordAlmacenada == HashPassword(passwordLogin.Text))
                                {
                                    credencialesValidas = true;
                                }
                            }
                        }
                    }

                    connection.Close();
                }

                
                if (credencialesValidas)
                {
                    await Navigation.PushAsync(new MainPage());
                }
                else
                {
                    await DisplayAlert("Error", "Nombre de usuario o contraseña incorrectos", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Los campos Usuario o Contraseña no pueden estar vacíos", "OK");
            }
        }

        // Método para hashear la contraseña antes de almacenarla o compararla
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                // Generar el hash de la contraseña
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // Método para mostrar el formulario de registro y ocultar el de login
        private void onRegistroMenuClick(object sender, EventArgs e)
        {
            MenuLogin.IsVisible = false;
            MenuRegistro.IsVisible = true;
        }

        // Método para mostrar el formulario de login y ocultar el de registro
        private void onLoginMenuClick(object sender, EventArgs e)
        {
            MenuLogin.IsVisible = true;
            MenuRegistro.IsVisible = false;
        }

        // Método para registrar un nuevo usuario, validando las contraseñas
        private async void RegisterButton(object sender, EventArgs e)
        {
            // Verificar que las contraseñas coincidan
            if (passwordRegistro.Text == confirmpassword.Text)
            {
                // Usar 'using' para manejar la conexión a la base de datos
                using (var connection = new SqliteConnection(ConnectionString))
                {
                    connection.Open();

                    // Consulta SQL para insertar un nuevo usuario
                    string insertQuery = @"
                        INSERT INTO Usuario (User, Password)
                        VALUES (@usuario, @password);";

                    using (var command = new SqliteCommand(insertQuery, connection))
                    {
                        // Agregar los parámetros a la consulta
                        command.Parameters.AddWithValue("@usuario", usuarioRegistro.Text);
                        command.Parameters.AddWithValue("@password", HashPassword(passwordRegistro.Text));

                        // Ejecutar la consulta
                        command.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                // Navegar a la pantalla de login después de registrar el usuario
                await Navigation.PushAsync(new Login());
            }
            else
            {
                await DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
            }
        }
    }
}
