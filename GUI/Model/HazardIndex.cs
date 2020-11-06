using ResTB.Translation;
using System.Windows.Media;

namespace ResTB.GUI.Model
{
    public class HazardIndex
    {
        public int Index { get; set; }
        public Color IntensityColor { get; private set; }
        public Color IndexColor { get; private set; }

        public HazardIndex(int index)
        {
            this.Index = index;

            switch (index)
            {
                case 1:
                case 2:
                    IntensityColor = Colors.LightGreen;
                    IndexColor = Colors.Green;
                    break;
                case 3:
                    IntensityColor = Colors.LightGreen;
                    IndexColor = Colors.Yellow;
                    break;
                case 4:
                case 5:
                case 6:
                    IntensityColor = Colors.LimeGreen;
                    IndexColor = Colors.Yellow;
                    break;
                case 7:
                case 8:
                case 9:
                    IntensityColor = Colors.DarkGreen;
                    IndexColor = Colors.Red;
                    break;
                default:
                    break;
            }
        }
    }
}