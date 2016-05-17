using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimilarityCalculation.Algorithms
{
    public class GeometrySimilarity
    {
        /// <summary>
        /// 拓扑关系计算
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns></returns>
        public static PolygonTopRelation CalTopRelationOfTwoPolygons(Geometry g1, Geometry g2)
        {
            Geometry intersectionG = g1.Intersection(g2);
            double area1 = g1.GetArea();
            double area2 = g2.GetArea();
            if (intersectionG == null || intersectionG.IsEmpty())
                return PolygonTopRelation.ss_disjoint;

            double area3 = intersectionG.GetArea();
            
            if (intersectionG.GetGeometryType() == wkbGeometryType.wkbLineString)
                return PolygonTopRelation.ss_touch;
            if ((area3 == area1) && (area3 == area2))
                return PolygonTopRelation.ss_equal;
            if ((area3 < area1) && (area3 < area2))
                return PolygonTopRelation.ss_overlap;
            if (area1 > area2)
            {
                if (area2 != area3)
                    throw new Exception("不应该出现的判断路径!");
                if (JudgePolygonTouch(intersectionG, g1))
                    return PolygonTopRelation.ss_covers;
                else
                    return PolygonTopRelation.ss_contains;
            }
            else
            {
                if (area1 != area3)
                    throw new Exception("不应该出现的判断路径!");
                if (JudgePolygonTouch(intersectionG, g2))
                    return PolygonTopRelation.ss_covered_by;
                else
                    return PolygonTopRelation.ss_inside;
            }
        }

        /// <summary>
        /// 方向关系计算
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns></returns>
        public static DirRelation CalDirRelationOfTwoGeometrys(Geometry g1, Geometry g2)
        {
            var c1 = g1.Centroid();
            var c2 = g2.Centroid();
            var vector = new[] { c2.GetX(0) - c1.GetX(0), c2.GetY(0) - c1.GetY(0) };

            //var c1 = GetCentroid(g1);
            //var c2 = GetCentroid(g2);
            //var vector = new[] { c2[1] - c1[1], c2[0] - c1[0] };
            var baseVector = new[] { 1, 0 };
            var theta = Math.Acos(vector[0] /
                                    Math.Sqrt(Math.Pow(vector[0], 2) + Math.Pow(vector[1], 2)))
                * 180 / Math.PI;
            if (vector[1] < 0)
                theta = 360 - theta;
            return TheUniversal.RelationDataParsers.GetDirRelationByValue(theta);
        }

        /// <summary>
        /// 距离关系计算
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public static DisRelation CalDisRelationOfTwoGeometrys(Geometry g1,Geometry g2,double maxDistance)
        {
            var distance = CalDistanceOfTwoGeometrys(g1, g2);
            return TheUniversal.RelationDataParsers.GetDisRelationByValue(distance / maxDistance);
        }

        #region 形状关系

        /// <summary>
        /// 凹凸性形状关系
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pointIndex"></param>
        /// <returns></returns>
        public static ConcavityRelation CalPointConvaityRelation(Geometry g, int pointIndex)
        {
            var theta = CalPointAngle(g, pointIndex);
            return TheUniversal.RelationDataParsers.GetConcavityRelationByValue(theta);
            //if (theta > 180)
            //    return ConcavityRelation.Concave;
            //else
            //    return ConcavityRelation.Convex;
        }

        /// <summary>
        /// 角度形状关系
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pointIndex"></param>
        /// <returns></returns>
        public static AngleRelation CalPointAngleRelation(Geometry g, int pointIndex)
        {
            var theta = CalPointAngle(g, pointIndex);
            if (theta > 180)
                theta = 360 - theta;
            return TheUniversal.RelationDataParsers.GetAngleRelationByValue(theta);
        }

        /// <summary>
        /// 长度形状关系
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pointIndex"></param>
        /// <returns></returns>
        public static LengthRelation CalLengthRelation(Geometry g,int pointIndex)
        {
            int formerPointIndex = pointIndex - 1;
            if (formerPointIndex < 0)
                formerPointIndex = g.GetPointCount() - 1;
            int latterPointIndex = pointIndex + 1;
            if (latterPointIndex >= g.GetPointCount() - 1)
                latterPointIndex = 0;
            double[] center = new double[3];
            double[] latter = new double[3];
            double[] former = new double[3];
            g.GetPoint(pointIndex, center);
            g.GetPoint(formerPointIndex, former);
            g.GetPoint(latterPointIndex, latter);
            var vector1 = new[] { latter[0] - center[0], latter[1] - center[1] };
            var vector2 = new[] { former[0] - center[0], former[1] - center[1] };
            var len1 = Math.Sqrt(Math.Pow(vector1[0], 2) + Math.Pow(vector1[1], 2));
            var len2 = Math.Sqrt(Math.Pow(vector2[0], 2) + Math.Pow(vector2[1], 2));
            return TheUniversal.RelationDataParsers.GetLengthRelationByValue(len2 / len1);
        }

        public static double CalShapeSimilarity(Geometry g1,int p1,Geometry g2,int p2)
        {
            var c1 = g1.GetPointCount() - 1;
            if (p1 > c1)
                p1 = p1 - c1;

            ConcavityRelation cr1 = CalPointConvaityRelation(g1, p1);
            AngleRelation ar1 = CalPointAngleRelation(g1, p1);
            LengthRelation lr1 = CalLengthRelation(g1, p1);

            ConcavityRelation cr2 = CalPointConvaityRelation(g2, p2);
            AngleRelation ar2 = CalPointAngleRelation(g2, p2);
            LengthRelation lr2 = CalLengthRelation(g2, p2);

            var cr3 = TheUniversal.RelationDataParsers.GetValueOfTwoRelations(cr1, cr2);
            var ar3 = TheUniversal.RelationDataParsers.GetValueOfTwoRelations(ar1, ar2);
            var lr3 = TheUniversal.RelationDataParsers.GetValueOfTwoRelations(lr1, lr2);
            return (cr3 + ar3 + lr3) / 3;
        }

        /// <summary>
        ///假设g1顶点个数大于g2
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns></returns>
        public static double CalShapeSimilarity(Geometry g1, Geometry g2)
        {
            var c1 = g1.GetPointCount() - 1;
            var c2 = g2.GetPointCount() - 1;
            int[] indexes = new int[c1];  //存储数组索引
            for (int i = 0; i < c1; i++)
                indexes[i] = i;
            List<List<int>> availableArrays = GetList(indexes, c2);

            double maxValue = double.MinValue;

            foreach (var availableArray in availableArrays)
            {
                double sumValue = 0;
                for (int i = 0; i < c2; i++)
                {
                    sumValue += CalShapeSimilarity(g1, availableArray[i], g2, i);
                    //for (int j = 0; j < c2; j++)
                    //{
                    //    sumValue += CalShapeSimilarity(g1, availableArray[i], g2, j);
                    //}
                }
                if (maxValue < sumValue)
                    maxValue = sumValue;
            }

            return maxValue / c1;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexes">剩余可使用的索引</param>
        /// <param name="reff">已经添加的列表</param>
        /// <param name="count">还需要加的个数</param>
        static List<List<int>> GetList(int[] indexes,int count)
        {
            List<List<int>> result = new List<List<int>>();
            if (count == 0)
                return result;
            else if(count ==1)
            {
                foreach (var index in indexes)
                    result.Add(new[] { index }.ToList<int>());
                return result;
            }

            var fullCount = indexes.Length;
            List<List<int>> formerReff = GetList(indexes, count - 1);

            foreach (var item in formerReff)
            {
                var minValue = item.First();
                var maxValue = item.Last();
                IEnumerable<int> availableIndexes;
                if (maxValue >= minValue)
                    availableIndexes = indexes.Where(_ => (!item.Contains(_)) &&
                    (_ > maxValue || _ < minValue));
                else
                    availableIndexes = indexes.Where(_ => (!item.Contains(_)) &&
                    (_ > maxValue && _ < minValue));

                foreach (var availableIndex in availableIndexes)
                {
                    List<int> subItem = new List<int>();
                    subItem.AddRange(item);
                    subItem.Add(availableIndex);
                    result.Add(subItem);
                }
            }

            //去重
            result = result.Distinct(new List_User_DistinctBy_userId()).ToList();
            return result;
        }


        /// <summary>
        /// 计算点的角度（0~360度）
        /// 仅适用多边形点顺时针排列的情况
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pointIndex"></param>
        /// <returns></returns>
        public static double CalPointAngle(Geometry g, int pointIndex)
        {
            int pointCount = g.GetPointCount() - 1;
            int formerPointIndex = pointIndex - 1;
            if (formerPointIndex < 0)
                formerPointIndex = pointCount - 1;
            int latterPointIndex = pointIndex + 1;
            if (latterPointIndex >= pointCount)
                latterPointIndex = 0;
            double[] center = new double[3];
            double[] latter = new double[3];
            double[] former = new double[3];
            g.GetPoint(pointIndex, center);
            g.GetPoint(formerPointIndex, former);
            g.GetPoint(latterPointIndex, latter);

            var vector1 = new[] { center[0] - former[0], center[1] - former[1] };
            var vector2 = new[] { latter[0] - center[0], latter[1] - center[1] };
           
            //var dot = vector1[0] * vector2[0] + vector2[1] * vector2[1];
            //var det = vector2[0] * vector1[1] - vector2[1] * vector1[0];
            //var angle = Math.Atan2(det, dot)*180/Math.PI;
            //angle = 180 - angle;

            var zcrossproduct = vector1[0] * vector2[1] - vector1[1] * vector2[0];

            var angle0 = (vector1[0] * vector2[0] + vector1[1] * vector2[1]) / Math.Sqrt((vector1[0] * vector1[0] + vector1[1] * vector1[1]) * (vector2[0] * vector2[0] + vector2[1] * vector2[1]));
            angle0 = 180 - Math.Acos(angle0) * 180 / Math.PI;
            if (zcrossproduct > 0)
                angle0 = 360 - angle0;
            return angle0;
        }


        #endregion

        /// <summary>
        /// 计算两个图形间的距离
        /// </summary>
        /// <param name="g1"></param>
        /// <param name="g2"></param>
        /// <returns></returns>
        public static double CalDistanceOfTwoGeometrys(Geometry g1, Geometry g2)
        {
            var c1 = g1.Centroid();
            var c2 = g2.Centroid();
            var vector = new[] { c2.GetX(0) - c1.GetX(0), c2.GetY(0) - c1.GetY(0) };

            //var c1 = GetCentroid(g1);
            //var c2 = GetCentroid(g2);
            //var vector = new[] { c2[1] - c1[1], c2[0] - c1[0] };

            var distance = Math.Sqrt(Math.Pow(vector[0], 2) + Math.Pow(vector[1], 2));
            return distance;
        }

        /// <summary>
        /// 计算场景内最大的距离
        /// </summary>
        /// <param name="geometrys">Geometry必须是Feature下的直接变量，不能再深一层</param>
        /// <returns></returns>
        public static double CalculateMaxDistanceOfScene(List<Geometry> geometrys)
        {
            int count = geometrys.Count;
            double maxValue = double.MinValue;
            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    //var c1 = geometrys[i].Centroid();
                    //var c2 = geometrys[j].Centroid();
                    //var c1 = GetCentroid(geometrys[i]);
                    //var c2 = GetCentroid(geometrys[j]);
                    ////var vector = new[] { c2.GetY(0) - c1.GetY(0), c2.GetX(0) - c1.GetX(0) };
                    //var vector = new[] { c2[1] - c1[1], c2[0] - c1[0] };
                    var distance = CalDistanceOfTwoGeometrys(geometrys[i], geometrys[j]);
                    if (distance > maxValue)
                        maxValue = distance;
                }
            }
            return maxValue;
        }

       /// <summary>
       /// 判断图形顶点是否在另一个图形边上
       /// </summary>
       /// <param name="g1"></param>
       /// <param name="g2"></param>
       /// <returns></returns>
        public static bool JudgePolygonTouch(Geometry g1,Geometry g2)
        {
            int pc1 = g1.GetPointCount() - 1;
            for (int i = 0; i < pc1; i++)
            {
                double[] args=new double[3];
                g1.GetPoint(i, args);
                Geometry point = new Geometry(wkbGeometryType.wkbPoint);
                point.SetPoint(0, args[0], args[1], args[2]);
                if (point.Intersect(g2))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 获取图形的中心点
        /// </summary>
        /// <param name="shape"></param>
        /// <returns></returns>
        public static double[] GetCentroid(Geometry shape)
        {
            if (shape.GetPointCount() == 0)
                return null;

            var pointCount = shape.GetPointCount() - 1;
            var points = new List<double[]>(pointCount);
            for (int i = 0; i < pointCount; i++)
                points.Add(new[] { shape.GetX(i), shape.GetY(i) });

            double accumulatedArea = 0.0f;
            double centerX = 0.0f;
            double centerY = 0.0f;

            for (int i = 0, j = pointCount - 1; i < pointCount; j = i++)
            {
                double temp = points[i][0] * points[j][1] - points[j][0] * points[i][1];
                accumulatedArea += temp;
                centerX += (points[i][0] + points[j][0]) * temp;
                centerY += (points[i][1] + points[j][1]) * temp;
            }

            if (accumulatedArea < 1E-7f)
                return null;  // Avoid division by zero

            accumulatedArea *= 3f;
            return new[] { centerX / accumulatedArea, centerY / accumulatedArea };
        }


    }


    public class List_User_DistinctBy_userId : IEqualityComparer<List<int>>
    {
        bool IEqualityComparer<List<int>>.Equals(List<int> x, List<int> y)
        {
            if (x.Count != y.Count)
                return false;
            for (int i = 0; i < x.Count; i++)
                if (x[i] != y[i])
                    return false;

            return true;
        }

        int IEqualityComparer<List<int>>.GetHashCode(List<int> obj)
        {
            return 0;
        }
    }


}
