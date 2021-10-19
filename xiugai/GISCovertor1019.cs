using System;
//测试修改
namespace GibbonGIS.Coords
{
    public class GISConvertor 
    {
        static private bool IsInChina(double[] lnglat)
        {
            return (72.004 < lnglat[0]) && (lnglat[0] < 137.8347) && (0.8293 < lnglat[1]) && (lnglat[1] < 55.8271);
        }

        static private double TransformLng(double lng, double lat)
        {
            double ret = 300.0 + lng + 2 * lat + 0.1 * lng * lng + 0.1 * lng * lat + 0.1 * Math.Sqrt(Math.Abs(lng));
            ret += (20.0 * Math.Sin(6.0 * lng * Constance.PI) + 20.0 * Math.Sin(2.0 * lng * Constance.PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(lng * Constance.PI) + 40.0 * Math.Sin(lng / 3.0 * Constance.PI)) * 2.0 / 3.0;
            ret += (150.0 * Math.Sin(lng / 12.0 * Constance.PI) + 300.0 * Math.Sin(lng / 30.0 * Constance.PI)) * 2.0 / 3.0;

            return ret;
        }

        static private double TransformLat(double lng, double lat)
        {
            double ret = -100.0 + 2.0 * lng + 3.0 * lat + 0.2 * lat * lat + 0.1 * lng * lat + 0.2 * Math.Sqrt(Math.Abs(lng));
            ret += (20.0 * Math.Sin(6.0 * lng * Constance.PI) + 20.0 * Math.Sin(2.0 * lng * Constance.PI)) * 2.0 / 3.0;
            ret += (20.0 * Math.Sin(lat * Constance.PI) + 40.0 * Math.Sin(lat / 3.0 * Constance.PI)) * 2.0 / 3.0;
            ret += (160.0 * Math.Sin(lat / 12.0 * Constance.PI) + 320.0 * Math.Sin(lat * Constance.PI / 30.0)) * 2.0 / 3.0;

            return ret;
        }

        static public double[] Wgs842Gcj02(double[] lnglat)
        {
            double lng = lnglat[0];
            double lat = lnglat[1];
            
            if (IsInChina(lnglat))
            {
                double dlng = TransformLng(lng - 105.0, lat - 35.0);
                double dlat = TransformLat(lng - 105.0, lat - 35.0);

                double radlat = lat / 180.0 * Constance.PI;
                double magic = Math.Sin(radlat);
                magic = 1.0 - Constance.EE * magic * magic;
                double sqrtMagic = Math.Sqrt(magic);

                dlng = dlng * 180.0 / (Constance.AA / sqrtMagic * Math.Cos(radlat) * Constance.PI);
                dlat = dlat * 180.0 / (Constance.AA * (1.0 - Constance.EE) / (magic * sqrtMagic) * Constance.PI);

                lng += dlng;
                lat += dlat;
            }

            return new double[] { lng, lat };
        }

        //static public double[] Wgs842Bd09(double[] lnglat)
        //{
        //    return Wgs842Gcj02(Gcj022Bd09(lnglat));
        //}

        static public double[] Gcj022Wgs84(double[] lnglat)
        {
            double lng = lnglat[0];
            double lat = lnglat[1];

            if (IsInChina(lnglat))
            {
                double dlng = TransformLng(lng - 105.0, lat - 35.0);
                double dlat = TransformLat(lng - 105.0, lat - 35.0);

                double radlat = lat / 180.0 * Constance.PI;
                double magic = Math.Sin(radlat);
                magic = 1.0 - Constance.EE * magic * magic;
                double sqrtMagic = Math.Sqrt(magic);

                dlng = dlng * 180.0 / (Constance.AA / sqrtMagic * Math.Cos(radlat) * Constance.PI);
                dlat = dlat * 180.0 / (Constance.AA * (1.0 - Constance.EE) / (magic * sqrtMagic) * Constance.PI);

                lng = lng * 2 - (lng + dlng);
                lat = lat * 2 - (lat + dlat);
            }

            return new double[] { lng, lat };
        }

        static public double[] Gcj022Bd09(double[] lnglat)
        {
            double lng = lnglat[0];
            double lat = lnglat[1];

            double z = Math.Sqrt(lng * lng + lat * lat) + 0.00002 * Math.Sin(lat * Constance.XPI);
            double theta = Math.Atan2(lat, lng) + 0.000003 * Math.Cos(lng * Constance.XPI);

            return new double[] { z * Math.Cos(theta) + 0.0065, z * Math.Sin(theta) + 0.006 };
        }

        static public double[] Bd092Wgs84(double[] lnglat)
        {
            return Gcj022Wgs84(Bd092Gcj02(lnglat));
        }

        static public double[] Bd092Gcj02(double[] lnglat)
        {
            double lng = lnglat[0] - 0.0065;
            double lat = lnglat[1] - 0.006;
            double z = Math.Sqrt(lng * lng + lat * lat) - 0.00002 * Math.Sin(lat * Constance.XPI);
            double theta = Math.Atan2(lat, lng) - 0.000003 * Math.Cos(lng * Constance.XPI);

            return new double[] { z * Math.Cos(theta), z * Math.Sin(theta) };
        }

        static public double[] LngLat2Mercator(double[] lnglat, double[] reference, bool isMilimeter)
        {
            double mercatorX = lnglat[0] * Constance.MERCATOR;
            double mercatorY = Math.Log(Math.Tan((90.0 + lnglat[1]) * Constance.PI / 360.0)) / (Constance.PI / 180.0);
            mercatorY *= Constance.MERCATOR;

            double mercatorRefX = reference[0] * Constance.MERCATOR;
            double mercatorRefY = Math.Log(Math.Tan((90.0 + reference[1]) * Constance.PI / 360.0)) / (Constance.PI / 180.0);
            mercatorRefY *= Constance.MERCATOR;

            double x = mercatorX - mercatorRefX;
            double y = mercatorY - mercatorRefY;

            return isMilimeter ? new double[] { x * 1000.0, y * 1000.0 } : new double[] { x, y };
        }

        static public double[] Mercator2LngLat(double[] mercator, double[] reference, bool isMilimeter)
        {
            double x = mercator[0] / Constance.MERCATOR;
            double y = mercator[1] / Constance.MERCATOR;

            if (isMilimeter)
            {
                x /= 1000;
                y /= 1000;
            }
            
            y = 180.0 / Constance.PI * (2 * Math.Atan(Math.Exp(y * Constance.PI / 180.0)) - Constance.PI / 2);
            return new double[] { x + reference[0], y + reference[1] };
        }
    }
}
