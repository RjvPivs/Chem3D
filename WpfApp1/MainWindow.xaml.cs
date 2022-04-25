using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChemSharp;
using ChemSharp.Molecules;
using ChemSharp.Spectroscopy;
using Microsoft.Win32;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {

            InitializeComponent();
            ModelVisual3D visual3d = new ModelVisual3D();
            group = new Model3DGroup();

            visual3d.Content = group;
            viewport.Children.Add(visual3d);

            DefineCamera(viewport);

            //DefineModel(group);

        }
        // The camera.
        string path;
        Model3DGroup group;
        private PerspectiveCamera perspCamera = null;

        // The camera controller.
        private SphericalCameraController controller = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void DefineCamera(Viewport3D vp)
        {
            perspCamera = new PerspectiveCamera();
            perspCamera.FieldOfView = 60;
            controller = new SphericalCameraController(perspCamera, vp, this, grid, grid);
        }
        private void DefineLights(Model3DGroup gp)
        {
            Color col = Color.FromArgb(255, 96, 96, 96);
            gp.Children.Add(new AmbientLight(col));
            gp.Children.Add(new DirectionalLight(col, new Vector3D(0, -1, 0)));
            gp.Children.Add(new DirectionalLight(col, new Vector3D(1, -3, -2)));
            gp.Children.Add(new DirectionalLight(col, new Vector3D(-1, 3, 2)));
        }
        private void DefineModel(Model3DGroup group)
        {
            group.Children.Clear();
            DefineLights(group);
            // Make a non-smooth sphere.
            // MeshGeometry3D mesh1 = new MeshGeometry3D();
            //mesh1.AddSphere(new Point3D(-1.75, 0, 1.75), 1.5, 20, 10);
            // group.Children.Add(mesh1.MakeModel(Brushes.Pink));

            // Make a smooth sphere.
            // MeshGeometry3D mesh2 = new MeshGeometry3D();
            // mesh2.AddSphere(new Point3D(1.75, 0, 1.75), 1.5, 20, 10, true);
            // group.Children.Add(mesh2.MakeModel(Brushes.Pink));
            //string path = @"C:\Users\Pivs\Downloads\Telegram Desktop\640536-MOF(100).cif";
            var provider = new ChemSharp.Molecules.DataProviders.CIFDataProvider(path);
            var mol = new Molecule(provider.Atoms, provider.Bonds);
            var a = mol.Atoms;
            int count = 0;
            foreach (var b in a)
            {
                //double t = b.Location.X;
                //MessageBox.Show(t.ToString());
                MeshGeometry3D meh = new MeshGeometry3D();
                //var tt = b.Color;
                count++;
                meh.AddSphere(new Point3D(b.Location.X, b.Location.Y, b.Location.Z), 0.2, 20, 10, true);
                group.Children.Add(meh.MakeModel(Brushes.Pink));
            }
            MessageBox.Show(count.ToString());
            count = 0;
            var c = mol.Bonds;
            foreach (var b in c)
            {
                count++;
                MeshGeometry3D meh = new MeshGeometry3D();
                meh.AddLine(new Point3D(b.Atom2.Location.X, b.Atom2.Location.Y, b.Atom2.Location.Z), new Point3D(b.Atom1.Location.X, b.Atom1.Location.Y, b.Atom1.Location.Z));
                group.Children.Add(meh.MakeModel(Brushes.AliceBlue));
            }
            MessageBox.Show(count.ToString());
            //group.Children.Add(meh.MakeModel(Brushes.Blue));
            //MeshExtensions.AddAxes(group);
        }

        private void openCIF_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CIF files (*.CIF)|*.CIF";
            openFileDialog.ShowDialog();
            if (!String.IsNullOrEmpty(openFileDialog.FileName))
            {
                path = openFileDialog.FileName;
                DefineModel(group);
            }
        }
    }
}
