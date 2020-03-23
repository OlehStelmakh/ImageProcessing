using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using Microsoft.Win32;
using Color = System.Drawing.Color;

namespace WpfApp1
{
    public class MainCalculations : INotifyPropertyChanged
    {
        // Наша картинка
        public ImageSource Image { get; private set; }
        public Bitmap imageCache { set; get; }
        readonly OpenFileDialog openFileDialog;
        readonly ICommand openFileDialogCommand;
        public ICommand OpenFileDialogCommand { get { return openFileDialogCommand; } }

        readonly ICommand binaryImageCommand;
        public ICommand BinaryImageCommand { get { return binaryImageCommand; } }

        readonly ICommand grayScaleImageCommand;
        public ICommand GrayScaleImageCommand { get { return grayScaleImageCommand; } }

        readonly ICommand undoChangesImageCommand;
        public ICommand UndoChangesImageCommand { get { return undoChangesImageCommand; } }

        readonly ICommand maskImageCommand;
        public ICommand MaskImageCommand { get { return maskImageCommand; } }

        public Bitmap bm { set; get; }

        public MainCalculations()
        {
            openFileDialogCommand = new Command(ExecuteOpenFileDialog);
            openFileDialog = new OpenFileDialog()
            {
                Multiselect = false,
                Filter = "Image files (*.BMP, *.JPG, *.GIF, *.TIF, *.PNG, *.ICO, *.EMF, *.WMF)|*.bmp;*.jpg;*.gif; *.tif; *.png; *.ico; *.emf; *.wmf"
            };

            binaryImageCommand = new Command(ExecuteBinaryImage);
            grayScaleImageCommand = new Command(ExecuteGrayScaleImage);
            maskImageCommand = new Command(ExecuteMaskImage);
            undoChangesImageCommand = new Command(ExecuteUndoChangesImage);
        }

        // Действие при нажатии на кнопку "Open File Dialog"
        void ExecuteOpenFileDialog()
        {
            if (openFileDialog.ShowDialog() == true)
            {
                using (var stream = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    Image = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                   
                    RaisePropertyChanged("Image");
                    bm = new Bitmap(stream);
                    imageCache = new Bitmap(stream);
                    HistogramConstructor.bitmap = new Bitmap(stream);
                    
                }
            }
        }
        
        //TODO continue ...
        void ExecuteBinaryImage()
        {
            if (bm != null)
            {
                for (int i = 0; i < bm.Height-1; i++)
                {
                    for (int j = 0; j < bm.Width-1; j++)
                    {
                        float E = 0;
                        float Y = (bm.GetPixel(j, i).R + bm.GetPixel(j, i).G + bm.GetPixel(j, i).B)/3;
                        Color color = Color.Black;
                        if(Y<128)
                        {
                            E = Y;
                        }
                        else
                        {
                            color = Color.White;
                            E = Y - 256;
                        }
                        bm.SetPixel(j, i, color);
                        int temp = Convert.ToInt32((bm.GetPixel(j + 1, i).R + bm.GetPixel(j + 1, i).G + bm.GetPixel(j + 1, i).B)/3 + E * 3 / 8);
                        bm.SetPixel(j + 1, i, Color.FromArgb(temp,temp,temp));
                        temp = Convert.ToInt32((bm.GetPixel(j, i + 1).R + bm.GetPixel(j, i + 1).G + bm.GetPixel(j, i + 1).B) / 3 + E * 3 / 8);
                        bm.SetPixel(j, i + 1, Color.FromArgb(temp, temp, temp));
                        temp = Convert.ToInt32((bm.GetPixel(j + 1, i + 1).R + bm.GetPixel(j + 1, i + 1).G + bm.GetPixel(j + 1, i + 1).B) / 3 + E / 4);
                        bm.SetPixel(j + 1, i + 1, Color.FromArgb(temp, temp, temp));
                    }
                }
                Image = ImageSourceFromBitmap(bm);
                RaisePropertyChanged("Image");
            }
            else
            {
                MessageBox.Show("Select an image!");
            }
        }

        /*
         for (x = xmin; x < xmax-1; x++)
         {
            for (y = ymin; y < ymax-1; y++) 
            { 
                if (I[x,y] < A) 
                { 
                    Out[x,y]=0; 
                    E=I[x,y]; 
                } 
                else 
                { 
                    Out[x,y]=1; 
                    E=I[x,y]-Imax; 
                }   
                I[x+1,y]+=E*3/8; 
                I[x,y+1]+=E*3/8; 
                I[x+1,y+1]+=E/4; 
            }
        }*/

        void ExecuteGrayScaleImage()
        {
            if (bm != null)
            {
                for (int i = 0; i < bm.Height; i++)
                {
                    for (int j = 0; j < bm.Width; j++)
                    {
                        int Y = Convert.ToInt32(bm.GetPixel(j, i).R * 0.3 + bm.GetPixel(j, i).G * 0.59 + bm.GetPixel(j, i).B * 0.11);
                        Color color = Color.FromArgb(255, Y, Y, Y);
                        bm.SetPixel(j, i, color);
                    }
                }
                Image = ImageSourceFromBitmap(bm);
                RaisePropertyChanged("Image");
            }
            else
            {
                MessageBox.Show("Select an image!");
            }
        }

        void ExecuteMaskImage()
        {
            if (bm !=null)
            {
                int[,] mask = new int[3, 3] { { -1, 1, 1 }, { 1, -2, -1 }, { -1, 1, 1 } };
                Bitmap bmMask = new Bitmap(bm);

                for (int i = 1; i < bm.Height-1; i++)
                {
                    for (int j = 1; j < bm.Width-1; j++)
                    {
                        int sum = 0;
                        try
                        {
                            Color pixel = bm.GetPixel(j - 1, i - 1);
                            sum += (pixel.R + pixel.G + pixel.B) * mask[0, 0];
                            pixel = bm.GetPixel(j, i - 1);
                            sum += (pixel.R + pixel.G + pixel.B) * mask[0, 1];
                            pixel = bm.GetPixel(j + 1, i - 1);
                            sum += (pixel.R + pixel.G + pixel.B) * mask[0, 2];
                            pixel = bm.GetPixel(j - 1, i);
                            sum += (pixel.R + pixel.G + pixel.B) * mask[1, 0];
                            pixel = bm.GetPixel(j, i);
                            sum += (pixel.R + pixel.G + pixel.B) * mask[1, 1];
                            pixel = bm.GetPixel(j + 1, i);
                            sum += (pixel.R + pixel.G + pixel.B) * mask[1, 2];
                            pixel = bm.GetPixel(j - 1, i + 1);
                            sum += (pixel.R + pixel.G + pixel.B) * mask[2, 0];
                            pixel = bm.GetPixel(j, i + 1);
                            sum += (pixel.R + pixel.G + pixel.B) * mask[2, 1];
                            pixel = bm.GetPixel(j + 1, i + 1);
                            sum += (pixel.R + pixel.G + pixel.B) * mask[2, 2];

                            sum = Math.Abs(sum) / 3;

                            Color color = Color.FromArgb(255, sum, sum, sum);
                            bmMask.SetPixel(j, i, color);
                        }
                        catch (Exception)
                        {
                            bmMask.SetPixel(j, i, Color.FromArgb(255, 0, 0, 0));
                        }
                    }
                }
                Image = ImageSourceFromBitmap(bmMask);
                RaisePropertyChanged("Image");
            }
            else
            {
                MessageBox.Show("Select an image!");
            }
        }

        void ExecuteUndoChangesImage()
        {
            if (bm != null)
            {
                bm = new Bitmap(imageCache);
                Image = ImageSourceFromBitmap(imageCache);
                RaisePropertyChanged("Image");
            }
            else
            {
                MessageBox.Show("Select an image!");
            }
        }

        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());          
        }

        // Реализация интерфейса INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Command : ICommand
    {
        public Command(Action action)
        {
            this.action = action;
        }

        Action action;

        EventHandler canExecuteChanged;
        event EventHandler ICommand.CanExecuteChanged
        {
            add { canExecuteChanged += value; }
            remove { canExecuteChanged -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action();
        }
    }
}
