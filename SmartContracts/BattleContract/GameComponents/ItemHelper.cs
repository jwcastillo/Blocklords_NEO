
namespace BattleContract.GameComponents
{
    public class Helper
    {
        public static QualityType StringToQualityType(string value)
        {
            if (value.Equals("1"))
            {
                return QualityType.SSR;
            }
            else if (value.Equals("2"))
            {
                return QualityType.SR;
            }
            return QualityType.R;
        }
        public static string QualityTypeToString(QualityType qualityType)
        {
            if (qualityType.Equals(QualityType.SSR))
            {
                return "1";
            }
            if (qualityType.Equals(QualityType.SR))
            {
                return "2";
            }
            return "3";
        }
    }
}
