using System.Drawing;
using System.Drawing.Imaging;
using dlog.server.Infrastructure.Data.Repo.Helpers;

namespace dlog.server.Helpers
{
    public class CustomHelpers
    {
        public static async Task<Bitmap> Base64ToBitmap(IFormFile file)
        {
            try
            {
                Bitmap bmpReturn = null;
                string base64String = "";
                using (var stream = new StreamReader(file.OpenReadStream()))
                {
                    base64String = stream.ReadToEnd();
                    base64String = base64String.Split(',')[1];
                }
                byte[] byteBuffer = Convert.FromBase64String(base64String);
                using (var ms = new MemoryStream(byteBuffer))
                {
                    bmpReturn = (Bitmap)Bitmap.FromStream(ms);
                }
                return bmpReturn;
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }
        public static async Task<Bitmap> ResizeImage(Bitmap original, int width, int height)
        {
            try
            {
                Bitmap resizedImage;

                int rectHeight = width;
                int rectWidth = height;

                if (original.Height == original.Width)
                {
                    resizedImage = new Bitmap(original, rectHeight, rectHeight);
                }
                else
                {
                    float aspect = original.Width / (float)original.Height;
                    int newWidth, newHeight;
                    newWidth = (int)(rectWidth * aspect);
                    newHeight = (int)(newWidth / aspect);

                    if (newWidth > rectWidth || newHeight > rectHeight)
                    {
                        if (newWidth > newHeight)
                        {
                            newWidth = rectWidth;
                            newHeight = (int)(newWidth / aspect);
                        }
                        else
                        {
                            newHeight = rectHeight;
                            newWidth = (int)(newHeight * aspect);
                        }
                    }
                    resizedImage = new Bitmap(original, newWidth, newHeight);
                }
                return resizedImage;
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }

        public static string PathFromUserID(string UserID)
        {
            return "/" + string.Join("/", UserID.ToArray()) + "/";
        }

        public static async Task<bool> WriteImage(Bitmap bitmap, string path, string filename)
        {
            try
            {
                Directory.CreateDirectory(path);
                using (Bitmap bmp = new Bitmap(bitmap))
                {
                    bmp.Save(path + filename, ImageFormat.Jpeg);
                }
                return true;
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return false;
            }
        }
        public static async Task<string> SaveUserAvatar(int UserID, string envPath, IFormFile file)
        {
            try
            {
                Bitmap original;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    using (var img = Image.FromStream(memoryStream))
                    {
                        original = new Bitmap(img);
                    }
                }
                //var original = Base64ToBitmap(file);
                var small = await ResizeImage(original, 220, 220);
                var large = await ResizeImage(original, 1000, 1000);
                var userPath = PathFromUserID(UserID.ToString());
                var savePath = envPath + "/media/avatars/user" + userPath;
                if (small != null && large != null)
                {
                    var writeSmall = await WriteImage(small, savePath, "ua-small.jpg");
                    var writeLarge = await WriteImage(large, savePath, "ua-large.jpg");

                    if (writeSmall && writeLarge)
                    {
                        return userPath;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }
    }
}
