using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAsyncUdp
{
    [Serializable]
    class _vector
    {
        public float x;
        public float y;
        public float z;

        public _vector(float _x = 0, float _y = 0, float _z = 0)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public static _vector operator +(_vector v1, _vector v2)
        {
            _vector _v3 = new _vector();
            _v3.x = v1.x + v2.x;
            _v3.y = v1.y + v2.y;
            _v3.z = v1.z + v2.z;
            return _v3;
        }
        public static bool operator !=(_vector v1, _vector v2)
        {
            if (v1.x != v2.x) return true;
            if (v1.y != v2.y) return true;
            if (v1.z != v2.z) return true;
            return false;
        }
        public static bool operator ==(_vector v1, _vector v2)
        {
            if (v1.x != v2.x) return false;
            if (v1.y != v2.y) return false;
            if (v1.z != v2.z) return false;
            return true;
        }
        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;
            _vector _v2 = obj as _vector;
            if ((System.Object)_v2 == null)
                return false;
            return (this == _v2);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
