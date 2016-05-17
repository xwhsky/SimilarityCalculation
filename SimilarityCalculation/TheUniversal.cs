using OSGeo.GDAL;
using OSGeo.OGR;
using OSGeo.OSR;
using SimilarityCalculation.Algorithms.DataParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimilarityCalculation
{
    public class TheUniversal
    {
        /// <summary>
        /// 开启环境
        /// </summary>
        /// <returns></returns>
        public static bool StartRuntime()
        {
            try
            {
                Gdal.AllRegister();
                Ogr.RegisterAll();
                OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        private static RelationDataParser _relationDataParser;
        public static RelationDataParser RelationDataParsers
        {
            get
            {
                if (_relationDataParser == null)
                    _relationDataParser = new RelationDataParser(string.Format("{0}config.xlsx", AppDomain.CurrentDomain.BaseDirectory));
                return _relationDataParser;
            }
        }

    }
}
