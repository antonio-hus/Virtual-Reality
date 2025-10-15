using System;
using System.IO;
using System.Text.RegularExpressions;

namespace rt;

/// <summary>
/// Represents a volumetric CT scan dataset for medical visualization in ray tracing.
/// Uses 3D voxel data loaded from raw files with metadata describing resolution and spacing.
/// Renders volume using ray marching through the voxel grid with color mapping for different densities.
/// </summary>
public class CtScan: Geometry
{
    /// <summary>
    /// Position of the CT scan volume in world coordinates (minimum corner).
    /// </summary>
    private readonly Vector _position;

    /// <summary>
    /// Scale factor applied to voxel dimensions for world-space sizing.
    /// </summary>
    private readonly double _scale;

    /// <summary>
    /// Maps voxel density values to colors for visualization.
    /// Different density ranges represent different tissue types.
    /// </summary>
    private readonly ColorMap _colorMap;

    /// <summary>
    /// Raw voxel data stored as bytes - each value represents tissue density.
    /// Stored in Z-Y-X order (Z changes slowest, X changes fastest).
    /// </summary>
    private readonly byte[] _data;

    /// <summary>
    /// Resolution of the CT scan in voxels [X, Y, Z].
    /// Number of samples along each axis.
    /// </summary>
    private readonly int[] _resolution = new int[3];

    /// <summary>
    /// Physical spacing between voxels in each dimension [X, Y, Z].
    /// Represents real-world distance between consecutive samples.
    /// </summary>
    private readonly double[] _thickness = new double[3];

    /// <summary>
    /// Minimum corner of the bounding box in world coordinates.
    /// </summary>
    private readonly Vector _v0;

    /// <summary>
    /// Maximum corner of the bounding box in world coordinates.
    /// </summary>
    private readonly Vector _v1;

    /// <summary>
    /// Loads a CT scan from .dat metadata file and .raw voxel data file.
    /// </summary>
    /// <param name="datFile">Path to metadata file containing resolution and slice thickness.</param>
    /// <param name="rawFile">Path to raw binary voxel data file.</param>
    /// <param name="position">Position of volume minimum corner in world space.</param>
    /// <param name="scale">Scale multiplier for voxel dimensions.</param>
    /// <param name="colorMap">Color mapping for different density values.</param>
    public CtScan(string datFile, string rawFile, Vector position, double scale, ColorMap colorMap) : base(Color.NONE)
    {
        _position = position;
        _scale = scale;
        _colorMap = colorMap;

        // Parse metadata file for resolution and voxel spacing
        var lines = File.ReadLines(datFile);
        foreach (var line in lines)
        {
            var kv = Regex.Replace(line, "[:\\t ]+", ":").Split(":");
            if (kv[0] == "Resolution")
            {
                _resolution[0] = Convert.ToInt32(kv[1]);
                _resolution[1] = Convert.ToInt32(kv[2]);
                _resolution[2] = Convert.ToInt32(kv[3]);
            } else if (kv[0] == "SliceThickness")
            {
                _thickness[0] = Convert.ToDouble(kv[1]);
                _thickness[1] = Convert.ToDouble(kv[2]);
                _thickness[2] = Convert.ToDouble(kv[3]);
            }
        }

        // Calculate bounding box corners
        _v0 = position;
        _v1 = position + new Vector(_resolution[0]*_thickness[0]*scale, _resolution[1]*_thickness[1]*scale, _resolution[2]*_thickness[2]*scale);

        // Load raw voxel data
        var len = _resolution[0] * _resolution[1] * _resolution[2];
        _data = new byte[len];
        using FileStream f = new FileStream(rawFile, FileMode.Open, FileAccess.Read);
        if (f.Read(_data, 0, len) != len)
        {
            throw new InvalidDataException($"Failed to read the {len}-byte raw data");
        }
    }
    
    /// <summary>
    /// Retrieves the density value at a specific voxel coordinate.
    /// Returns 0 for out-of-bounds coordinates (empty space).
    /// </summary>
    /// <param name="x">X voxel index.</param>
    /// <param name="y">Y voxel index.</param>
    /// <param name="z">Z voxel index.</param>
    /// <returns>Density value at the voxel, or 0 if outside bounds.</returns>
    private ushort Value(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0 || x >= _resolution[0] || y >= _resolution[1] || z >= _resolution[2])
        {
            return 0;
        }

        return _data[z * _resolution[1] * _resolution[0] + y * _resolution[0] + x];
    }

    /// <summary>
    /// Ray-volume intersection using volumetric ray marching with alpha compositing.
    /// Steps through voxels along the ray path, accumulating colors from semi-transparent layers.
    /// This allows seeing through the semi-transparent shell into the interior structure.
    /// Uses axis-aligned bounding box test first to determine entry/exit points.
    /// </summary>
    /// <param name="line">Ray to intersect with the volume.</param>
    /// <param name="minDist">Minimum valid intersection distance.</param>
    /// <param name="maxDist">Maximum valid intersection distance.</param>
    /// <returns>Composited color from all semi-transparent voxels along the ray, or Intersection.NONE if ray misses.</returns>
    public override Intersection GetIntersection(Line line, double minDist, double maxDist)
    {
        // First, perform ray-AABB (axis-aligned bounding box) intersection
        // to find where ray enters and exits the volume
        double tMin = minDist;
        double tMax = maxDist;

        // Check intersection with each pair of parallel planes (X, Y, Z)
        for (int i = 0; i < 3; i++)
        {
            double origin = i == 0 ? line.X0.X : (i == 1 ? line.X0.Y : line.X0.Z);
            double dir = i == 0 ? line.Dx.X : (i == 1 ? line.Dx.Y : line.Dx.Z);
            double boxMin = i == 0 ? _v0.X : (i == 1 ? _v0.Y : _v0.Z);
            double boxMax = i == 0 ? _v1.X : (i == 1 ? _v1.Y : _v1.Z);

            if (Math.Abs(dir) < 1e-8)
            {
                // Ray is parallel to slab - check if origin is within bounds
                if (origin < boxMin || origin > boxMax)
                {
                    return Intersection.NONE;
                }
            }
            else
            {
                // Calculate intersection distances with both planes
                double t1 = (boxMin - origin) / dir;
                double t2 = (boxMax - origin) / dir;

                // Ensure t1 is the near intersection, t2 is far
                if (t1 > t2)
                {
                    (t1, t2) = (t2, t1);
                }

                // Update tMin and tMax
                tMin = Math.Max(tMin, t1);
                tMax = Math.Min(tMax, t2);

                // Check if ray misses the box
                if (tMin > tMax)
                {
                    return Intersection.NONE;
                }
            }
        }

        // Ray marching through the volume with alpha compositing
        double stepSize = Math.Min(_thickness[0], Math.Min(_thickness[1], _thickness[2])) * _scale * 0.5;
        double t = Math.Max(tMin, minDist) + stepSize * 0.05;
        
        // Accumulated color and opacity using front-to-back compositing
        Color accumulatedColor = new Color(0, 0, 0, 0);
        double accumulatedAlpha = 0.0;
        
        // Position and time of first visible hit (for normal calculation)
        double firstHitT = -1;
        Vector firstHitPosition = null;
        
        // March through the volume accumulating semi-transparent layers
        while (t <= tMax && accumulatedAlpha < 1)
        {
            Vector position = line.CoordinateToPosition(t);
            Color color = GetColor(position);
            
            // If this voxel has any opacity, accumulate it using front-to-back compositing
            if (color.Alpha > 0.0)
            {
                // Remember first hit for intersection point and normal
                if (firstHitT < 0)
                {
                    firstHitT = t;
                    firstHitPosition = position;
                }
                
                // Front-to-back alpha compositing formula:
                // C_out = C_src * alpha_src * (1 - alpha_dst) + C_dst
                // alpha_out = alpha_src * (1 - alpha_dst) + alpha_dst
                double weight = color.Alpha * (1.0 - accumulatedAlpha);
                accumulatedColor += color * weight;
                accumulatedAlpha += weight;
            }
            
            t += stepSize;
        }
        
        // If we accumulated any visible material, return the composited result
        if (accumulatedAlpha > 0.0 && firstHitPosition != null)
        {
            // Calculate normal at first hit position for lighting
            Vector normal = GetNormal(firstHitPosition);
            
            // Create material from accumulated color
            Material material = Material.FromColor(accumulatedColor);
            
            // Return intersection with composited color
            return new Intersection(true, true, this, line, firstHitT, normal, material, accumulatedColor);
        }
        
        // No visible voxels found along the ray
        return Intersection.NONE;
    }
    
    /// <summary>
    /// Converts world position to voxel indices.
    /// </summary>
    /// <param name="v">World position vector.</param>
    /// <returns>Array of voxel indices [x, y, z].</returns>
    private int[] GetIndexes(Vector v)
    {
        return new []{
            (int)Math.Floor((v.X - _position.X) / _thickness[0] / _scale), 
            (int)Math.Floor((v.Y - _position.Y) / _thickness[1] / _scale),
            (int)Math.Floor((v.Z - _position.Z) / _thickness[2] / _scale)};
    }

    /// <summary>
    /// Gets the color at a world position by sampling the voxel grid and color map.
    /// </summary>
    /// <param name="v">World position to sample.</param>
    /// <returns>Color from the color map based on voxel density.</returns>
    private Color GetColor(Vector v)
    {
        int[] idx = GetIndexes(v);

        ushort value = Value(idx[0], idx[1], idx[2]);
        return _colorMap.GetColor(value);
    }

    /// <summary>
    /// Calculates surface normal at a world position using central differences.
    /// Approximates the gradient of the density field for lighting calculations.
    /// </summary>
    /// <param name="v">World position to calculate normal at.</param>
    /// <returns>Normalized gradient vector (surface normal).</returns>
    private Vector GetNormal(Vector v)
    {
        int[] idx = GetIndexes(v);
        double x0 = Value(idx[0] - 1, idx[1], idx[2]);
        double x1 = Value(idx[0] + 1, idx[1], idx[2]);
        double y0 = Value(idx[0], idx[1] - 1, idx[2]);
        double y1 = Value(idx[0], idx[1] + 1, idx[2]);
        double z0 = Value(idx[0], idx[1], idx[2] - 1);
        double z1 = Value(idx[0], idx[1], idx[2] + 1);

        return new Vector(x1 - x0, y1 - y0, z1 - z0).Normalize();
    }
}
