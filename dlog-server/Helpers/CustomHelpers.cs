using SkiaSharp;
using dlog.server.Infrastructure.Data.Repo.Helpers;

namespace dlog.server.Helpers
{
    public class CustomHelpers
    {
        public static string PathFromUserID(string UserID)
        {
            return "/" + string.Join("/", UserID.ToArray()) + "/";
        }

        public static async Task<byte[]> ResizeImage(IFormFile file, int width, int height)
        {
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (SKMemoryStream sourceStream = new SKMemoryStream(stream.ToArray()))
                {
                    using (SKCodec codec = SKCodec.Create(sourceStream))
                    {
                        sourceStream.Seek(0);
                        using (SKImage image = SKImage.FromEncodedData(SKData.Create(sourceStream)))
                        {
                            int newHeight = image.Height;
                            int newWidth = image.Width;

                            if (height > 0 && newHeight > height)
                            {
                                double scale = (double)height / newHeight;
                                newHeight = height;
                                newWidth = (int)Math.Floor(newWidth * scale);
                            }
                            if (width > 0 && newWidth > width)
                            {
                                double scale = (double)width / newWidth;
                                newWidth = width;
                                newHeight = (int)Math.Floor(newHeight * scale);
                            }

                            var info = codec.Info.ColorSpace.IsSrgb ? new SKImageInfo(newWidth, newHeight) : new SKImageInfo();
                            using (SKSurface surface = SKSurface.Create(info))
                            {
                                using (SKPaint paint = new SKPaint())
                                {
                                    paint.IsAntialias = true;
                                    paint.FilterQuality = SKFilterQuality.High;

                                    surface.Canvas.Clear(SKColors.White);
                                    var rect = new SKRect(0, 0, newWidth, newHeight);
                                    surface.Canvas.DrawImage(image, rect, paint);
                                    surface.Canvas.Flush();

                                    using (SKImage newImage = surface.Snapshot())
                                    {
                                        using (SKData newImageData = newImage.Encode())
                                        {
                                            return newImageData.ToArray();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static async Task<bool> WriteImage(byte[] file, string savePath, string fileName)
        {
            Directory.CreateDirectory(savePath);
            using (var fs = new FileStream(savePath + fileName, FileMode.Create, FileAccess.Write))
            {
                await fs.WriteAsync(file);
                return true;
            }
        }

        public static async Task<string> SaveUserAvatar(int UserID, string envPath, IFormFile file)
        {
            try
            {
                var userPath = PathFromUserID(UserID.ToString());
                var savePath = envPath + "/media/avatars/user" + userPath;

                var small = await ResizeImage(file, 220, 220);
                var large = await ResizeImage(file, 800, 800);
                if (small != null && large != null)
                {
                    var writeSmall = await WriteImage(small, savePath, "ua-small.jpg");
                    var writeLarge = await WriteImage(large, savePath, "ua-large.jpg");
                    if (writeSmall && writeLarge)
                    {
                        System.Diagnostics.Debug.WriteLine("writeSmall" + writeSmall);
                        return userPath;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                await new LogsRepository().CreateLog(ex);
                return null;
            }
        }
    }
}
