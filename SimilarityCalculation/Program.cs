using OfficeOpenXml;
using OSGeo.GDAL;
using OSGeo.OGR;
using SimilarityCalculation.Algorithms;
using SimilarityCalculation.Algorithms.DataParsers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimilarityCalculation
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime time1 = DateTime.Now;
           var flag = TheUniversal.StartRuntime();

            string[] shpPaths = Directory.GetFiles(@"D:\Study\Projects\ga\DATA20160325", "*.shp");
            var shps = shpPaths.Select(x => new ShapfileDataParser(x));
            var shpScenes = shps.Select(x => new Scene(x.GetFeaturesByLayerIndex(0), x.Name)).ToList();

            string destFilename = @"D:\Study\Projects\ga\AnalysisResults";
            var srcScene = shpScenes[0];
            for (int i = 0; i < shpScenes.Count; i++)
            {
                var shapeTable = srcScene.CallShapeSimilarityInDetail(shpScenes[i]);
                var spatialDataset = srcScene.CallSpatialSimilarityInDetail(shpScenes[i]);

                spatialDataset.Tables.Add(shapeTable);
                var fi = new FileInfo(string.Format(@"{0}\{1}.xls", destFilename, i));
                SaveDataTableToExcel(fi, spatialDataset);
            }

            DateTime time2 = DateTime.Now;
            Console.WriteLine((time2 - time1).TotalSeconds);

            return;

            DataTable table = new DataTable();
            for (int i = 0; i < shpScenes.Count; i++)
                table.Columns.Add(i.ToString());
            for (int i = 0; i < shpScenes.Count; i++)
                table.Rows.Add();

            //var scene = shpScenes[0].CalFullSimilarity(shpScenes[2]);
            //var scenXian = shpScenes[1].CalFullSimilarity(shpScenes[3]);
            // var ss= shpScenes[0].CalFullSimilarity(shpScenes[3]);
            //return;
            
            for (int i = 0; i < shpScenes.Count; i++)
            {
                for (int j = i + 1; j < shpScenes.Count; j++)
                {
                    var result = shpScenes[i].CalFullSimilarity(shpScenes[j]);
                    table.Rows[i][j] = result;
                }
            }


            //  string path = @"D:\Study\Projects\ga\code\SimilarityCalculation\SimilarityCalculation\bin\Debug\Test\buildings.shp";
            //  ShapfileDataParser shapefile = new ShapfileDataParser(path);
            //  var geometrys = shapefile.GetGeometrysByLayerIndex(0);
            //var type=  geometrys.First().GetGeometryType();

            //  Polygon polygon1 = new Polygon(geometrys[0]);
            //  Polygon polygon2 = new Polygon(geometrys[1]);

            //  TheUniversal.RelationDataParsers.GetRelationEnumRange(DisRelation.abc);
            //// double aa = TheUniversal.RelationDataParsers.GetValueOfTwoRelations(PolygonTopRelation.ss_contains, PolygonTopRelation.ss_covers);
            //  aa = TheUniversal.RelationDataParsers.GetValueOfTwoRelations(DisRelation.abc, DisRelation.cl);
            //var dirValue =  (polygon1 as ISpatialSimilarity).DirOpt(polygon2.Shape);
        }

        static void SaveDataTableToExcel(FileInfo newFile,DataSet dataset)
        {
            using (ExcelPackage pck = new ExcelPackage(newFile))
            {
                foreach (DataTable item in dataset.Tables)
                {
                    ExcelWorksheet ws = pck.Workbook.Worksheets.Add(item.TableName);
                    ws.Cells["A1"].LoadFromDataTable(item, true);
                }
                pck.Save();
            }
        }

        List<Scene> GetScenesFromShp(string shpFile,int featureCount)
        {
            ShapfileDataParser parser = new ShapfileDataParser(shpFile);
            var features = parser.GetFeaturesByLayerIndex(0);
            int index = 0;
            List<Feature> subFeatures = new List<Feature>();
            while (index<featureCount)
            {
                  
                
            }
        }
        
    }
}
