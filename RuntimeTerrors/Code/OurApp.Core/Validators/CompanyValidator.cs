using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.Validators
{
    public class CompanyValidator
    {
        public bool NameValidator(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("Name is mandatory");
            }
            if (name.Length > 200)
            {
                throw new Exception("Name is too long");
            }
            return true;
        }

        public bool AboutUsValidator(string aboutus)
        {
            if (string.IsNullOrEmpty(aboutus))
            {
                return true;
            }
            if (aboutus.Length > 2000)
            {
                throw new Exception("AboutUs is too long");
            }
            return true;
        }

        public bool LocationValidator(string location)
        {
            if (string.IsNullOrEmpty(location))
            {
                return true;
            }
            if (location.Length > 300)
            {
                throw new Exception("Location is too long");
            }
            return true;
        }

        public bool EmailValidator(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return true;
            }
            if (!email.Contains("@"))
            {
                throw new Exception("Email must contain '@'");
            }
            if (email.Length > 100)
            {
                throw new Exception("Email is too long");
            }
            return true;
        }

        public bool PfpValidator(string pfp)
        {
            if (string.IsNullOrEmpty(pfp))
            {
                return true;
            }
            if (!HasAllowedImageExtension(pfp))
            {
                throw new Exception("Profile picture must be .jpg, .jpeg or .png");
            }
            return true;
        }

        public bool LogoValidator(string logo)
        {
            if (string.IsNullOrWhiteSpace(logo))
            {
                throw new Exception("Logo is mandatory");
            }
            if (!HasAllowedImageExtension(logo))
            {
                throw new Exception("Logo must be .jpg, .jpeg or .png");
            }
            return true;
        }

        private static bool HasAllowedImageExtension(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (value.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                || value.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                || value.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (value.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                var rest = value.Substring("data:image/".Length);
                var semicolonIndex = rest.IndexOf(';');
                var mimeSubtype = (semicolonIndex >= 0 ? rest.Substring(0, semicolonIndex) : rest)
                    .Trim()
                    .ToLowerInvariant();

                return mimeSubtype == "png"
                    || mimeSubtype == "jpeg"
                    || mimeSubtype == "jpg";
            }

            return false;
        }
    }
}
