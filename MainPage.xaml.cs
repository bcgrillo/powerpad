using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ComponentModel;
using ST = System.Timers;
using System.Timers;

namespace PowerPad
{
    public partial class MainPage : ContentPage
    {
        public MainViewModel ViewModel { get; set; }

        public MainPage()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            BindingContext = ViewModel;
        }

        private void OnNewNoteClicked(object sender, EventArgs e)
        {
            ViewModel.AgregarNota();
        }

        private void OnTabChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Nota seleccionada)
            {
                ViewModel.SeleccionarNota(seleccionada);
            }
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        private const string WorkspaceFolder = "workspace";
        private string workspacePath;
        private ST.Timer saveTimer;
        private Nota _notaActual;

        public ObservableCollection<Nota> Notas { get; set; }

        public Nota NotaActual
        {
            get => _notaActual;
            set
            {
                _notaActual = value;
                OnPropertyChanged(nameof(NotaActual));
            }
        }

        public MainViewModel()
        {
            // Definir el workspace dentro de la carpeta bin
            workspacePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, WorkspaceFolder);

            if (!Directory.Exists(workspacePath))
                Directory.CreateDirectory(workspacePath);

            // Cargar notas existentes
            Notas = new ObservableCollection<Nota>(Directory.GetFiles(workspacePath, "*.txt")
                                .Select(file => new Nota(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file))));

            // Seleccionar la primera nota si existe
            NotaActual = Notas.FirstOrDefault();

            // Iniciar auto-guardado
            saveTimer = new ST.Timer(5000);
            saveTimer.Elapsed += GuardarNota;
            saveTimer.Start();
        }

        public void AgregarNota()
        {
            var nuevaNota = new Nota($"Nota {Notas.Count + 1}", "");
            Notas.Add(nuevaNota);
            SeleccionarNota(nuevaNota);
        }

        public void SeleccionarNota(Nota nota)
        {
            NotaActual = nota;
        }

        private void GuardarNota(object sender, ElapsedEventArgs e)
        {
            if (NotaActual != null)
            {
                string filePath = Path.Combine(workspacePath, NotaActual.Nombre + ".txt");
                File.WriteAllText(filePath, NotaActual.Texto);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Nota : INotifyPropertyChanged
    {
        private string _nombre;
        private string _texto;

        public string Nombre
        {
            get => _nombre;
            set
            {
                _nombre = value;
                OnPropertyChanged(nameof(Nombre));
            }
        }

        public string Texto
        {
            get => _texto;
            set
            {
                _texto = value;
                OnPropertyChanged(nameof(Texto));
            }
        }

        public Nota(string nombre, string texto)
        {
            _nombre = nombre;
            _texto = texto;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
