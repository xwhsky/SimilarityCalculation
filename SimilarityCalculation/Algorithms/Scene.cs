using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimilarityCalculation.Algorithms
{
    public class Scene
    {
        List<Feature> _features;

        public List<Feature> Features { get { return _features; } }
        public string Name { get; private set;}

        public Scene(IList<Feature> features,string name)
        {
            _features = features.ToList();
            _features.Sort((a, b) => a.GetFieldAsString("sx").CompareTo(b.GetFieldAsString("sx")));
            Name = name;
        }


        

        /// <summary>
        /// 查找对应要素
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="featureIndex"></param>
        /// <returns></returns>
        public int GetReferencedFeatureIndex(Scene scene,int featureIndex)
        {
            string fieldName = "sx";
            int trgfieldIndex = scene.Features[featureIndex].GetFieldIndex(fieldName);
            int srcfieldIndex = scene.Features.First().GetFieldIndex(fieldName);
            var str = scene.Features[featureIndex].GetFieldAsString(trgfieldIndex);
            return this.Features.FindIndex(_ => _.GetFieldAsString(srcfieldIndex).Equals(str));
        }

        public DataTable CallShapeSimilarityInDetail(Scene scene)
        {
            DataTable shapeTable = new DataTable();
            shapeTable.TableName = "Shape";

            for (int i = 0; i < scene.Features.Count; i++)
            {
                var trgFeat = scene.Features[i];
                var srcFeat = this.Features[GetReferencedFeatureIndex(scene, i)];
                shapeTable.Columns.Add(string.Format("{0}-{1}", srcFeat.GetFieldAsString("sx"), trgFeat.GetFieldAsString("sx")));
            }

            var row = shapeTable.NewRow();
            for (int i = 0; i < scene.Features.Count; i++)
            {
                var trgFeat = scene.Features[i];
                var srcFeat = this.Features[GetReferencedFeatureIndex(scene, i)];

                var trgGeometry = trgFeat.GetGeometryRef().GetGeometryRef(0);

                var srcGeometry = srcFeat.GetGeometryRef().GetGeometryRef(0);

                double shapeValue;
                if ((trgGeometry.GetPointCount() - 1) > (srcGeometry.GetPointCount() - 1))
                    shapeValue = GeometrySimilarity.CalShapeSimilarity(trgGeometry, srcGeometry);
                else
                    shapeValue = GeometrySimilarity.CalShapeSimilarity(srcGeometry, trgGeometry);

                row[i] = shapeValue;
            }
            shapeTable.Rows.Add(row);

            return shapeTable;
        }

        public DataSet CallSpatialSimilarityInDetail(Scene scene)
        {
            DataSet result = new DataSet();

            DataTable topologyTable = new DataTable() { TableName = "Topology"};
            DataTable directionTable = new DataTable() { TableName="Direction"};
            DataTable distanceTable = new DataTable() { TableName = "Distance"};

            //空间相似性
            double spatialSimilarity = 0;
            var indexes = GetReferencedFeatureIndexes(scene);

            var maxDis1 = GeometrySimilarity.CalculateMaxDistanceOfScene(scene.Features.Select(_ => _.GetGeometryRef()).ToList());
            var maxDis2 = GeometrySimilarity.CalculateMaxDistanceOfScene(this.Features.Select(_ => _.GetGeometryRef()).ToList());

            double topSimilarity = 0;
            double dirSimilarity = 0;
            double disSimilarity = 0;

            foreach (var item in indexes)
            {
                var g1 = scene.Features[item[0]].GetGeometryRef();
                var g2 = scene.Features[item[1]].GetGeometryRef();
                string columnName = string.Format("{0}&{1}-{2}&{3}",
                    this.Features[item[2]].GetFieldAsString("sx"), this.Features[item[3]].GetFieldAsString("sx"),
                   scene.Features[item[0]].GetFieldAsString("sx"), scene.Features[item[1]].GetFieldAsString("sx"));
                topologyTable.Columns.Add(columnName);
                directionTable.Columns.Add(columnName);
                distanceTable.Columns.Add(columnName);
            }

            var topologyRow = topologyTable.NewRow();
            var directionRow = directionTable.NewRow();
            var distanceRow = distanceTable.NewRow();

            for(int k=0;k<indexes.Count;k++)
            {
                var item = indexes[k];
                var g1 = scene.Features[item[0]].GetGeometryRef();
                var g2 = scene.Features[item[1]].GetGeometryRef();
                PolygonTopRelation top1 = GeometrySimilarity.CalTopRelationOfTwoPolygons(
                    scene.GetParentGeometry(item[0]), scene.GetParentGeometry(item[1]));
                PolygonTopRelation top2 = GeometrySimilarity.CalTopRelationOfTwoPolygons(
                    this.GetParentGeometry(item[2]), this.GetParentGeometry(item[3]));
                topologyRow[k] = TheUniversal.RelationDataParsers.GetValueOfTwoRelations(top1, top2);

                DirRelation dir1 = GeometrySimilarity.CalDirRelationOfTwoGeometrys(
                    scene.GetParentGeometry(item[0]), scene.GetParentGeometry(item[1]));
                DirRelation dir2 = GeometrySimilarity.CalDirRelationOfTwoGeometrys(
                     this.GetParentGeometry(item[2]), this.GetParentGeometry(item[3]));
                directionRow[k] = TheUniversal.RelationDataParsers.GetValueOfTwoRelations(dir1, dir2);

                DisRelation dis1 = GeometrySimilarity.CalDisRelationOfTwoGeometrys(
                    scene.GetParentGeometry(item[0]), scene.GetParentGeometry(item[1]), maxDis1);
                DisRelation dis2 = GeometrySimilarity.CalDisRelationOfTwoGeometrys(
                     this.GetParentGeometry(item[2]), this.GetParentGeometry(item[3]), maxDis2);
                distanceRow[k] = TheUniversal.RelationDataParsers.GetValueOfTwoRelations(dis1, dis2);
            }

            topologyTable.Rows.Add(topologyRow);
            directionTable.Rows.Add(directionRow);
            distanceTable.Rows.Add(distanceRow);

            result.Tables.Add(topologyTable);
            result.Tables.Add(directionTable);
            result.Tables.Add(distanceTable);
            return result;
        }


        public double CalFullSimilarity(Scene scene)
        {
            //形状相似性
            double shapeSimilarity = 0;
            for (int i = 0; i < scene.Features.Count; i++)
            {
                var trgFeat = scene.Features[i];
                var srcFeat = this.Features[GetReferencedFeatureIndex(scene, i)];

                var trgGeometry = trgFeat.GetGeometryRef().GetGeometryRef(0);
               
                var srcGeometry = srcFeat.GetGeometryRef().GetGeometryRef(0);

                double shapeValue;
                if ((trgGeometry.GetPointCount() - 1) > (srcGeometry.GetPointCount() - 1))
                    shapeValue = GeometrySimilarity.CalShapeSimilarity(trgGeometry, srcGeometry);
                else
                    shapeValue = GeometrySimilarity.CalShapeSimilarity(srcGeometry, trgGeometry);

                shapeSimilarity += shapeValue;
            }

            //空间相似性
            double spatialSimilarity = 0;
            var indexes = GetReferencedFeatureIndexes(scene);
            var maxDis1 = GeometrySimilarity.CalculateMaxDistanceOfScene(scene.Features.Select(_ => _.GetGeometryRef()).ToList());
            var maxDis2 = GeometrySimilarity.CalculateMaxDistanceOfScene(this.Features.Select(_ => _.GetGeometryRef()).ToList());

            double topSimilarity = 0;
            double dirSimilarity = 0;
            double disSimilarity = 0;
            foreach (var item in indexes)
            {
                var g1 = scene.Features[item[0]].GetGeometryRef();
                var g2 = scene.Features[item[1]].GetGeometryRef();
                PolygonTopRelation top1 = GeometrySimilarity.CalTopRelationOfTwoPolygons(
                    scene.GetParentGeometry(item[0]), scene.GetParentGeometry(item[1]));
                PolygonTopRelation top2 = GeometrySimilarity.CalTopRelationOfTwoPolygons(
                    this.GetParentGeometry(item[2]), this.GetParentGeometry(item[3]));
                topSimilarity+= TheUniversal.RelationDataParsers.GetValueOfTwoRelations(top1, top2); ;
                spatialSimilarity += TheUniversal.RelationDataParsers.GetValueOfTwoRelations(top1, top2);

                DirRelation dir1 = GeometrySimilarity.CalDirRelationOfTwoGeometrys(
                    scene.GetParentGeometry(item[0]), scene.GetParentGeometry(item[1]));
                DirRelation dir2 = GeometrySimilarity.CalDirRelationOfTwoGeometrys(
                     this.GetParentGeometry(item[2]), this.GetParentGeometry(item[3]));
                dirSimilarity += TheUniversal.RelationDataParsers.GetValueOfTwoRelations(dir1, dir2);
                spatialSimilarity += TheUniversal.RelationDataParsers.GetValueOfTwoRelations(dir1, dir2);

                DisRelation dis1 = GeometrySimilarity.CalDisRelationOfTwoGeometrys(
                    scene.GetParentGeometry(item[0]), scene.GetParentGeometry(item[1]), maxDis1);
                DisRelation dis2 = GeometrySimilarity.CalDisRelationOfTwoGeometrys(
                     this.GetParentGeometry(item[2]), this.GetParentGeometry(item[3]), maxDis2);
                disSimilarity += TheUniversal.RelationDataParsers.GetValueOfTwoRelations(dis1, dis2);
                spatialSimilarity += TheUniversal.RelationDataParsers.GetValueOfTwoRelations(dis1, dis2);
            }

            topSimilarity = topSimilarity / 6 / indexes.Count;
            dirSimilarity = dirSimilarity / 6 / indexes.Count;
            disSimilarity = disSimilarity / 6 / indexes.Count;


            return shapeSimilarity / 2 / scene.Features.Count + spatialSimilarity / 6 / indexes.Count;
        }

        public List<int[]> GetReferencedFeatureIndexes(Scene scene)
        {
            List<int[]> result = new List<int[]>();
            var count = scene.Features.Count;
            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    int[] item = new int[4];
                    item[0] = i;
                    item[1] = j;
                    item[2] = GetReferencedFeatureIndex(scene, i);
                    item[3] = GetReferencedFeatureIndex(scene, j);
                    result.Add(item);
                }
            }
            return result;
        }

        public Geometry GetGeometry(int index)
        {
            return this.Features[index].GetGeometryRef().GetGeometryRef(0);
        }

        public Geometry GetParentGeometry(int index)
        {
            return this.Features[index].GetGeometryRef();
        }
    }
}
