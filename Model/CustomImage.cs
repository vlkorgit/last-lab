using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

namespace Model
{
    public class CustomImage
    {


        public static float[] parseImage(string filename, byte[] blob)
        {
            if (filename == null || blob == null) return null;
            float[] mean_vec = new float[3] { 0.485f, 0.456f, 0.406f };
            float[] stddev_vec = new float[3] { 0.229f, 0.224f, 0.225f };
            const int R = 0, G = 224 * 224, B = 224 * 224 * 2;
            float[] arr = null;
            int header = 0;
            //try
            //{
            MemoryStream ms = new MemoryStream(blob);
            Image image = new Bitmap(Image.FromStream(ms), new Size(224, 244));
            image.RotateFlip(RotateFlipType.RotateNoneFlipY);
            ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            blob = ms.ToArray();
            ms.Close();
            header = blob.Length - G * 4;
            arr = new float[G * 3];
            for (int i = 0; i < G; i++)
            {//bgr или rgb?? результаты неоднозначны
                arr[R + i] = (blob[i * 4 + header] / 255f - mean_vec[0]) / (stddev_vec[0]);
                arr[G + i] = (blob[i * 4 + 1 + header] / 255f - mean_vec[1]) / (stddev_vec[1]);
                arr[B + i] = (blob[i * 4 + 2 + header] / 255f - mean_vec[2]) / (stddev_vec[2]);
            }
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine("CANNOT PARSE " + filename);
            //    blob = null;
            //}

            return arr;
        }
    }
}
