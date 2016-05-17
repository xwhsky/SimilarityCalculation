using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimilarityCalculation.Algorithms.DataParsers
{
    /// <summary>
    /// 空间关系配置表
    /// </summary>
    public class RelationDataParser
    {
        private DataSet _dataset;

        public RelationDataParser(string xlspath)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                using (var stream = System.IO.File.OpenRead(xlspath))
                {
                    package.Load(stream);
                }

                bool hasHeader = true;
                _dataset = new DataSet();

                var worksheetCount = package.Workbook.Worksheets.Count;
                foreach (var ws in package.Workbook.Worksheets)
                {
                    DataTable dt = new DataTable(ws.Name);
                    int totalCols = ws.Dimension.End.Column;
                    int totalRows = ws.Dimension.End.Row;
                    int startRow = hasHeader ? 2 : 1;
                    ExcelRange wsRow;
                    DataRow dr;
                    foreach (var firstRowCell in ws.Cells[1, 1, 1, totalCols])
                    {
                        dt.Columns.Add(hasHeader ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
                    }

                    for (int rowNum = startRow; rowNum <= totalRows; rowNum++)
                    {
                        wsRow = ws.Cells[rowNum, 1, rowNum, totalCols];
                        dr = dt.NewRow();
                        foreach (var cell in wsRow)
                        {
                            dr[cell.Start.Column - 1] = cell.Text;
                        }

                        dt.Rows.Add(dr);
                    }
                    
                    _dataset.Tables.Add(dt);
                }
            }
        }

        /// <summary>
        /// 获取某一空间关系阈值范围
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public double[] GetRelationEnumRange(Enum enumType)
        {
            Type type = enumType.GetType();
            var dt = _dataset.Tables.Cast<DataTable>().SingleOrDefault(_ => _.TableName.Equals("MatchingTable"));
            var row = dt.Rows.Cast<DataRow>().SingleOrDefault(_ => _[0].Equals(enumType.ToString()));
            return row[1].ToString().Split(new[] { '，', ',' }).Select(_ =>
            {
                double result;
                if (double.TryParse(_, out result))
                    return result;
                else
                    return double.PositiveInfinity;
            }).ToArray();
        }

        /// <summary>
        /// 获取该枚举的最大值
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public double GetMaxValueOfEnum(Enum enumType)
        {
            double maxValue = double.MinValue;
            var type = enumType.GetType();
            var dt = _dataset.Tables.Cast<DataTable>().SingleOrDefault(_ => _.TableName.Equals(type.Name));
            int columnCount = dt.Columns.Count;
            foreach (DataRow item in dt.Rows)
            {
                for (int i = 0; i < columnCount; i++)
                {
                    double value;
                    if(double.TryParse(item[i].ToString(),out value))
                    {
                        if (value > maxValue)
                            maxValue = value;
                    }
                }
            }
            return maxValue;
        }

        public DirRelation GetDirRelationByValue(double value)
        {
            foreach (DirRelation myCode in (DirRelation[])Enum.GetValues(typeof(DirRelation)))
            {
                double[] range = GetRelationEnumRange(myCode);
                //特殊情况337.5，22.5
                if (range[0]>range[1])
                {
                    if ((value > 0 && value < range[1]) || (value > 337.5))
                        return myCode;
                }
                else
                {
                    if (value >= range[0] && value < range[1])
                        return myCode;
                }
                
            }
            throw new Exception("未找到匹配关系");
        }

        public DisRelation GetDisRelationByValue(double value)
        {
            foreach (DisRelation myCode in (DisRelation[])Enum.GetValues(typeof(DisRelation)))
            {
                double[] range = GetRelationEnumRange(myCode);
                if (value >= range[0] && value < range[1])
                    return myCode;
            }
            throw new Exception("未找到匹配关系");
        }

        public LengthRelation GetLengthRelationByValue(double value)
        {
            foreach (LengthRelation myCode in (LengthRelation[])Enum.GetValues(typeof(LengthRelation)))
            {
                double[] range = GetRelationEnumRange(myCode);
                if (value >= range[0] && value < range[1])
                    return myCode;
            }
            throw new Exception("未找到匹配关系");
        }

        public AngleRelation GetAngleRelationByValue(double value)
        {
            foreach (AngleRelation myCode in (AngleRelation[])Enum.GetValues(typeof(AngleRelation)))
            {
                double[] range = GetRelationEnumRange(myCode);
                if (value >= range[0] && value < range[1])
                    return myCode;
            }
            throw new Exception("未找到匹配关系");
        }

        public ConcavityRelation GetConcavityRelationByValue(double value)
        {
            foreach (ConcavityRelation myCode in (ConcavityRelation[])Enum.GetValues(typeof(ConcavityRelation)))
            {
                double[] range = GetRelationEnumRange(myCode);
                if (value >= range[0] && value < range[1])
                    return myCode;
            }
            throw new Exception("未找到匹配关系");
        }

        /// <summary>
        /// 获取两类空间关系距离值
        /// </summary>
        /// <param name="enumType1"></param>
        /// <param name="enumType2"></param>
        /// <returns></returns>
        public double GetValueOfTwoRelations(Enum enumType1, Enum enumType2)
        {
            Type type = enumType1.GetType();
            var dt = _dataset.Tables.Cast<DataTable>().SingleOrDefault(_ => _.TableName.Equals(type.Name));
            var row = dt.Rows.Cast<DataRow>().SingleOrDefault(_ => _[0].Equals(enumType1.ToString()));
            return 1 -
                double.Parse(row[dt.Columns[enumType2.ToString()]].ToString()) / GetMaxValueOfEnum(enumType1);
        }
    }
}
