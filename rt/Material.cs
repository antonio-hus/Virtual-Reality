namespace rt
{
    /// <summary>
    /// Defines the surface material properties of an object.
    /// Controls how light interacts with a surface through ambient, diffuse, and specular components.
    /// </summary>
    public class Material
    {
        /// <summary>
        /// Default blank material with no lighting contribution.
        /// </summary>
        public static readonly Material BLANK = new(); 

        /// <summary>
        /// Ambient color - the base color visible even without direct lighting.
        /// Simulates indirect environmental lighting and prevents completely black shadows.
        /// </summary>
        public Color Ambient { get; }

        /// <summary>
        /// Diffuse color - determines the color of matte, non-directional surface reflection.
        /// Main component for object color appearance.
        /// Intensity depends on the angle between surface normal and light direction.
        /// </summary>
        public Color Diffuse { get; }

        /// <summary>
        /// Specular color - defines the color and intensity of shiny highlights.
        /// Creates bright spots on glossy surfaces where viewer, surface, and light align.
        /// Often white or light-colored for realistic materials.
        /// </summary>
        public Color Specular { get; }

        /// <summary>
        /// Shininess exponent - controls the size and sharpness of specular highlights.
        /// Higher values (100-1000) create small, sharp highlights for very glossy surfaces.
        /// Lower values (1-50) create large, soft highlights for rough or matte surfaces.
        /// </summary>
        public int Shininess { get; }

        /// <summary>
        /// Creates a blank material with no lighting contribution (all black, zero shininess).
        /// </summary>
        public Material()
        {
            Ambient = new Color();
            Diffuse = new Color();
            Specular = new Color();
            Shininess = 0;
        }

        /// <summary>
        /// Creates a material with custom ambient, diffuse, and specular properties.
        /// Provides full control over all Phong shading components.
        /// </summary>
        /// <param name="ambient">Ambient color component - base illumination without direct light.</param>
        /// <param name="diffuse">Diffuse color component - matte surface reflection.</param>
        /// <param name="specular">Specular color component - glossy highlight color.</param>
        /// <param name="shininess">Shininess exponent - controls highlight sharpness (typically 1-1000).</param>
        public Material(Color ambient, Color diffuse, Color specular, int shininess)
        {
            Ambient = new Color(ambient);
            Diffuse = new Color(diffuse);
            Specular = new Color(specular);
            Shininess = shininess;
        }
        
        /// <summary>
        /// Copy constructor - creates a new material from an existing one.
        /// </summary>
        /// <param name="m">The material to copy.</param>
        public Material(Material m) : this(m.Ambient, m.Diffuse, m.Specular, m.Shininess)
        {
        }

        /// <summary>
        /// Factory method to create a material from a single base color with reasonable defaults.
        /// Applies standard ratios: 10% ambient, 30% diffuse, 50% specular with moderate shininess.
        /// Useful for quickly creating materials without manually balancing lighting components.
        /// </summary>
        /// <param name="color">Base color to derive material properties from.</param>
        /// <returns>Material with balanced lighting properties based on the input color.</returns>
        public static Material FromColor(Color color)
        {
            return new Material(color*0.1, color*0.3, color*0.5, 100);
        }
    }
}
