namespace ProjectReferencesManager.Tools
{
    public static class GUIDFormatter
    {
        public static string Format(string guid)
        {
            return guid.ToLower()
                       .Replace("{", "")
                       .Replace("}", "");
        }
    }
}