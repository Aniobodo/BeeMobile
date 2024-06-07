
/* Nicht gemergte Änderung aus Projekt "BeeMobileApp (net8.0-windows10.0.19041.0)"
Vor:
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
Nach:
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
*/

/* Nicht gemergte Änderung aus Projekt "BeeMobileApp (net8.0-ios)"
Vor:
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
Nach:
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
*/

/* Nicht gemergte Änderung aus Projekt "BeeMobileApp (net8.0-android)"
Vor:
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
Nach:
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
*/
namespace BeeMobileApp.Classes
{
    public class ARClass : View  // This class will redirect the app to local platform for Augmented reality renderisation. Check the local classes inside BeeMobile.Android(AndroidARRenderer) and BeeMobile.iOS
    {
        public List<float> GeomCoor { get; set; }                      // Contains the geometry coordinates of the structural member
        public List<List<float>> BarCoords { get; set; }                // Contains all the bar coordinates of the rendered carpet
        public List<List<float>> BandCoords { get; set; }               // Contains all the band coordinates of the rendered carpet
        public List<List<float>> RolloutBox { get; set; }               // Contains the coordinates of the box inside which the image for rollout symbol should be placed
        public List<List<float>> EdgeToCarpDimBox { get; set; }         // Contains the coordinates of the box inside which the image for dimension from edge of formwork to start of carpet should be placed
        public List<float> EdgeToCarpDim { get; set; }                  // Contains the coordinates of the dimension line from edge of formwork to start of carpet
        public float InitAngle { get; set; }                            // Angle between the vertical line and the vector created by the line on geometry
        public byte[] GeomImage { get; set; }                           // Geometry image
        public byte[] EdgeToCarpDimImage { get; set; }                  // Dimension image (dimension from edge of formwork to start of carpet)
        public byte[] RolloutImage { get; set; }                        // Rollout symbol image
        public bool PlotCarpet { get; set; }                            // This tells if in Augmented reality the carpet needs to be rendered or just the measurment is done
        public float ARScale { get; set; }                              // Scale of the rendered area. Changes if the person is in game mode

        public ARClass(List<float> geoCoord = null, List<List<float>> barCoords = null, List<List<float>> bandCoords = null, float angle = 0, byte[] bytes = null,
            bool isPlot = false, float scale = 0, List<List<float>> boxCoords = null, byte[] bamImage = null,
            List<List<float>> dimBox = null, List<float> dimCoord = null, byte[] dimImage = null)
        {
            GeomCoor = geoCoord; BarCoords = barCoords; BandCoords = bandCoords; InitAngle = angle; RolloutBox = boxCoords; RolloutImage = bamImage;
            GeomImage = bytes; PlotCarpet = isPlot; ARScale = scale; EdgeToCarpDimBox = dimBox; EdgeToCarpDim = dimCoord; EdgeToCarpDimImage = dimImage;
        }
    }
}
