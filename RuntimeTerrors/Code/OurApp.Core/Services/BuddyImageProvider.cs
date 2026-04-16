using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurApp.Core.Services
{
    public static class BuddyImageProvider
    {
        private static readonly Dictionary<int, string> BuddyImages = new Dictionary<int, string>
        {
            { 0, "Assets\\AvatarFemale.png" },
            { 1, "Assets\\AvatarMale.png" }
        };

        public static string GetImagePathById(int id)
        {
            if (BuddyImages.TryGetValue(id, out var path))
            {
                return path;
            }
            return BuddyImages[0];
        }
    }
}
