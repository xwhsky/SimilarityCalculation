using System;
using System.Collections.Generic;
using System.Drawing;

namespace TINVoronoi
{
    public partial class Delaynay
    {
       
        public List<long> HullPoint;  //͹�Ƕ�������
        private struct PntV_ID
        {
            public double Value;
            public long ID;
        }
        
        //����͹��
        public void CreateConvex()
        {
            //��ʼ��͹�Ƕ�������
            if (HullPoint == null)
                HullPoint = new List<long>();
            else
            {
                for (int i = 0; i < HullPoint.Count; i++)
                    DS.Vertex[HullPoint[i]].isHullEdge = 0;  //ȥ��͹�Ǳ��
                HullPoint.Clear();
            }

            #region ����x-y��x+y�������С��
            PntV_ID MaxMinus, MinMinus, MaxAdd, MinAdd;
            MaxMinus.ID = MinMinus.ID = MaxAdd.ID = MinAdd.ID = DS.Vertex[0].ID;//�õ�һ���ʼ��
            MaxMinus.Value = MinMinus.Value = DS.Vertex[0].x - DS.Vertex[0].y;
            MaxAdd.Value = MinAdd.Value = DS.Vertex[0].x + DS.Vertex[0].y;

            double temp;
            for (int i = 1; i < DS.VerticesNum; i++)
            {
                temp = DS.Vertex[i].x - DS.Vertex[i].y;
                if (temp > MaxMinus.Value)
                {
                    MaxMinus.Value = temp;
                    MaxMinus.ID = DS.Vertex[i].ID;
                }
                if (temp < MinMinus.Value)
                {
                    MinMinus.Value = temp;
                    MinMinus.ID = DS.Vertex[i].ID;
                }

                temp = DS.Vertex[i].x + DS.Vertex[i].y;
                if (temp > MaxAdd.Value)
                {
                    MaxAdd.Value = temp;
                    MaxAdd.ID = DS.Vertex[i].ID;
                }
                if (temp < MinAdd.Value)
                {
                    MinAdd.Value = temp;
                    MinAdd.ID = DS.Vertex[i].ID;
                }
            }
            #endregion

            //��������
            HullPoint.Add(MinMinus.ID);
            HullPoint.Add(MaxAdd.ID);
            HullPoint.Add(MaxMinus.ID); 
            HullPoint.Add(MinAdd.ID);
            //��Ҫȥ���ظ��㡭��
            for (int i = 0; i < HullPoint.Count; i++)
            {
                if (HullPoint[i] == HullPoint[(i + 1) % HullPoint.Count])
                    HullPoint.RemoveAt(i);
            }

            //�����ǣ���һ�����ٴ���
            for (int i = 0; i < HullPoint.Count; i++)
                DS.Vertex[HullPoint[i]].isHullEdge = -1;  

            #region ����������
            for (int i = 0; i < DS.VerticesNum; i++)
            {
                if (DS.Vertex[i].isHullEdge == -1)
                    continue;

                //�жϵ�i��ÿ���ߵĹ�ϵ
                double isOnRight;
                for (int j = 0; j < HullPoint.Count; j++)
                {
                    PointF pnt1 = new PointF(Convert.ToSingle(DS.Vertex[HullPoint[j]].x),
                            Convert.ToSingle(DS.Vertex[HullPoint[j]].y));
                    PointF pnt2 = new PointF(Convert.ToSingle(DS.Vertex[HullPoint[(j + 1) % HullPoint.Count]].x),
                            Convert.ToSingle(DS.Vertex[HullPoint[(j + 1) % HullPoint.Count]].y));
                    PointF pnt3 = new PointF(Convert.ToSingle(DS.Vertex[i].x), Convert.ToSingle(DS.Vertex[i].y));
                    isOnRight = VectorXMultiply(pnt1, pnt2, pnt3);// >0��λ�����(�豸����ϵ����ʱ��)

                    //������ڱ�������޸�͹��
                    if (isOnRight >= 0)
                    {
                        //�����͹������
                        HullPoint.Insert((j + 1) % (HullPoint.Count + 1), DS.Vertex[i].ID);

                        //�ж���ӵ���Ƿ�����ڰ�
                        pnt1 = new PointF(Convert.ToSingle(DS.Vertex[HullPoint[(j + 3) % HullPoint.Count]].x),
                            Convert.ToSingle(DS.Vertex[HullPoint[(j + 3) % HullPoint.Count]].y));
                        isOnRight = VectorXMultiply(pnt3, pnt2, pnt1);
                        if (isOnRight > 0)    //ɾ���ڰ���p2(��v[j+2])
                        {
                            int index = (j + 2) % HullPoint.Count;    //����͹�������е�����
                            DS.Vertex[HullPoint[index]].isHullEdge = 0;
                            HullPoint.RemoveAt(index);
                        }

                        break;
                    }
                }//for
            }//for
            #endregion
           
            //�������͹�Ǳ����Ϊ1
            for (int i = 0; i < HullPoint.Count; i++)
                DS.Vertex[HullPoint[i]].isHullEdge = 1;
        }

        //͹�������ʷ�
        public void HullTriangulation()
        {
            DS.TriangleNum = 0;

            //͹��Ϊ����ߵ����
            //��㹲�ߵ����

            //����͹�Ƕ���
            List<long> points = new List<long>();
            for (int i = 0; i < HullPoint.Count; i++)
                points.Add(HullPoint[i]);

            //����
            long id1, id2, id3;
            while (points.Count >= 3)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    //��Ϊ�ɾ������������
                    id1 = points[i];
                    id2 = points[(i + 1) % points.Count];
                    id3 = points[(i + 2) % points.Count];
                    if (IsClean(id1, id2, id3))
                    {                        
                        DS.Triangle[DS.TriangleNum].V1Index = id1;
                        DS.Triangle[DS.TriangleNum].V2Index = id2;
                        DS.Triangle[DS.TriangleNum].V3Index = id3;
                        DS.TriangleNum++;
                        DS.Vertex[id2].isHullEdge = 2;  //����ѹ�����
                        points.Remove(id2);

                        break;
                    }
                }//for
            }//while
        }

        //���������Բ�в�������͹�Ƕ���
        private bool IsClean(long p1ID, long p2ID, long p3ID)
        {
            for (int i = 0; i < HullPoint.Count; i++)
            {
                //�����ѹ����ĵ�͡�����
                if (DS.Vertex[HullPoint[i]].isHullEdge == 2 || HullPoint[i] == p1ID || HullPoint[i] == p2ID || HullPoint[i] == p3ID)
                    continue;
                
                //�����iλ�ڡ����Բ��
                if (InTriangleExtCircle(DS.Vertex[HullPoint[i]].x, DS.Vertex[HullPoint[i]].y, DS.Vertex[p1ID].x,
                    DS.Vertex[p1ID].y, DS.Vertex[p2ID].x, DS.Vertex[p2ID].y, DS.Vertex[p3ID].x, DS.Vertex[p3ID].y))
                    return false;
            }

            return true;
        }

    }
}