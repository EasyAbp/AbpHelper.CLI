using System.Text;

namespace DosSEdo.AbpHelper.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendLineWithControlChar(this StringBuilder stringBuilder, StringBuilder sb, string newLine)
        {
            stringBuilder = AppendWithControlChar(stringBuilder, sb.ToString());
            return stringBuilder.Append(newLine);
        }
        
        public static StringBuilder AppendLineWithControlChar(this StringBuilder stringBuilder, string str, string newLine)
        {
            stringBuilder = AppendWithControlChar(stringBuilder, str);
            return stringBuilder.Append(newLine);
        }

        public static StringBuilder AppendWithControlChar(this StringBuilder stringBuilder, StringBuilder sb)
        {
            return AppendWithControlChar(stringBuilder, sb.ToString());
        }
        
        public static StringBuilder AppendWithControlChar(this StringBuilder stringBuilder, string str)
        {
            if (str.Contains('\b'))
            {
                foreach (char c in str)
                {
                    if (c == '\b')
                    {
                        stringBuilder.Length--;
                    }
                    else
                    {
                        stringBuilder.Append(c);
                    }
                }
            }
            else
            {
                stringBuilder.Append(str);
            }

            return stringBuilder;
        }
    }
}