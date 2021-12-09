using Caliburn.Micro;
using GMap.NET;

namespace Iit.Fibertest.Client
{
    public class MapLimits : PropertyChangedBase
    {
        private double _left, _right;
        private double _top, _bottom;

        public bool IsEmpty => _left.Equals(0);

        public void Set(PointLatLng p1, PointLatLng p2)
        {
            if (p1.Lat > p2.Lat)
            {
                _top = p1.Lat;
                _bottom = p2.Lat;
            }
            else
            {
                _top = p2.Lat;
                _bottom = p1.Lat;
            }

            if (p1.Lng > p2.Lng)
            {
                _left = p2.Lng;
                _right = p1.Lng;
            }
            else
            {
                _left = p1.Lng;
                _right = p2.Lng;
            }

            NotifyOfPropertyChange(nameof(IsEmpty));
        }

        public int NodeCount;

        public bool IsIn(PointLatLng point)
        {
            return !(point.Lat > _top) && !(point.Lat < _bottom) && !(point.Lng < _left) && !(point.Lng > _right);
        }


        public override string ToString()
        {
            // return $@"lat [{_bottom:F6} : {_top:F6}] - lng [{_left:F6} : {_right:F6}]";
            return $@"node count {NodeCount}";
        }

    }
}
