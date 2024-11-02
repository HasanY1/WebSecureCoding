using System.Reflection;
using Ganss.Xss;

namespace PostService.Utilities
{
    public static class SanitizationHelper
    {
        private static readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

        public static bool SanitizeRequest<T>(T request)
        {
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    var currentValue = property.GetValue(request) as string;
                    if (currentValue != null)
                    {
                        // Sanitize the string property
                        var sanitizedValue = _sanitizer.Sanitize(currentValue);
                        property.SetValue(request, sanitizedValue.Trim()); // Trim whitespace if needed
                    }
                    else
                    {
                        return false; // Return false if the current value is null
                    }
                }
            }

            // Check if any sanitized value is empty after trimming
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    var sanitizedValue = property.GetValue(request) as string;
                    if (string.IsNullOrEmpty(sanitizedValue))
                    {
                        return false; // Return false if any sanitized value is empty
                    }
                }
            }

            return true; // Return true if all properties are valid
        }
    }
}
