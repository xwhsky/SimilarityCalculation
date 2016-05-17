using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimilarityCalculation.Algorithms
{
    /// <summary>
    /// 空间相似性
    /// </summary>
    public interface ISpatialSimilarity
    {
        /// <summary>
        /// 形状相似性
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        double ShpOpt( Geometry geometry);

        /// <summary>
        /// 拓扑相似性
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        double TopOpt( Geometry geometry);

        /// <summary>
        /// 方向相似性
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        double DirOpt( Geometry geometry);

        /// <summary>
        /// 距离相似性
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        double DisOpt(Geometry geometry);


    }
}
