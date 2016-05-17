using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSGeo.OGR;

namespace SimilarityCalculation.Algorithms
{
    public class Polygon : ISpatialSimilarity
    {
        public Geometry Shape { get; private set; }

        public Polygon(Geometry shape)
        {
            Shape = shape;
        }

        public double ShpOpt(Geometry geometry)
        {
            throw new NotImplementedException();
        }

        public double TopOpt(Geometry geometry)
        {
            throw new NotImplementedException();
        }

        public double DirOpt(Geometry geometry)
        {
            if (geometry.GetGeometryType() == wkbGeometryType.wkbPolygon)
            {
                var shp1 = Shape.Centroid();
                double[] args1 = new double[2];
                shp1.GetPoint_2D(0, args1);
                var shp2 = geometry.Centroid();
                double[] args2 = new double[2];
                shp2.GetPoint_2D(0, args2);

                
            }

            throw new NotImplementedException();
        }

        public double DisOpt(Geometry geometry)
        {
            if (geometry.GetGeometryType() == wkbGeometryType.wkbPolygon)
            {
                var shp1 = Shape.Centroid();
                var shp2 = geometry.Centroid();

            }

            throw new NotImplementedException();
        }
    }
}
