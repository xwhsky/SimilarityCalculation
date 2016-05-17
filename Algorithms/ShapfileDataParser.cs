using OSGeo.GDAL;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimilarityCalculation.Algorithms.DataParsers
{
    public class ShapfileDataParser
    {
        private DataSource _dataset;

        public string Name { get { return _dataset.GetName(); } }

        public ShapfileDataParser(string shpPath)
        {
            _dataset = Ogr.OpenShared(shpPath, 0);
        } 

        public Layer GetLayerByIndex(int index)
        {
            return _dataset.GetLayerByIndex(index);
        }

        public IList<Layer> GetLayers()
        {
            IList<Layer> result = new List<Layer>();
            var count = _dataset.GetLayerCount();
            for (int i = 0; i < count; i++)
                result.Add(_dataset.GetLayerByIndex(i));
            return result;
        }

        public IList<Feature> GetFeaturesByLayerIndex(int index)
        {
            Layer layer = GetLayerByIndex(index);
            IList<Feature> features = new List<Feature>();
            for (int i = 0; i < layer.GetFeatureCount(0); i++)
            {
                features.Add(layer.GetFeature(i));
            }
            return features;
        }

        public IList<Geometry> GetGeometrysByLayerIndex(int index)
        {
            Layer layer = GetLayerByIndex(index);
            IList<Geometry> geometrys = new List<Geometry>();
            for (int i = 0; i < layer.GetFeatureCount(0); i++)
            {
                geometrys.Add(layer.GetFeature(i).GetGeometryRef());
            }
            return geometrys;
        }

        public int GetFeatureCount(int layerIndex)
        {
            return GetLayerByIndex(layerIndex).GetFeatureCount(0);
        }



    }
}
