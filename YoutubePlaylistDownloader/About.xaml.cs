﻿namespace YoutubePlaylistDownloader;

/// <summary>
/// Interaction logic for About.xaml
/// </summary>
public partial class About : UserControl
{
	public About()
	{
		InitializeComponent();
		GlobalConsts.HideAboutButton();
		GlobalConsts.ShowHomeButton();
		GlobalConsts.ShowSettingsButton();
		GlobalConsts.ShowHelpButton();

		AboutRun.Text += GlobalConsts.VERSION;
	}
    
	private async void UpdateButton_Click(object sender, RoutedEventArgs e)
	{
		try
		{
			using var client = new HttpClient();
			client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { NoCache = true };
			
			string gitHubUrl = String.Format("https://raw.githubusercontent.com/{0}/{1}/{2}/{3}/",
				GlobalConsts.APPLICATION_GITHUB_USERNAME,
                GlobalConsts.APPLICATION_REPOSITORY_NAME,
                GlobalConsts.APPLICATION_REPOSITORY_BRANCH,
                GlobalConsts.APPLICATION_REPOSITORY_FOLDER
                );

            var response = await client.GetStringAsync(gitHubUrl + "latestVersionWithRevision.txt");
			var latestVersion = Version.Parse(response);

			if (latestVersion > GlobalConsts.VERSION)
			{
				var changelog = await client.GetStringAsync(gitHubUrl + "changelog.txt");
				var dialogSettings = new MetroDialogSettings()
				{
					AffirmativeButtonText = $"{FindResource("UpdateNow")}",
					NegativeButtonText = $"{FindResource("No")}",
					FirstAuxiliaryButtonText = $"{FindResource("UpdateWhenIExit")}",
					ColorScheme = MetroDialogColorScheme.Theme,
					DefaultButtonFocus = MessageDialogResult.Affirmative,
				};
				var update = await GlobalConsts.Current.ShowMessageAsync($"{FindResource("NewVersionAvailable")}", $"{FindResource("DoYouWantToUpdate")}\n{changelog}",
						MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, dialogSettings);
				if (update == MessageDialogResult.Affirmative)
                {
                    GlobalConsts.LoadPage(new DownloadUpdate(latestVersion, changelog));
                }
                else if (update == MessageDialogResult.FirstAuxiliary)
				{
					GlobalConsts.UpdateControl = new DownloadUpdate(latestVersion, changelog, true).UpdateLaterStillDownloading();
				}
			}
			else
			{
				await GlobalConsts.ShowMessage($"{FindResource("NoUpdates")}", $"{FindResource("NoUpdatesAvailable")}");
			}
		}
		catch (Exception ex)
		{
			await GlobalConsts.Log(ex.ToString(), "About UpdateButton_Click");
			await GlobalConsts.ShowMessage($"{FindResource("Error")}", $"{FindResource("CannotUpdate")} {ex.Message}");
		}
	}
}
