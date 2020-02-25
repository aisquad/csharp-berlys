using System;
using System.Text.RegularExpressions;

namespace capp1
{
    public sealed class Convert
    {
        private static Convert instance = null;
        private static readonly object Instancelock = new object();

        private Convert() { }

        public static Convert GetInstance
        {
            get
            {
                if (instance == null)
                {
                    lock (Instancelock)
                    {
                        if (instance == null)
                        {
                            instance = new Convert();
                        }
                    }
                }
                return instance;
            }
        }

        public int ToInt(Group group)
        {
            return int.Parse(group.Value);
        }

        public float ToFloat(Group group)
        {
            return float.Parse(group.Value);
        }

        public string ToString(Group group)
        {
            return group.Value.Trim();
        }

        public DateTime ToDateTime(Group group)
        {
            return DateTime.ParseExact(group.Value, "dd.mm.yyyy", System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}
