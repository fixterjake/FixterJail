using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DOJ.Core.Models
{
    internal static class Vector3Extensions {

        internal static float DistanceTo(this Vector3 v1, Vector3 v2) {
            float distance = DistanceTo(v1, v2, true);
            return distance;
        }

        internal static float DistanceTo2D(this Vector3 v1, Vector3 v2) {
            float distance = DistanceTo(v1, v2, false);
            return distance;
        }

        private static float DistanceTo(Vector3 v1, Vector3 v2, bool useZ) {
            try {
                float distance = Function.Call<float>(Hash.GET_DISTANCE_BETWEEN_COORDS, v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, true);
                return distance;
            } catch {
                return -1.0f;
            }
        }



    }
}
