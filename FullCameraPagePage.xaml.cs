using System.IO;
using Xamarin.Forms;

namespace FullCameraPage
{
	public partial class FullCameraPagePage : ContentPage
	{
		public FullCameraPagePage()
		{
			InitializeComponent();
			TakePhotoButton.Clicked += TakePhotoButton_Clicked;
		}

		async void TakePhotoButton_Clicked(object sender, System.EventArgs e)
		{
			var cameraPage = new CameraPage();
			cameraPage.OnPhotoResult += CameraPage_OnPhotoResult;
			await Navigation.PushModalAsync(cameraPage);
		}

		async void CameraPage_OnPhotoResult(FullCameraPage.PhotoResultEventArgs result)
		{
			await Navigation.PopModalAsync();
			if (!result.Success)
				return;

			Photo.Source = ImageSource.FromStream(() => new MemoryStream(result.Image));
		}
	}
}
