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
            // Frage siehe https://gis.stackexchange.com/questions/435057/c-gdal-spatial-transformation-reprojection-always-fails

            MaxRev.Gdal.Core.GdalBase.ConfigureAll();
            OSGeo.OGR.Ogr.RegisterAll();

            OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "NO");
            OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "");
            OSGeo.GDAL.Gdal.SetConfigOption("PROJ_DEBUG", "5");

            OSGeo.OGR.Driver drv = OSGeo.OGR.Ogr.GetDriverByName("ESRI Shapefile");
            OSGeo.OGR.DataSource ds = drv.Open(@"C:\Users\croonenbroeck\source\repos\Prototyp\Testdata\Gemeinden.shp", 0);
            OSGeo.OGR.Layer MyLayer = ds.GetLayerByIndex(0);

            OSGeo.OSR.SpatialReference FromSRS = MyLayer.GetSpatialRef();
            FromSRS.SetAxisMappingStrategy(OSGeo.OSR.AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
            string CheckSRS;
            FromSRS.ExportToWkt(out CheckSRS, null);

            OSGeo.OSR.SpatialReference ToSRS = new OSGeo.OSR.SpatialReference(null);
            ToSRS.ImportFromEPSG(4326);
            ToSRS.SetAxisMappingStrategy(OSGeo.OSR.AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
            ToSRS.ExportToWkt(out CheckSRS, null);

            //OSGeo.OGR.DataSource NewDS = drv.CreateDataSource("/vsimem/Temporary", new string[] { });
            OSGeo.OGR.DataSource NewDS = drv.CreateDataSource(@"C:\Users\croonenbroeck\source\repos\Prototyp\Testdata\Test.shp", new string[] { });
            OSGeo.OGR.Layer NewLayer = NewDS.CreateLayer(MyLayer.GetName() + "_WGS84", ToSRS, MyLayer.GetGeomType(), new string[] { });

            OSGeo.OSR.CoordinateTransformation CT = new OSGeo.OSR.CoordinateTransformation(FromSRS, ToSRS);

            OSGeo.OGR.FeatureDefn MyFeatureDefn = MyLayer.GetLayerDefn();
            OSGeo.OGR.FeatureDefn outLayerDefn = MyLayer.GetLayerDefn();

            OSGeo.OGR.Geometry OGRGeom;
            for (long i = 0; i < MyLayer.GetFeatureCount(0); i++)
            {
                OGRGeom = MyLayer.GetFeature(i).GetGeometryRef();

                if (OGRGeom.Transform(CT) != 0) System.Diagnostics.Debug.WriteLine("Error during projection.");

                OSGeo.OGR.Feature OutFeature = new OSGeo.OGR.Feature(MyFeatureDefn);
                OutFeature.SetGeometry(OGRGeom);
                for (int j = 0; j < outLayerDefn.GetFieldCount(); j++)
                {
                    // Liste unvollständig...

                    if (MyLayer.GetFeature(i).GetFieldType(j) == OSGeo.OGR.FieldType.OFTString)
                        OutFeature.SetField(outLayerDefn.GetFieldDefn(j).GetNameRef(), MyLayer.GetFeature(i).GetFieldAsString(j));

                    if (MyLayer.GetFeature(i).GetFieldType(j) == OSGeo.OGR.FieldType.OFTInteger)
                        OutFeature.SetField(outLayerDefn.GetFieldDefn(j).GetNameRef(), MyLayer.GetFeature(i).GetFieldAsInteger(j));

                    if (MyLayer.GetFeature(i).GetFieldType(j) == OSGeo.OGR.FieldType.OFTInteger64)
                        OutFeature.SetField(outLayerDefn.GetFieldDefn(j).GetNameRef(), MyLayer.GetFeature(i).GetFieldAsInteger64(j));
                }

                NewLayer.CreateFeature(OutFeature);
                OutFeature.Dispose();
            }

            MyFeatureDefn.Dispose();
            NewDS.SyncToDisk();
            drv.Dispose();
        }
    }
}
