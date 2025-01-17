﻿namespace LaMuccaRossaVideoDownloader;

static class GlobalConsts
{
	public static Skeleton Current;
	public static MainPage MainPage;
	public static System.Windows.Media.Brush ErrorBrush;
	public static readonly string TempFolderPath;
	//public static string SaveDirectory;
	public static readonly string CurrentDir;
	public static readonly string FFmpegFilePath;
	private static readonly string _configFilePath;
	private static readonly string _errorFilePath;
	public static readonly Version VERSION = new(1, 9, 25);
	public static bool UpdateOnExit;
	public static string UpdateSetupLocation;
	public static bool UpdateFinishedDownloading;
	public static bool UpdateLater;
	public static DownloadUpdate UpdateControl;
	public static readonly string ChannelSubscriptionsFilePath;
	public static TimeSpan SubscriptionsUpdateDelay;
	private static DownloadSettings _downloadSettings;
	public static readonly string DownloadSettingsFilePath;
	public static readonly ObservableCollection<QueuedDownload> Downloads;
	private static SemaphoreSlim _conversionLocker;
	public static Objects.Settings settings;

	public const string APPLICATION_FOLDER_NAME = "LaMuccaRossaVideoDownloader";
    public const string APPLICATION_TEMP_FOLDER = "La_Mucca_Rossa_Video_Downloader";
    public const string APPLICATION_PUBLIC_NAME = "La Mucca Rossa Video Downloader";
	
	public const string APPLICATION_GITHUB_USERNAME = "dardizzola";
    public const string APPLICATION_REPOSITORY_NAME = "YouTubeDownloaderAllInOne";
    public const string APPLICATION_REPOSITORY_BRANCH = "main";
	public const string APPLICATION_REPOSITORY_FOLDER = "LaMuccaRossaVideoDownloader";

    public static string OppositeTheme => settings.Theme == "Light" ? "Dark" : "Light";
	public static YoutubeClient YoutubeClient => new();
	public static SemaphoreSlim ConversionsLocker { get => _conversionLocker; set => _conversionLocker ??= value; }
	public static DownloadSettings DownloadSettings
	{
		get
		{
			_downloadSettings ??= new DownloadSettings("mp3",
                false,
                YoutubeHelpers.High1080,
                false,
                false,
                false,
                false,
                "192",
                false,
                "it",
                false,
                false,
                0,
                0,
                false,
                true,
                false,
                true,
                4,
                "$title",
                false,
                "mp4");
			return _downloadSettings;
		}
		set
		{
			if (value != null)
			{
				_downloadSettings = value;

				if (settings.SaveDownloadOptions)
                {
                    File.WriteAllText(DownloadSettingsFilePath, JsonConvert.SerializeObject(_downloadSettings));
                }
            }
		}
	}


	static GlobalConsts()
	{
		JsonConvert.DefaultSettings = () =>
		{
			var settings = new JsonSerializerSettings();
			settings.Converters.Add(new VideoQualityConverter());
			return settings;
		};

		Downloads = [];
		CurrentDir = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString();
		FFmpegFilePath = $"{CurrentDir}\\ffmpeg.exe";
		var appDataPath = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "\\" + APPLICATION_REPOSITORY_NAME + "\\");
		_configFilePath = string.Concat(appDataPath, "Settings.json");
		_errorFilePath = string.Concat(appDataPath, "Errors.txt");
		DownloadSettingsFilePath = string.Concat(appDataPath, "DownloadSettings.json");
		ChannelSubscriptionsFilePath = string.Concat(appDataPath, "Subscriptions.ypds");

		if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        ErrorBrush = Brushes.Crimson;
		settings = new()
		{
			Language = "Italiano"
		};
		TempFolderPath = string.Concat(Path.GetTempPath(), APPLICATION_TEMP_FOLDER + "\\");
		UpdateOnExit = false;
		UpdateLater = false;
		UpdateSetupLocation = string.Empty;
		SubscriptionsUpdateDelay = TimeSpan.FromMinutes(1);
		Downloads.CollectionChanged += Downloads_CollectionChanged;
	}

	//The const methods are used mainly for saving/loading consts, and handling page\menu management.

	public static void HideHelpButton()
	{
		Current.HelpButton.Visibility = Visibility.Collapsed;
	}
	public static void HideHomeButton()
	{
		Current.HomeButton.Visibility = Visibility.Collapsed;
	}
	public static void HideAboutButton()
	{
		Current.AboutButton.Visibility = Visibility.Collapsed;
	}
	public static void HideSettingsButton()
	{
		Current.SettingsButton.Visibility = Visibility.Collapsed;
	}
	public static void ShowSettingsButton()
	{
		Current.SettingsButton.Visibility = Visibility.Visible;
	}
	public static void ShowHelpButton()
	{
		Current.HelpButton.Visibility = Visibility.Visible;
	}
	public static void ShowAboutButton()
	{
		Current.AboutButton.Visibility = Visibility.Visible;
	}
	public static void ShowHomeButton()
	{
		Current.HomeButton.Visibility = Visibility.Visible;
	}

	public static async Task ShowMessage(string title, string message)
	{
		if (Current.DefaultFlyout.IsOpen)
        {
            Current.DefaultFlyout.IsOpen = false;
        }

        await Current.ShowMessage(title, message).ConfigureAwait(false);
	}
	public static async Task<MessageDialogResult> ShowYesNoDialog(string title, string message)
	{
		if (Current.DefaultFlyout.IsOpen)
        {
            Current.DefaultFlyout.IsOpen = false;
        }

        return await Current.ShowYesNoDialog(title, message).ConfigureAwait(false);
	}
	public static Task ShowSelectableDialog(string title, string message, Action retryAction)
	{
		if (Current.DefaultFlyout.IsOpen)
        {
            Current.DefaultFlyout.IsOpen = false;
        }

        return Current.ShowSelectableDialog(title, message, retryAction);
	}
	public static void LoadPage(UserControl page) => Current.CurrentPage.Content = page;
	public static void SaveConsts()
	{
		try
		{
			File.WriteAllText(_configFilePath, JsonConvert.SerializeObject(settings));
			SaveDownloadSettings();
		}
		catch (Exception ex)
		{
			Log(ex.ToString(), "SaveConsts").Wait();
		}
	}
	public static void RestoreDefualts()
	{
		Log("Restoring defaults", "RestoreDefaults at GlobalConsts").Wait();
		settings = new Objects.Settings("Dark", "Red", "Italiano", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), false, false, true, TimeSpan.FromMinutes(1), true, 20, 2, true, true);
		DownloadSettings = new DownloadSettings("mp3", false, YoutubeHelpers.High1080, false, false, false, false, "192", false, "it", false, false, 0, 0, false, true, false, true, 4, "$title", false, "mp4");
		SaveConsts();
	}
	public static void LoadConsts()
	{

		if (!File.Exists(_configFilePath))
		{
			Log("Config file does not exist, restoring defaults", "LoadConsts at GlobalConsts").Wait();

			RestoreDefualts();
			return;
		}

		try
		{
			settings = JsonConvert.DeserializeObject<Objects.Settings>(File.ReadAllText(_configFilePath));
			ConversionsLocker = new SemaphoreSlim(settings.ActualConversionsLimit, settings.MaximumConversionsCount);

			LoadDownloadSettings();
		}
		catch (Exception ex)
		{
			Log(ex.ToString(), "LoadConsts at GlobalConsts").Wait();
			RestoreDefualts();
		}
		UpdateTheme();
		UpdateLanguage();

	}
	public static void CreateTempFolder()
	{
		try
		{
			if (!Directory.Exists(Path.GetTempPath() + APPLICATION_TEMP_FOLDER))
            {
                Directory.CreateDirectory(Path.GetTempPath() + APPLICATION_TEMP_FOLDER);
            }
        }
		catch (Exception ex)
		{
			Log($"Failed to create temp folder, {ex}", "CreateTempFolder at GlobalConsts").Wait();
		}

	}
	public static void CleanTempFolder()
	{
		if (Directory.Exists(Path.GetTempPath() + APPLICATION_TEMP_FOLDER))
		{
			DirectoryInfo di = new(Path.GetTempPath() + APPLICATION_TEMP_FOLDER);

			foreach (var file in di.GetFiles())
            {
                try { file.Delete(); } catch { }
            };

			foreach (var dir in di.GetDirectories())
            {
                try { dir.Delete(true); } catch { }
            };
		}
	}
	private static void UpdateTheme()
	{
		try
		{
			ThemeManager.Current.ChangeTheme(Application.Current, $"{OppositeTheme}.{settings.Accent}");
			ThemeManager.Current.ChangeTheme(Application.Current, $"{settings.Theme}.{settings.Accent}");
		}
		catch (Exception ex)
		{
			RestoreDefualts();
			Log(ex.ToString(), "UpdateTheme").ConfigureAwait(false);
		}
	}
	private static void UpdateLanguage()
	{
		var toRemove = Application.Current.Resources.MergedDictionaries.First(x => x.Source.OriginalString.Contains("Italiano"));
		ResourceDictionary r = new()
		{
			Source = new Uri($"/Languages/{settings.Language}.xaml", UriKind.Relative)
		};
		Application.Current.Resources.MergedDictionaries.Add(r);
		Application.Current.Resources.MergedDictionaries.Remove(toRemove);
	}
	public static void ChangeLanguage(string nLang)
	{
		var toRemove = Application.Current.Resources.MergedDictionaries.First(x => x.Source?.OriginalString.Contains(settings.Language) ?? false);
		ResourceDictionary r = new()
		{
			Source = new Uri($"/Languages/{nLang}.xaml", UriKind.Relative)
		};
		Application.Current.Resources.MergedDictionaries.Add(r);
		Application.Current.Resources.MergedDictionaries.Remove(toRemove);
		settings.Language = nLang;
	}
	public static async Task Log(string message, object sender)
	{
		using StreamWriter sw = new(_errorFilePath, true);
		await sw.WriteLineAsync($"[{DateTime.Now.ToUniversalTime()}], [{sender}]:\n\n{message}\n\n").ConfigureAwait(false);

	}
	public static string CleanFileName(string filename)
	{
		var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
		var invalidReStr = string.Format(@"[{0}]+", invalidChars);

		var reservedWords = new[]
		{
						"CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
						"COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
						"LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
				};

		var sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");
		foreach (var reservedWord in reservedWords)
		{
			var reservedWordPattern = string.Format("^{0}\\.", reservedWord);
			sanitisedNamePart = Regex.Replace(sanitisedNamePart, reservedWordPattern, "_reservedWord_.", RegexOptions.IgnoreCase);
		}

		return sanitisedNamePart;
	}

	static void CropAndSaveImage(byte[] imageBytes, string picLoc)
	{
		using var imageBuffer = new MemoryStream(imageBytes);
		using var src = System.Drawing.Image.FromStream(imageBuffer);
		var cropRect = new Rectangle((src.Width - src.Height) / 2, 0, src.Height, src.Height);
		using var target = new Bitmap(cropRect.Width, cropRect.Height);
		using var g = Graphics.FromImage(target);
		g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height), cropRect, GraphicsUnit.Pixel);
		target.Save(picLoc, ImageFormat.Jpeg);
	}

	static readonly string[] ignoredComments = ["Auto-generated by YouTube.", "Provided to YouTube by"];
	internal static readonly string[] ignoredGeneres = ["download", "out now", "mostercat", "video", "lyric", "release", "ncs"];
	internal static readonly string[] artistsSeperators = ["&", "feat.", "feat", "ft.", " ft ", "Feat.", " x ", " X "];

	static async Task<string> TagMusicFile(Video fullVideo, string file, int vIndex)
	{
		// Index YouTube Auto Generated Description
		var description = fullVideo.Description.Split("\n");
		var title = "";
		var artists = new List<string>();
		var album = "";
		DateTime releaseDate = default;
		var comment = new StringBuilder();
		var commentIndex = 0;

		try
		{
			foreach (var line in description)
			{
				if (string.IsNullOrWhiteSpace(line))
                {
                    goto loopEnd;
                }

                if (line.Contains('·'))
				{
					var titleAndArtists = line.Split('·').Select(x => x.Trim());
					title = titleAndArtists.FirstOrDefault();
					artists.AddRange(titleAndArtists.Skip(1));
					album = description.ElementAtOrDefault(commentIndex + 2);
					if (album != null && !album.Contains("Auto-generated by YouTube.") && !album.StartsWith("Released on:"))
					{
						goto loopEnd;
					}
					else
					{
						album = "";
					}
				}

				if (line.StartsWith("Released on:"))
				{
					releaseDate = DateTime.Parse(line.Split(":").ElementAtOrDefault(1));
					goto loopEnd;
				}

				if (!string.IsNullOrWhiteSpace(album) && line.StartsWith(album))
				{
					goto loopEnd;
				}

				if (ignoredComments.Any(x => line.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
				{
					goto loopEnd;
				}

				comment.AppendLine(line);

			loopEnd:
				commentIndex++;
			}
		}
		catch (Exception e)
		{
			await Log(e.ToString(), "TagMusicFile inside description loop").ConfigureAwait(false);
			return null;
		}

		if (releaseDate == default)
		{
			releaseDate = fullVideo.UploadDate.Date;
		}

		var t = TagLib.File.Create(file);
		t.Tag.Title = title;
		t.Tag.Performers = [.. artists];
		t.Tag.Year = (uint)releaseDate.Year;
		t.Tag.Comment = comment.ToString();
		t.Tag.Album = album;
		t.Tag.Track = (uint)vIndex;
		var picLoc = $"{TempFolderPath}{CleanFileName(fullVideo.Title)}.jpg";
		try
		{
			using (var httpClient = new HttpClient())
			{
				var imageContent = await httpClient.GetByteArrayAsync($"https://img.youtube.com/vi/{fullVideo.Id}/maxresdefault.jpg").ConfigureAwait(false);
				CropAndSaveImage(imageContent, picLoc);
			}

			t.Tag.Pictures = [new TagLib.Picture(picLoc)];
		}
		catch (Exception ex)
		{
			await Log("Failed to save picture at TagMusicFile", ex.ToString()).ConfigureAwait(false);
		}

		t.Save();
		return $"{string.Join(", ", artists)} - {title}";
	}

	public static async Task<string> TagFileBasedOnTitle(IVideo video, int vIndex, string file, FullPlaylist playlist = null)
	{
		var genre = video.Title.Split('[', ']').ElementAtOrDefault(1);
		if (genre == null)
        {
            genre = string.Empty;
        }
        else if (genre.Length >= video.Title.Length)
        {
            genre = string.Empty;
        }

        var title = video.Title;

		if (!string.IsNullOrWhiteSpace(genre))
		{
			title = video.Title.Replace($"[{genre}]", string.Empty);
			var rm = title.Split('[', ']', '【', '】').ElementAtOrDefault(1);
			if (!string.IsNullOrWhiteSpace(rm))
            {
                title = title.Replace($"[{rm}]", string.Empty);
            }
        }
		title = title.TrimStart(' ', '-', '[', ']');

		var t = TagLib.File.Create(file);

		t.Tag.Album = playlist?.BasePlaylist?.Title;
		t.Tag.Track = (uint)vIndex;
		//t.Tag.Year = (uint)video.UploadDate.Year;
		///t.Tag.DateTagged = video.UploadDate.UtcDateTime;
		t.Tag.AlbumArtists = [playlist?.BasePlaylist?.Author?.ChannelTitle];
		var lowerGenre = genre.ToLower();
		if (ignoredGeneres.Any(lowerGenre.Contains))
        {
            genre = string.Empty;
        }
        else
        {
            t.Tag.Genres = genre.Split('/', '\\');
        }

        var index = title.LastIndexOf('-');
		if (index > 0)
		{
			var vTitle = title[(index + 1)..].Trim(' ', '-');
			if (string.IsNullOrWhiteSpace(vTitle))
			{
				index = title.IndexOf('-');
				if (index > 0)
                {
                    vTitle = title[(index + 1)..].Trim(' ', '-');
                }
            }
			t.Tag.Title = vTitle;
			t.Tag.Performers = title[..(index - 1)].Trim().Split(artistsSeperators, StringSplitOptions.RemoveEmptyEntries);
		}

		try
		{
			var picLoc = $"{TempFolderPath}{CleanFileName(video.Title)}.jpg";
			using var http = new HttpClient();
			var response = await http.GetAsync($"https://img.youtube.com/vi/{video.Id}/maxresdefault.jpg").ConfigureAwait(false);
			using (var picStream = File.Create(picLoc))
			{
				await response.Content.CopyToAsync(picStream).ConfigureAwait(false);
			}
			t.Tag.Pictures = [new TagLib.Picture(picLoc)];
		}
		catch (Exception ex)
		{
			await Log("Failed to save picture at TagFile", ex.ToString()).ConfigureAwait(false);
		}

		t.Save();
		return file;
	}

	public static async Task<string> TagFile(IVideo video, int vIndex, string file, FullPlaylist playlist = null)
	{
		ArgumentNullException.ThrowIfNull(video);

		if (!video.Title.Contains(" - "))
		{
			var fullVideo = await YoutubeClient.Videos.GetAsync(video.Id).ConfigureAwait(false);
			if (fullVideo.Description.Contains("Auto-generated by YouTube."))
			{
				var fileName = await TagMusicFile(fullVideo, file, vIndex);
				if (fileName != null)
                {
                    return fileName;
                }
            }
		}

		return await TagFileBasedOnTitle(video, vIndex, file, playlist);
	}
	public static void LoadFlyoutPage(UserControl page)
	{
		Current.DefaultFlyoutUserControl.Content = page;
		Current.DefaultFlyout.IsOpen = true;
	}
	public static void CloseFlyout()
	{
		Current.DefaultFlyout.IsOpen = false;
		Current.DefaultFlyoutUserControl.Content = null;
	}
	public static double GetOffset()
	{
		return Current.ActualHeight - 95;
	}
	private static void LoadDownloadSettings()
	{
		if (File.Exists(DownloadSettingsFilePath))
		{
			try
			{
				_downloadSettings = JsonConvert.DeserializeObject<DownloadSettings>(File.ReadAllText(DownloadSettingsFilePath));
			}
			catch (Exception ex)
			{
				Log(ex.ToString(), "LoadDownloadSettings at GlobalConsts").Wait();
				try
				{
					if (File.Exists(DownloadSettingsFilePath))
                    {
                        File.Delete(DownloadSettingsFilePath);
                    }
                }
				catch (Exception ex2)
				{
					Log(ex2.ToString(), "Delete download settings file path").Wait();
				}
				_downloadSettings = new DownloadSettings("mp3", false, YoutubeHelpers.High720, false, false, false, false, "192", false, "en", false, false, 0, 0, false, true, false, true, 4, "$title", false, "mkv");
			}
		}
		else
		{
			_downloadSettings = new DownloadSettings("mp3", false, YoutubeHelpers.High720, false, false, false, false, "192", false, "en", false, false, 0, 0, false, true, false, true, 4, "$title", false, "mkv");
		}
	}
	public static void SaveDownloadSettings()
	{
		try
		{
			File.WriteAllText(DownloadSettingsFilePath, JsonConvert.SerializeObject(_downloadSettings));
		}
		catch (Exception ex)
		{
			Log(ex.ToString(), "SaveDownloadSettings at GlobalConsts").Wait();
		}
	}
	private static void Downloads_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (QueuedDownload item in e.NewItems)
            {
                MainPage.QueueStackPanel.Children.Add(item?.GetDisplayGrid());
            }
        }

        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (QueuedDownload item in e.OldItems)
			{
				MainPage.QueueStackPanel.Children.Remove(item?.GetDisplayGrid());
				item?.Dispose();
			}
        }
    }

}
