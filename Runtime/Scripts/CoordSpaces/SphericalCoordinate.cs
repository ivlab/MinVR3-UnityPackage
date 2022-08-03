
namespace IVLab.MinVR3
{
    /// <summary>
    /// A tiny class to hold the spherical coordinates for a point, used primarily for working
    /// with planetarium domes -- see SphericalDome.cs.
    /// </summary>
    public class SphericalCoordinate
    {
        public SphericalCoordinate()
        {
            radialDist = 0;
            polarAngleInDeg = 0;
            azimuthalAngleInDeg = 0;
        }

        public SphericalCoordinate(float radialDist, float polarAngleInDeg, float azimuthalAngleInDeg)
        {
            this.radialDist = radialDist;
            this.polarAngleInDeg = polarAngleInDeg;
            this.azimuthalAngleInDeg = azimuthalAngleInDeg;
        }

        public float radialDist;
        public float polarAngleInDeg;
        public float azimuthalAngleInDeg;
    }

}
