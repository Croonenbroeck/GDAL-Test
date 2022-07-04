using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GDAL_Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MaxRev.Gdal.Core.GdalBase.ConfigureAll();
            OSGeo.OGR.Ogr.RegisterAll();

            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");
            OSGeo.GDAL.Gdal.SetConfigOption("PROJ_DEBUG", "5");

            OSGeo.OGR.Driver drv = OSGeo.OGR.Ogr.GetDriverByName("ESRI Shapefile");
            OSGeo.OGR.DataSource ds = drv.Open(@"C:\Users\[EnterNameHere]\source\repos\Prototyp\Testdata\Gemeinde.shp", 0);
            OSGeo.OGR.Layer MyLayer = ds.GetLayerByIndex(0);

            OSGeo.OSR.SpatialReference FromSRS = MyLayer.GetSpatialRef();
            string CheckSRS;
            FromSRS.ExportToWkt(out CheckSRS, null);

            OSGeo.OSR.SpatialReference ToSRS = new OSGeo.OSR.SpatialReference(null);
            ToSRS.ImportFromEPSG(4326);
            ToSRS.SetAxisMappingStrategy(OSGeo.OSR.AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);

            OSGeo.OSR.CoordinateTransformation CT = new OSGeo.OSR.CoordinateTransformation(FromSRS, ToSRS);

            OSGeo.OGR.Geometry OGRGeom;
            for (long i = 0; i < MyLayer.GetFeatureCount(0); i++)
            {
                OGRGeom = MyLayer.GetFeature(i).GetGeometryRef();

                if (OGRGeom.Transform(CT) == 0) System.Diagnostics.Debug.WriteLine("Error during projection.");

                OGRGeom.AssignSpatialReference(ToSRS); //Even necessary after transformation?
            }
        }
    }
}
