namespace GymMate.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            foreach (var element in ButtonsCollection.VisibleViews)
            {
                if (element is VisualElement ve)
                    ve.Scale = 0;
            }

            await Task.Delay(200);

            foreach (var element in ButtonsCollection.VisibleViews)
            {
                if (element is VisualElement ve)
                    await ve.ScaleTo(1, 250, Easing.SinOut);
            }
        }
    }
}
