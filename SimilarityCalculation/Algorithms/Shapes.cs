using OSGeo.OGR;
using SimilarityCalculation.Algorithms.DataParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimilarityCalculation.Algorithms
{
    /// <summary>
    /// 所有要素的集合
    /// </summary>
    public class Shapes
    {
        public IList<Feature> Features {  get; private set; }
        public string Name {  get; private set; }

        public Shapes(ShapfileDataParser dataParser)
        {
            Name = dataParser.Name;
            Features = dataParser.GetFeaturesByLayerIndex(0);
        }
    }
}
