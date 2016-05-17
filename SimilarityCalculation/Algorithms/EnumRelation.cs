using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimilarityCalculation.Algorithms
{
    /// <summary>
    /// 方向关系
    /// </summary>
    public enum DirRelation
    {
        N = 0,
        NE = 1,
        E = 2,
        SE = 3,
        S = 4,
        SW = 5,
        W = 6,
        NW = 7
    }

    /// <summary>
    /// 距离关系
    /// </summary>
    public enum DisRelation
    {
        vc=0,
        cl=1,
        abc=2,
        co=3,
        abf=4,
        f=5,
        vf=6
    }

    /// <summary>
    /// 多边形间拓扑关系
    /// </summary>
    public enum PolygonTopRelation
    {
        ss_disjoint=0,
        ss_touch=1,
        ss_overlap=2,
        ss_covers=3,
        ss_covered_by=4,
        ss_contains=5,
        ss_inside=6,
        ss_equal=7
    }

    /// <summary>
    /// 凹凸性
    /// </summary>
    public enum ConcavityRelation
    {
        Concave=0,
        Convex = 1
    }

    /// <summary>
    /// 角度
    /// </summary>
    public enum AngleRelation
    {
        Veryacute=0,
        Acute=1,
        Right=2,
        Obtuse=3,
        Veryobtuse=4
    }

    /// <summary>
    /// 长度
    /// </summary>
    public enum LengthRelation
    {
        msh=0,
        hl=1,
        absh=2,
        sl=3,
        abl=4,
        dl=5,
        ml=6
    }



}
