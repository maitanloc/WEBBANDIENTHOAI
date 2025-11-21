using System;

namespace WEBBANDIENTHOAI.Helpers
{
    public static class ImageHelper
    {
        // Simple magic bytes detection for common image types
        public static string GetContentType(byte[] data)
        {
            if (data == null || data.Length < 4) return "application/octet-stream";

            // JPEG (FF D8)
            if (data[0] == 0xFF && data[1] == 0xD8) return "image/jpeg";

            // PNG (89 50 4E 47)
            if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47) return "image/png";

            // GIF (47 49 46 38)
            if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x38) return "image/gif";

            // BMP (42 4D)
            if (data[0] == 0x42 && data[1] == 0x4D) return "image/bmp";

            return "application/octet-stream";
        }
    }
}
