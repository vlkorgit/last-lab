using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Database
{
    public class ImageContext : DbContext
    {
        public DbSet<Image> Images { get; set; }
        public DbSet<Bitmap> Bitmaps { get; set; }
        public ImageContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=db11;Trusted_Connection=True;");
        }
    }
    public class DBprovider
    {
        AutoResetEvent are = new AutoResetEvent(true);
        public IEnumerable<Image> getImagesByClass(string classname)
        {
            Image[] array = null; ;
            using (ImageContext db = new ImageContext())
            {
                array = db.Images.Where(obj => obj.Class == classname).Include(obj => obj.Bitmap).ToArray();
            }
            return array;
        }
        public Image cant_name_it(string cla, int idx)
        {
            Image img = null;
            are.WaitOne();
            using (ImageContext db = new ImageContext())
            {
                img = db.Images.Where((obj) => (obj.Class == cla) ? true : false).FirstOrDefault();
                // img = q.ElementAtOrDefault(idx);
            }
            are.Set();
            return img;
        }
        public Image getNextImage()
        {
            are.WaitOne();
            Image image = null;
            using (ImageContext db = new ImageContext())
            {
                image = db.Images.Include(b => b.Bitmap).FirstOrDefault();
            }
            are.Set();
            return image;
        }
        public Image getPrevImage()
        {
            throw new NotImplementedException();
        }
        public void clear()
        {
            are.WaitOne();
            using (ImageContext db = new ImageContext())
            {
                db.Bitmaps.RemoveRange(db.Bitmaps);
                db.Images.RemoveRange(db.Images);
                db.SaveChanges();
            }
            are.Set();
        }
        public bool isExist(string filename)
        {
            are.WaitOne();
            using (ImageContext db = new ImageContext())
            {
                foreach (var img in db.Images)
                {
                    if (img.FullName == filename)
                    {
                        are.Set();
                        return true;
                    }
                }
            }
            are.Set();
            return false;
        }
        public void add(string filename, string clas, float prob, byte[] blob)
        {
            are.WaitOne();
            using (ImageContext db = new ImageContext())
            {
                //img.Bitmap = new Bitmap() { Id=img.Id, Blob = blob };
                Bitmap bitmap = new Bitmap() { Blob = blob };
                Image img = new Image() { Class = clas, FullName = filename, Probability = prob };
                img.Bitmap = bitmap;
                db.Images.Add(img);
                //db.Bitmaps.Add(bitmap);
                db.SaveChanges();
                //db.SaveChanges();
                //db.SaveChanges();
                Debug.WriteLine("ADD " + filename);
            }
            are.Set();
        }
        public Image getImage(string filename)
        {
            Image buf = null;
            are.WaitOne();
            using (ImageContext db = new ImageContext())
            {
                buf = db.Images.Where((obj) => (filename == obj.FullName) ? true : false).FirstOrDefault();
            }
            are.Set();
            return buf;
        }
        public bool compareBlob(string filename, byte[] blob)
        {
            bool f = false;
            are.WaitOne();
            using (ImageContext db = new ImageContext())
            {
                Image image = db.Images.Where((obj) => filename == obj.FullName).Include(b => b.Bitmap).FirstOrDefault();
                //if (image == null) Debug.WriteLine("IMAGE WAS NULL");
                //if (image.Bitmap == null) Debug.WriteLine("IMAGE>BITMAP WAS NULL");
                //if (image.Bitmap.Blob == null) Debug.WriteLine("IMAGE>BITMAP>BLOB WAS NULL");
                if (blob.SequenceEqual(image.Bitmap.Blob)) f = true;
                //способ через ключ
                //var bitmap =  db.Bitmaps.Find(image.BitmapId);
                //if (blob.SequenceEqual(bitmap.Blob)) f = true;
            }
            are.Set();
            return f;
        }
        public IEnumerable<KeyValuePair<string, int>> getClassStat()
        {
            Dictionary<string, int> pairs = new Dictionary<string, int>();
            are.WaitOne();
            using (ImageContext db = new ImageContext())
            {
                foreach (var img in db.Images)
                {
                    if (pairs.ContainsKey(img.Class))
                    {
                        pairs[img.Class]++;
                    }
                    else
                    {
                        pairs.Add(img.Class, 1);
                    }
                }
            }
            are.Set();
            return pairs;

        }
    }
    public class Image
    {
        [Key]
        [ForeignKey("Bitmap")]
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Class { get; set; }
        public float Probability { get; set; }
        public Bitmap Bitmap { get; set; }
    }
    public class Bitmap
    {
        [Key]
        public int Id { get; set; }
        public byte[] Blob { get; set; }
    }

}
