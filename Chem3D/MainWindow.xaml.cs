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

namespace Chem3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            mshLine = new List<MeshGeometry3D>();
            mshSphere = new List<MeshGeometry3D>();
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
        List<Atom> a;
        List<Bond> c;
        List<MeshGeometry3D> mshLine;
        List<MeshGeometry3D> mshSphere;
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
            ChemSharp.Molecules.DataProviders.CIFDataProviders provider;
            try
            {
                provider = new ChemSharp.Molecules.DataProviders.CIFDataProviders(path);
            }
            catch
            {
                MessageBox.Show("Невозможно считать CIF.");

                volume.Visibility = Visibility.Hidden;
                volume.Items.Clear();
                lBox.Visibility = Visibility.Hidden;
                lBox.Items.Clear();
                return;
            }
            var mol = new Molecule(provider.Atoms, provider.Bonds);
            a = mol.Atoms;
            if (a.Count == 0)
            {
                MessageBox.Show("В структуре отсутствуют атомы. Отображение невозможно.");
                volume.Visibility = Visibility.Hidden;
                volume.Items.Clear();
                lBox.Visibility = Visibility.Hidden;
                lBox.Items.Clear();
                return;
            }
            foreach (var b in a)
            {
                MeshGeometry3D meh = new MeshGeometry3D();
                Brush brush;
                try
                {
                    brush = new BrushConverter().ConvertFromString(b.Color) as Brush;
                }
                catch
                {
                    brush = Brushes.Pink;
                }
                double radius;
                try
                {
                    radius = (double)b.AtomicRadius;
                    if (radius < 0.1)
                    {

                        radius = 0.1;
                    }
                    else if (radius > 0.35)
                    {
                        radius = 0.35;
                    }
                }
                catch
                {
                    radius = 0.2;
                }
                meh.AddSphere(new Point3D(b.Location.X * 15, b.Location.Y * 15, b.Location.Z * 15), radius, 20, 10, true);
                mshSphere.Add(meh);
                group.Children.Add(meh.MakeModel(brush));
            }
            c = mol.Bonds;
            foreach (var b in c)
            {
                MeshGeometry3D meh = new MeshGeometry3D();
                meh.AddLine(new Point3D(b.Atom2.Location.X * 15, b.Atom2.Location.Y * 15, b.Atom2.Location.Z * 15), new Point3D(b.Atom1.Location.X * 15, b.Atom1.Location.Y * 15, b.Atom1.Location.Z * 15));
                mshLine.Add(meh);
                group.Children.Add(meh.MakeModel(Brushes.AliceBlue));
            }
            //group.Children.Add(meh.MakeModel(Brushes.Blue));
            //MeshExtensions.AddAxes(group);
            var txt = new TextBlock
            {
                Background = Brushes.LightGray,
                Text = "Подсчитанный объём:"
            };
            volume.Items.Add(txt);
            volume.Items.Add(Convert.ToString(CountVolume()));
            volume.Visibility = Visibility.Visible;
        }

        private void openCIF_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CIF files (*.CIF)|*.CIF";
            openFileDialog.ShowDialog();
            if (!String.IsNullOrEmpty(openFileDialog.FileName))
            {
                mshLine.Clear();
                mshSphere.Clear();
                volume.Items.Clear();
                lBox.Visibility = Visibility.Hidden;
                lBox.Items.Clear();
                path = openFileDialog.FileName;
                DefineModel(group);
            }
        }
        private void viewport_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //   if (SelectedModel != null)
            //  {
            //      SelectedModel.Material = NormalMaterial;
            //     SelectedModel = null;
            //  }
            //SelectedModel.Bo
            // Get the mouse's position relative to the viewport.
            Point mouse_pos = e.GetPosition(viewport);

            // Perform the hit test.
            HitTestResult result =
                VisualTreeHelper.HitTest(viewport, mouse_pos);

            // See if we hit a model.
            RayMeshGeometry3DHitTestResult mesh_result =
                result as RayMeshGeometry3DHitTestResult;
            if (mesh_result == null)
            {

            }
            if (mesh_result != null)
            {
                GeometryModel3D model = (GeometryModel3D)mesh_result.ModelHit;
                if (mshLine.Exists(el => el.Bounds == model.Bounds))
                {
                    //MessageBox.Show("Yes");
                    int index = mshLine.IndexOf(mshLine.Find(el => el.Bounds == model.Bounds));
                    //MessageBox.Show(Convert.ToString(c[index].Length));
                    lBox.Items.Clear();
                    lBox.Visibility = Visibility.Visible;
                    var txt = new TextBlock();
                    txt.Background = Brushes.LightGray;
                    txt.Text = "Выбранная связь обладает следующими характеристиками:";
                    lBox.Items.Add(txt);
                    //lBox.Items[0]
                    lBox.Items.Add("Расстояние: " + Convert.ToString(c[index].Length));

                }
                else if (mshSphere.Exists(el => el.Bounds == model.Bounds))
                {
                    //MessageBox.Show("Yes");
                    int index = mshSphere.IndexOf(mshSphere.Find(el => el.Bounds == model.Bounds));
                    lBox.Items.Clear();
                    lBox.Visibility = Visibility.Visible;
                    var txt = new TextBlock();
                    txt.Background = Brushes.LightGray;
                    txt.Text = "Выбранный атом обладает \nследующими \nхарактеристиками:";
                    lBox.Items.Add(txt);
                    try
                    {
                        lBox.Items.Add("Координаты: " + a[index].Location);
                    }
                    catch
                    {

                    }
                    try
                    {
                        string tmp = Convert.ToString(a[index].AtomicRadius);
                        if (!String.IsNullOrEmpty(tmp))
                        {
                            lBox.Items.Add("Радиус: " + tmp);
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        string tmp = Convert.ToString(a[index].AtomicWeight);
                        if (!String.IsNullOrEmpty(tmp))
                        {
                            lBox.Items.Add("Вес: " + tmp);
                        }
                    }
                    catch
                    {

                    }
                    try
                    {
                        string tmp = Convert.ToString(a[index].Group);
                        if (!String.IsNullOrEmpty(tmp))
                        {
                            lBox.Items.Add("Группа: " + tmp);
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
        private double CountVolume()
        {
            List<double[]> lst = new List<double[]>();
            foreach (var b in a)
            {
                double[] db = new double[3];
                db[0] = Convert.ToDouble(b.Location.X);
                db[1] = Convert.ToDouble(b.Location.Y);
                db[2] = Convert.ToDouble(b.Location.Z);
                lst.Add(db);
            }
            var res = MIConvexHull.ConvexHull.Create(lst);
            var vert = res.Result.Points.ToList();

            var faces = res.Result.Faces.ToList();

            var facesRes = new List<List<Vector3D>>();
            foreach (var face in faces)
            {
                var tempor = new List<Vector3D>();
                tempor.Add(new Vector3D(vert[vert.IndexOf(face.Vertices[0])].Position[0], vert[vert.IndexOf(face.Vertices[0])].Position[1], vert[vert.IndexOf(face.Vertices[0])].Position[2]));
                tempor.Add(new Vector3D(vert[vert.IndexOf(face.Vertices[1])].Position[0], vert[vert.IndexOf(face.Vertices[1])].Position[1], vert[vert.IndexOf(face.Vertices[1])].Position[2]));
                tempor.Add(new Vector3D(vert[vert.IndexOf(face.Vertices[2])].Position[0], vert[vert.IndexOf(face.Vertices[2])].Position[1], vert[vert.IndexOf(face.Vertices[2])].Position[2]));
                facesRes.Add(tempor);
            }
            double resulting = 0;
            var nullCoord = new Vector3D(vert[0].Position[0], vert[0].Position[1], vert[0].Position[2]);

            foreach (var face in facesRes)
            {
                if (!face.Contains(nullCoord))
                {
                    resulting += StillCounting(nullCoord, face[0], face[1], face[2]);
                }
            }
            return resulting;
        }
        private double StillCounting(Vector3D nullCoord, Vector3D first, Vector3D second, Vector3D third)
        {
            double fst = Distance(nullCoord, first), scnd = Distance(nullCoord, second), thrd = Distance(nullCoord, third);
            double frth = Distance(second, first), fifth = Distance(third, second), sxh = Distance(first, third);
            double coef = 1.0 / 144.0;
            return Math.Sqrt(coef * (fst * fst * fifth * fifth * (scnd * scnd + thrd * thrd + frth * frth + sxh * sxh - fst * fst - fifth * fifth) +
                scnd * scnd * sxh * sxh * (fst * fst + thrd * thrd + frth * frth + fifth * fifth - scnd * scnd - sxh * sxh)
                + thrd * thrd * frth * frth * (fst * fst + scnd * scnd + fifth * fifth + sxh * sxh - thrd * thrd - frth * frth)
                - fst * fst * scnd * scnd * frth * frth - scnd * scnd * thrd * thrd * fifth * fifth - fst * fst * thrd * thrd * sxh * sxh - frth * frth * fifth * fifth * sxh * sxh));
            // fst*fst*fifth*fifth*(scnd*scnd + thrd*thrd+frth*frth+sxh*sxh-fst*fst-fifth*fifth)
            //scnd*scnd * sxh*sxh * (fst*fst + thrd*thrd+frth*frth + fifth*fifth - scnd*scnd - sxh*sxh)
            //thrd*thrd * frth*frth * (fst*fst + scnd*scnd+ fifth*fifth + sxh*sxh - thrd*thrd - frth*frth)
            //- fst*fst*scnd*scnd *frth*frth - scnd*scnd *thrd*thrd * fifth*fifth - fst*fst* thrd*thrd * sxh*sxh - frth*frth *fifth*fifth*sxh*sxh
            //
        }
        private double Distance(Vector3D fst, Vector3D scnd)
        {
            return Math.Sqrt(Math.Pow(fst.X - scnd.X, 2) + Math.Pow(fst.Y - scnd.Y, 2) + Math.Pow(fst.Z - scnd.Z, 2));
        }
    }
}
