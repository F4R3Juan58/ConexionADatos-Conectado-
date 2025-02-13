using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.Maui.Controls;

namespace ConexionADatos
{
    public partial class MainPage : ContentPage
    {
        private readonly BBDD databaseService;  // Servicio para interactuar con la base de datos
        private List<Departamento> departamentos;  // Lista de departamentos
        private List<Empleado> empleados;  // Lista de empleados
        private List<Localizacion> localizacion;  // Lista de localizaciones
        int DeptNo = 0;  // Número del departamento seleccionado

        public MainPage()
        {
            InitializeComponent();  // Inicializa los componentes de la página
            databaseService = new BBDD();  // Crea una instancia del servicio de base de datos
            CargarDepartamentos();  // Carga los departamentos iniciales
        }

        // Método para cargar los departamentos desde la base de datos
        private void CargarDepartamentos()
        {
            empleadosListView.ItemsSource = null;  // Limpia la lista de empleados
            departamentos = databaseService.GetDepartamentos();  // Obtiene la lista de departamentos
            departamentosListView.ItemsSource = departamentos;  // Asigna la lista de departamentos a la vista
        }

        // Maneja la selección de un departamento
        private void OnDepartamentoSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var departamentoSeleccionado = e.SelectedItem as Departamento;  // Obtiene el departamento seleccionado
            if (departamentoSeleccionado != null)
            {
                localizacion = databaseService.GetLocalizaciones(departamentoSeleccionado.Nombre);  // Obtiene las localizaciones del departamento seleccionado
                localizacionesListView.ItemsSource = localizacion;  // Asigna las localizaciones a la vista
            }
        }

        // Maneja la selección de una localización
        private void OnLocalizacionSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var LocalizacionSeleccionado = e.SelectedItem as Localizacion;  // Obtiene la localización seleccionada
            if (LocalizacionSeleccionado != null)
            {
                DeptNo = LocalizacionSeleccionado.DeptNo;  // Guarda el número del departamento de la localización seleccionada
                empleados = databaseService.GetEmpleadosByLocalidad(DeptNo);  // Obtiene los empleados de la localización seleccionada
                empleadosListView.ItemsSource = empleados;  // Asigna los empleados a la vista
            }
        }

        // Maneja la selección de un empleado
        private void onEmpleadoSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var empleadoSelecionado = e.SelectedItem as Empleado;  // Obtiene el empleado seleccionado
            if (empleadoSelecionado != null)
            {
                // Muestra los detalles del empleado en los campos de entrada
                Id.Text = empleadoSelecionado.EMP_NO.ToString();
                ApellidosEntry.Text = empleadoSelecionado.Apellido;
                OficiosEntry.Text = empleadoSelecionado.Oficio;
                SalariosEntry.Text = empleadoSelecionado.Salario.ToString();
                ComisionesEntry.Text = empleadoSelecionado.Comision?.ToString() ?? string.Empty;
                FechaEntry.Date = empleadoSelecionado.FechaAlta;
            }
        }

        // Realiza una búsqueda de empleados según el campo y valor seleccionados
        private void BuscarButton_Clicked(object sender, EventArgs e)
        {
            string campoSelecionado = buscarPorPicker.SelectedItem.ToString();  // Obtiene el campo de búsqueda seleccionado
            string valorSelecionado = buscarPicker.SelectedItem.ToString();  // Obtiene el valor de búsqueda seleccionado
            empleados = databaseService.GetEmpleadosByValores(campoSelecionado, valorSelecionado);  // Realiza la búsqueda en la base de datos
            empleadosListView.ItemsSource = empleados;  // Asigna los resultados de la búsqueda a la vista
        }

        // Guarda un nuevo empleado en la base de datos
        private void GuardarButton_Clicked(object sender, EventArgs e)
        {
            // Obtiene los valores introducidos por el usuario
            string apellido = ApellidosEntry.Text;
            string oficio = OficiosEntry.Text;
            decimal.TryParse(SalariosEntry.Text, out decimal salario);
            decimal.TryParse(ComisionesEntry.Text, out decimal comision);
            DateTime fecha = FechaEntry.Date;

            // Inserta el nuevo empleado en la base de datos
            databaseService.InsertarEmpleado(apellido, oficio, salario, comision, fecha, DeptNo);
            DisplayAlert("Usuarios", "El usuario ha sido añadido de forma correcta", "ok");  // Muestra un mensaje de éxito
            limpiar();  // Limpia los campos de entrada
            empleados = databaseService.GetEmpleadosByLocalidad(DeptNo);  // Recarga los empleados del departamento
            empleadosListView.ItemsSource = empleados;  // Asigna los empleados a la vista
        }

        // Actualiza los datos de un empleado en la base de datos
        private void ActualizarButton_Clicked(object sender, EventArgs e)
        {
            // Obtiene los valores introducidos por el usuario
            string id = Id.Text;
            string apellido = ApellidosEntry.Text;
            string oficio = OficiosEntry.Text;
            decimal.TryParse(SalariosEntry.Text, out decimal salario);
            decimal.TryParse(ComisionesEntry.Text, out decimal comision);
            DateTime fecha = FechaEntry.Date;

            // Actualiza los datos del empleado en la base de datos
            databaseService.ActualizarEmpleado(id, apellido, oficio, salario, comision, fecha, DeptNo);
            DisplayAlert("Usuarios", "El usuario ha sido actualizada de forma correcta", "ok");  // Muestra un mensaje de éxito
            limpiar();  // Limpia los campos de entrada
            empleados = databaseService.GetEmpleadosByLocalidad(DeptNo);  // Recarga los empleados del departamento
            empleadosListView.ItemsSource = empleados;  // Asigna los empleados a la vista
        }

        // Elimina un empleado de la base de datos
        private void BorrarButton_Clicked(object sender, EventArgs e)
        {
            DisplayAlert("Usuarios", "El usuario ha sido eliminada de forma correcta", "ok");  // Muestra un mensaje de éxito
            string id = Id.Text;
            databaseService.BorrarEmpleado(id);  // Elimina el empleado de la base de datos
            limpiar();  // Limpia los campos de entrada
            empleados = databaseService.GetEmpleadosByLocalidad(DeptNo);  // Recarga los empleados del departamento
            empleadosListView.ItemsSource = empleados;  // Asigna los empleados a la vista
        }

        // Limpia los campos de entrada
        private void LimpiarButton_Clicked(object sender, EventArgs e)
        {
            limpiar();  // Llama al método limpiar
        }

        // Método para limpiar los campos de entrada
        private void limpiar()
        {
            Id.Text = "";
            ApellidosEntry.Text = "";
            OficiosEntry.Text = "";
            SalariosEntry.Text = "";
            ComisionesEntry.Text = "";
            FechaEntry.Date = DateTime.Now;  // Restablece la fecha a la actual
        }

        // Actualiza los valores posibles en el campo de búsqueda según el campo seleccionado
        private void campoBuscar(object sender, EventArgs e)
        {
            string campoSelecionado = buscarPorPicker.SelectedItem.ToString();  // Obtiene el campo de búsqueda seleccionado
            List<string> valores = databaseService.ObtenerValoresColumna(campoSelecionado);  // Obtiene los valores posibles para ese campo
            buscarPicker.Items.Clear();  // Limpia los valores anteriores en el picker
            foreach (var valor in valores)
            {
                buscarPicker.Items.Add(valor);  // Añade los nuevos valores al picker
            }
        }
    }
}
