using System;
using Android.App;
using Android.Graphics;
using Android.Views;
using Xamarin.Forms.Platform.Android;
using Android.Widget;
using Android.Content;
using FullCameraPage;
using System.Linq;
using Android.Hardware;
using System.Threading.Tasks;
using FullCameraPage.Droid;

[assembly: Xamarin.Forms.ExportRenderer(typeof(CameraPage), typeof(CameraPageRenderer))]
namespace FullCameraPage.Droid
{
	public class CameraPageRenderer : PageRenderer, TextureView.ISurfaceTextureListener
	{
		RelativeLayout mainLayout;
		TextureView liveView;
		PaintCodeButton capturePhotoButton;

		Android.Hardware.Camera camera;

		Activity Activity => this.Context as Activity;

		protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Page> e)
		{
			base.OnElementChanged(e);
			SetupUserInterface();
			SetupEventHandlers();
		}

		void SetupUserInterface()
		{
			mainLayout = new RelativeLayout(Context);

			//RelativeLayout.LayoutParams mainLayoutParams = new RelativeLayout.LayoutParams(
			//	RelativeLayout.LayoutParams.MatchParent,
			//	RelativeLayout.LayoutParams.MatchParent);
			//mainLayout.LayoutParameters = mainLayoutParams;

			liveView = new TextureView(Context);

			RelativeLayout.LayoutParams liveViewParams = new RelativeLayout.LayoutParams(
				RelativeLayout.LayoutParams.MatchParent,
				RelativeLayout.LayoutParams.MatchParent);
			liveView.LayoutParameters = liveViewParams;
			mainLayout.AddView(liveView);

			capturePhotoButton = new PaintCodeButton(Context);
			RelativeLayout.LayoutParams captureButtonParams = new RelativeLayout.LayoutParams(
				RelativeLayout.LayoutParams.WrapContent,
				RelativeLayout.LayoutParams.WrapContent);
			captureButtonParams.Height = 120;
			captureButtonParams.Width = 120;
			capturePhotoButton.LayoutParameters = captureButtonParams;
			mainLayout.AddView(capturePhotoButton);

			AddView(mainLayout);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);
			if (!changed)
				return;
			var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
			var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);
			mainLayout.Measure(msw, msh);
			mainLayout.Layout(0, 0, r - l, b - t);

			capturePhotoButton.SetX( mainLayout.Width / 2 - 60);
			capturePhotoButton.SetY(mainLayout.Height - 200);
		}

		public void SetupEventHandlers()
		{
			capturePhotoButton.Click += async (sender, e) =>
			{
				var bytes = await TakePhoto();
				(Element as CameraPage).SetPhotoResult(bytes, liveView.Bitmap.Width, liveView.Bitmap.Height);
			};
			liveView.SurfaceTextureListener = this;
		}

		public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
		{
			if (keyCode == Keycode.Back)
			{
				(Element as CameraPage).Cancel();
				return false;
			}
			return base.OnKeyDown(keyCode, e);
		}

		public async Task<byte[]> TakePhoto()
		{
			camera.StopPreview();
			var ratio = ((decimal)Height) / Width;
			var image = Bitmap.CreateBitmap(liveView.Bitmap, 0, 0, liveView.Bitmap.Width, (int)(liveView.Bitmap.Width * ratio));
			byte[] imageBytes = null;
			using (var imageStream = new System.IO.MemoryStream())
			{
				await image.CompressAsync(Bitmap.CompressFormat.Jpeg, 50, imageStream);
				image.Recycle();
				imageBytes = imageStream.ToArray();
			}
			camera.StartPreview();
			return imageBytes;
		}

		private void StopCamera()
		{
			camera.StopPreview();
			camera.Release();
		}

		private void StartCamera()
		{
			camera.SetDisplayOrientation(90);
			camera.StartPreview();
		}


		#region TextureView.ISurfaceTextureListener implementations

		public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
		{
			camera = Android.Hardware.Camera.Open();
			var parameters = camera.GetParameters();
			var aspect = ((decimal)height) / ((decimal)width);

			// Find the preview aspect ratio that is closest to the surface aspect
			var previewSize = parameters.SupportedPreviewSizes
										.OrderBy(s => Math.Abs(s.Width / (decimal)s.Height - aspect))
										.First();

			System.Diagnostics.Debug.WriteLine($"Preview sizes: {parameters.SupportedPreviewSizes.Count}");

			parameters.SetPreviewSize(previewSize.Width, previewSize.Height);
			camera.SetParameters(parameters);

			camera.SetPreviewTexture(surface);
			StartCamera();
		}

		public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface)
		{
			StopCamera();
			return true;
		}

		public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int width, int height)
		{
		}

		public void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface)
		{
		}
		#endregion
	}

	public class PaintCodeButton : Button
	{
		public PaintCodeButton(Context context) : base(context)
		{
			Background.Alpha = 0;
		}


		protected override void OnDraw(Canvas canvas)
		{
			var frame = new Rect(Left, Top, Right, Bottom);

			Paint paint;
			// Local Colors
			var color = Color.White;

			RectF bezierRect = new RectF(
				frame.Left + (float)Java.Lang.Math.Floor((frame.Width() - 120f) * 0.5f + 0.5f),
				frame.Top + (float)Java.Lang.Math.Floor((frame.Height() - 120f) * 0.5f + 0.5f),
				frame.Left + (float)Java.Lang.Math.Floor((frame.Width() - 120f) * 0.5f + 0.5f) + 120f,
				frame.Top + (float)Java.Lang.Math.Floor((frame.Height() - 120f) * 0.5f + 0.5f) + 120f);
			Path bezierPath = new Path();
			bezierPath.MoveTo(frame.Left + frame.Width() * 0.5f, frame.Top + frame.Height() * 0.08333f);
			bezierPath.CubicTo(frame.Left + frame.Width() * 0.41628f, frame.Top + frame.Height() * 0.08333f, frame.Left + frame.Width() * 0.33832f, frame.Top + frame.Height() * 0.10803f, frame.Left + frame.Width() * 0.27302f, frame.Top + frame.Height() * 0.15053f);
			bezierPath.CubicTo(frame.Left + frame.Width() * 0.15883f, frame.Top + frame.Height() * 0.22484f, frame.Left + frame.Width() * 0.08333f, frame.Top + frame.Height() * 0.3536f, frame.Left + frame.Width() * 0.08333f, frame.Top + frame.Height() * 0.5f);
			bezierPath.CubicTo(frame.Left + frame.Width() * 0.08333f, frame.Top + frame.Height() * 0.73012f, frame.Left + frame.Width() * 0.26988f, frame.Top + frame.Height() * 0.91667f, frame.Left + frame.Width() * 0.5f, frame.Top + frame.Height() * 0.91667f);
			bezierPath.CubicTo(frame.Left + frame.Width() * 0.73012f, frame.Top + frame.Height() * 0.91667f, frame.Left + frame.Width() * 0.91667f, frame.Top + frame.Height() * 0.73012f, frame.Left + frame.Width() * 0.91667f, frame.Top + frame.Height() * 0.5f);
			bezierPath.CubicTo(frame.Left + frame.Width() * 0.91667f, frame.Top + frame.Height() * 0.26988f, frame.Left + frame.Width() * 0.73012f, frame.Top + frame.Height() * 0.08333f, frame.Left + frame.Width() * 0.5f, frame.Top + frame.Height() * 0.08333f);
			bezierPath.Close();
			bezierPath.MoveTo(frame.Left + frame.Width(), frame.Top + frame.Height() * 0.5f);
			bezierPath.CubicTo(frame.Left + frame.Width(), frame.Top + frame.Height() * 0.77614f, frame.Left + frame.Width() * 0.77614f, frame.Top + frame.Height(), frame.Left + frame.Width() * 0.5f, frame.Top + frame.Height());
			bezierPath.CubicTo(frame.Left + frame.Width() * 0.22386f, frame.Top + frame.Height(), frame.Left, frame.Top + frame.Height() * 0.77614f, frame.Left, frame.Top + frame.Height() * 0.5f);
			bezierPath.CubicTo(frame.Left, frame.Top + frame.Height() * 0.33689f, frame.Left + frame.Width() * 0.0781f, frame.Top + frame.Height() * 0.19203f, frame.Left + frame.Width() * 0.19894f, frame.Top + frame.Height() * 0.10076f);
			bezierPath.CubicTo(frame.Left + frame.Width() * 0.28269f, frame.Top + frame.Height() * 0.03751f, frame.Left + frame.Width() * 0.38696f, frame.Top, frame.Left + frame.Width() * 0.5f, frame.Top);
			bezierPath.CubicTo(frame.Left + frame.Width() * 0.77614f, frame.Top, frame.Left + frame.Width(), frame.Top + frame.Height() * 0.22386f, frame.Left + frame.Width(), frame.Top + frame.Height() * 0.5f);
			bezierPath.Close();

			paint = new Paint();
			paint.SetStyle(Android.Graphics.Paint.Style.Fill);
			paint.Color = (color);
			canvas.DrawPath(bezierPath, paint);

			paint = new Paint();
			paint.StrokeWidth = (1f);
			paint.StrokeMiter = (10f);
			canvas.Save();
			paint.SetStyle(Android.Graphics.Paint.Style.Stroke);
			paint.Color = (Color.Black);
			canvas.DrawPath(bezierPath, paint);
			canvas.Restore();

			RectF ovalRect = new RectF(
				frame.Left + (float)Java.Lang.Math.Floor(frame.Width() * 0.12917f) + 0.5f,
				frame.Top + (float)Java.Lang.Math.Floor(frame.Height() * 0.12083f) + 0.5f,
				frame.Left + (float)Java.Lang.Math.Floor(frame.Width() * 0.87917f) + 0.5f,
				frame.Top + (float)Java.Lang.Math.Floor(frame.Height() * 0.87083f) + 0.5f);
			Path ovalPath = new Path();
			ovalPath.AddOval(ovalRect, Path.Direction.Cw);

			paint = new Paint();
			paint.SetStyle(Android.Graphics.Paint.Style.Fill);
			paint.Color = (color);
			canvas.DrawPath(ovalPath, paint);

			paint = new Paint();
			paint.StrokeWidth = (1f);
			paint.StrokeMiter = (10f);
			canvas.Save();
			paint.SetStyle(Android.Graphics.Paint.Style.Stroke);
			paint.Color = (Color.Black);
			canvas.DrawPath(ovalPath, paint);
			canvas.Restore();
		}
	}
}