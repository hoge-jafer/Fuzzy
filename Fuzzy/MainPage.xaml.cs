using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace Fuzzy
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CanvasRadialGradientBrush canvasRadialGradientBrush;

        private CanvasBitmap image;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Canvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            //创建径向渐变画笔的实例;中心将是透明的, 极端不透明的黑色
            canvasRadialGradientBrush = new CanvasRadialGradientBrush(sender,Colors.Transparent,Colors.Black);

            //加载要绘制的图像。
            args.TrackAsyncAction(Task.Run(async()=> { image = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///wallhaven-766453.jpg")); }).AsAsyncAction());
        }

        private void Canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.ICanvasAnimatedControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasAnimatedDrawEventArgs args)
        {
            //开始绘图会话, 并清除白色背景
            var seesion = args.DrawingSession;
            args.DrawingSession.Clear(Colors.White);

            //将径向渐变的中心设置为图像的中心。
            canvasRadialGradientBrush.Center = new System.Numerics.Vector2((float)(image.Size.Width/2.0f),(float)(image.Size.Height/2.0f));

            //从滑块控制中确定渐变半径。
            canvasRadialGradientBrush.RadiusY = canvasRadialGradientBrush.RadiusX = (float)200;//调整参数，也可以换成slider的Value

            //首先绘制未更改的图像。
            seesion.DrawImage(image,image.Bounds);

            //创建一个图层, 这样绘制的所有元素都会受到透明遮罩的影响
            //在我们的例子是径向梯度。
            using (seesion.CreateLayer(canvasRadialGradientBrush))
            {
                //创建高斯模糊效果。
                using (var blurEffect=new GaussianBlurEffect())
                {
                    //将图像设置为模糊
                    blurEffect.Source = image;
                    //设置滑块控件的模糊量。
                    blurEffect.BlurAmount = (float)7;//调整参数，也可以换成slider的Value
                    //明确设置优化模式的最高质量, 因为我们使用的是大模糊量值。
                    blurEffect.Optimization = EffectOptimization.Quality;
                    //这样可以防止模糊效果环绕
                    blurEffect.BorderMode = EffectBorderMode.Hard;
                    //在未改变的图像上绘制模糊图像。它将被径向梯度遮挡
                    //从而在中间显示一个透明的孔, 并正确地覆盖 alpha 值。
                    seesion.DrawImage(blurEffect,0,0);
                }
            }
            Canvas.Invalidate();
        }
    }
}
