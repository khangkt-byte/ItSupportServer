using static ItSupportServer.src.Modules.Ingredient.IngredientEnum;

namespace ItSupportServer.src.Shared.Helper
{
    public class UnitsHelper
    {
        public static float ConvertUnits(float n1, string t1, string t2)
        {
            if (t1 == t2) return n1;

            if (t1 == UNIT_TYPE.cai.ToString() ||
                t1 == UNIT_TYPE.qua.ToString() ||
                t1 == UNIT_TYPE.phan.ToString() ||
                t2 == UNIT_TYPE.cai.ToString() ||
                t2 == UNIT_TYPE.qua.ToString() ||
                t2 == UNIT_TYPE.phan.ToString()) return n1;

            if (t1 == UNIT_TYPE.gram.ToString() && t2 == UNIT_TYPE.kilogram.ToString()) return (float)n1 / 1000f;
            else if (t1 == UNIT_TYPE.kilogram.ToString() && t2 == UNIT_TYPE.gram.ToString()) return (float)n1 * 1000f;
            else if (t1 == UNIT_TYPE.liter.ToString() && t2 == UNIT_TYPE.milliliter.ToString()) return (float)n1 * 1000f;
            else if (t1 == UNIT_TYPE.milliliter.ToString() && t2 == UNIT_TYPE.liter.ToString()) return (float)n1 / 1000f;
            else return -1;
        }

        public static bool IsInteger(float number)
        {
            if (number % 1 == 0 && number >= 0) return true;
            else return false;
        }

        public static bool IsInteger(double number)
        {
            if (number % 1 == 0 && number >= 0) return true;
            else return false;
        }
    }
}
