using OSGeo.OGR;
using SimilarityCalculation.Algorithms.DataParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TINVoronoi;

namespace SimilarityCalculation.Algorithms
{
    /// <summary>
    /// 所有要素的集合
    /// </summary>
    public class Shapes
    {
        public IList<Feature> Features {  get; private set; }
        public string Name {  get; private set; }

        public IDictionary<int,Geometry> CenterGeometrys { get; private set; }

        public Delaynay D_TIN { get; private set; }

        public Shapes(ShapfileDataParser dataParser)
        {
            Name = dataParser.Name;
            Features = dataParser.GetFeaturesByLayerIndex(0);

            CenterGeometrys = new Dictionary<int, Geometry>();
            Features.ToList().ForEach(x => CenterGeometrys.Add(x.GetFID(), x.GetGeometryRef().Centroid()));

            D_TIN = new Delaynay(); //核心功能类
            D_TIN.DS.VerticesNum = CenterGeometrys.Count;
            var values = CenterGeometrys.Values.ToList();
            for (int i = 0; i < D_TIN.DS.VerticesNum; i++)
                D_TIN.DS.Vertex[i] = new Vertex { x = values[i].GetX(0), y = values[i].GetY(0), ID = i };


        }

        public IList<Scene> ConstructScenes(Scene srcScene)
        {
            IList<Scene> scenes = new List<Scene>();

            return scenes;
        }



        
    }
}
