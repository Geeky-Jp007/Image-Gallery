using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using C1.Win.C1Tile;

namespace Image_Gallery_Demo
{
    public partial class ImageGallery : Form
    {
        DataFetcher datafetch = new DataFetcher();
        List<ImageItem> imagesList;
        int checkedItems = 0;
        C1.C1Pdf.C1PdfDocument imagePdfDocument = new C1.C1Pdf.C1PdfDocument();

        public ImageGallery()
        {
            InitializeComponent();
            
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Rectangle r = _searchBox.Bounds;
            r.Inflate(3, 3);
            Pen p = new Pen(Color.LightGray);
            e.Graphics.DrawRectangle(p, r);
        }

        private async void _search_Click(object sender, EventArgs e)
        {
            statusStrip1.Visible = true;
            imagesList = await datafetch.GetImageData(_searchBox.Text);
            AddTiles(imagesList);
            statusStrip1.Visible = false;
        }


        private void AddTiles(List<ImageItem> imageList)
        {
            _imageTileControl.Groups[0].Tiles.Clear();
            foreach (var imageitem in imageList)
            {
                Tile tile = new Tile();
                
                Image img = Image.FromStream(new MemoryStream(imageitem.Base64));
                float imgHeight = img.Height;
                float imgWidth = img.Width;
                tile.VerticalSize = 2;
                tile.HorizontalSize = (int)Math.Round((imgWidth * tile.VerticalSize) / imgHeight);
                _imageTileControl.Groups[0].Tiles.Add(tile);

                
                Template tl = new Template();
                ImageElement ie = new ImageElement();
                ie.ImageLayout = ForeImageLayout.Stretch;
                tl.Elements.Add(ie);
                tile.Template = tl;
                tile.Image = img;
            }
        }

        private void _exportImage_Click(object sender, EventArgs e)
        {
            List<Image> images = new List<Image>();
            foreach (Tile tile in _imageTileControl.Groups[0].Tiles)
            {
                if (tile.Checked)
                {
                    images.Add(tile.Image);
                }
            }
            ConvertToPdf(images);
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.DefaultExt = "pdf";
            saveFile.Filter = "PDF files (*.pdf)|*.pdf*";

            if (saveFile.ShowDialog() == DialogResult.OK)
            {

                imagePdfDocument.Save(saveFile.FileName);

            }
        }

        private void ConvertToPdf(List<Image> images)
        {
            RectangleF rect = imagePdfDocument.PageRectangle;
            bool firstPage = true;
            rect.Inflate(-72, -72);
            foreach (var selectedimg in images)
            {
                if (!firstPage)
                {
                    imagePdfDocument.NewPage();
                }
                firstPage = false;
                
                imagePdfDocument.DrawImage(selectedimg, rect);
            }

        }

        private void _exportImage_Paint(object sender, PaintEventArgs e)
        {
            Rectangle r = new Rectangle(_exportImage.Location.X,
            _exportImage.Location.Y, _exportImage.Width, _exportImage.Height);
            r.X -= 29;
            r.Y -= 3;
            r.Width--;
            r.Height--;
            Pen p = new Pen(Color.LightGray);
            e.Graphics.DrawRectangle(p, r);
            e.Graphics.DrawLine(p, new Point(0, 43), new
           Point(this.Width, 43));
        }

        private void _imageTileControl_TileChecked(object sender, C1.Win.C1Tile.TileEventArgs e)
        {
            checkedItems++;
            _exportImage.Visible = true;
            _saveImage.Visible = true;
        }

        private void _imageTileControl_TileUnchecked(object sender, C1.Win.C1Tile.TileEventArgs e)
        {
            checkedItems--;
            _exportImage.Visible = checkedItems > 0;
            _saveImage.Visible = checkedItems > 0;
        }

        private void _imageTileControl_Paint(object sender, PaintEventArgs e)
        {
            Pen p = new Pen(Color.LightGray);
            e.Graphics.DrawLine(p, 0, 43, 800, 43);
        }

        private void toolStripProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void _saveImage_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
            int count = 0;

            if(folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (Tile tile in _imageTileControl.Groups[0].Tiles)
                {
                    if (tile.Checked)
                    {
                        var bitmap = new Bitmap(tile.Image);
                     //   System.Console.WriteLine(folderBrowserDialog.SelectedPath + "\\" + count + "jpg");
                        bitmap.Save(folderBrowserDialog.SelectedPath + "\\" + _searchBox.Text + count + ".jpg");
                        count++;
                    }
                }
            }
            
        }

        private async void ImageGallery_Load(object sender, EventArgs e)
        {
            statusStrip1.Visible = true;
            imagesList = await datafetch.GetImageData("");
            AddTiles(imagesList);
            statusStrip1.Visible = false;
        }

        private void _uploadImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter =
        "Images (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF";

            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _imageTileControl.Groups[0].Tiles.Clear();

                foreach (String file in openFileDialog.FileNames)
                {
                    Tile tile = new Tile();

                    Image img = Image.FromFile(file);
                    float imgHeight = img.Height;
                    float imgWidth = img.Width;
                    tile.VerticalSize = 2;
                    tile.HorizontalSize = (int)Math.Round((imgWidth * tile.VerticalSize) / imgHeight);
                    _imageTileControl.Groups[0].Tiles.Add(tile);

                    Template tl = new Template();
                    ImageElement ie = new ImageElement();
                    ie.ImageLayout = ForeImageLayout.Stretch;
                    tl.Elements.Add(ie);
                    tile.Template = tl;
                    tile.Image = img;
                }
            }
        }
    }
}
