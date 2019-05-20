using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace TWPoster
{
    public class CardMerge
    {
        public CardMerge(List<string> imagesPath)
        {
            this.Images = new List<Image>();

            foreach (var item in imagesPath)
                Images.Add(Image.FromFile(item));
        }
        

        public void SetWallpaper(Image wp)
        {
            this.Wallpaper = wp;
        }

        List<Image> Images = new List<Image>();

        public Image Wallpaper { get; set; } = null;

        /// <summary>
        /// This method merges cards to draw a deck.
        /// </summary>
        /// <returns>Merged image path</returns>
        public string GetMerge()
        {
            int widthSpaces = 110;
            int heightSpaces = 100;
            int cardWidth = 350;
            int cardHeight = 360;

            int imageWidth = (widthSpaces * 5) + (cardWidth * 4);
            int imageHeight = (heightSpaces * 4) + (cardHeight * 2);

            Image firstImage = Images.First();
            String resultPath = "output.png";

            Bitmap img3 = new Bitmap("wallpaper.png");

            Graphics g = Graphics.FromImage(img3);
            //g.Clear(Color.Black);

            for (int i = 0; i < Images.Count; i++)
            {
                if (i >= 0 && i < 4)
                {
                    if (i == 0)
                        g.DrawImage(Images[i], new System.Drawing.Point(widthSpaces, heightSpaces));
                    else
                        g.DrawImage(Images[i], new System.Drawing.Point(((widthSpaces + cardWidth) * i) + widthSpaces, heightSpaces));
                }
                else
                {
                    if (i == 4)
                        g.DrawImage(Images[i], new System.Drawing.Point(widthSpaces, (heightSpaces * 2) + cardHeight));
                    else
                        g.DrawImage(Images[i], new System.Drawing.Point(((widthSpaces + cardWidth) * (i - 4)) + widthSpaces, (heightSpaces * 2) + cardHeight));
                }
            }

            g.Dispose();


            img3.Save(resultPath, System.Drawing.Imaging.ImageFormat.Png);
            img3.Dispose();

            return resultPath;
        }

    }
}
