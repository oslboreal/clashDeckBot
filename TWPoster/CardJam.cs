using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWPoster
{
    class CardMerge
    {
        public CardMerge(List<Image> Images)
        {
            this.Images = Images;
        }

        public void SetWallpaper(Image wp)
        {
            this.Wallpaper = wp;
        }

        List<Image> Images = new List<Image>();

        public Image Wallpaper { get; set; } = null;

        public Image GetMerge()
        {
            String jpg1 = "image1.png";
            String jpg2 = "image2.png";
            String jpg3 = "image3.png";

            Image img1 = Image.FromFile(jpg1);
            Image img2 = Image.FromFile(jpg2);


            int distancia = img1.Width + 100;
            int width = distancia * 8;
            int height = img1.Height * 2;

            Bitmap img3 = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(img3);

            g.Clear(Color.Black);
            // LOAD WALLPAPER HERE.
            g.DrawImage(img1, new System.Drawing.Point(10, 10));
            g.DrawImage(img2, new System.Drawing.Point(distancia, 0));

            g.Dispose();
            img1.Dispose();
            img2.Dispose();

            img3.Save(jpg3, System.Drawing.Imaging.ImageFormat.Png);
            img3.Dispose();

            return null;

        }

    }
}
