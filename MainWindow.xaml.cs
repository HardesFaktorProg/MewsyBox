using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using TagLib;
using TagLib.WavPack;




namespace MewsyBox
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Init MediaPlayer
       private MediaPlayer media = new MediaPlayer();

        private double currentValue = 1;
        private double targetValue = 1000;
        private DispatcherTimer timer;

        string folderPath = @"C:\Users\HardesPC\source\repos\MewsyBox\Resources\Music\Daily_Singer\";
        

        public void DailySinger_sync(){

            string[] files = Directory.GetFiles(folderPath, "*.mp3"); // Получаем все файлы с расширением ".mp3" из папки
            int songIndex = 1;

            foreach (string file in files){
                string songName = Path.GetFileNameWithoutExtension(file); // Получаем название песни из имени файла без расширения
                string songDuration = GetSongDuration(file); // Получаем длительность песни

                // Формируем имена элементов TextBlock
                string nameTextBlockName = "DSname_slot_" + songIndex;
                string timeTextBlockName = "DStime_slot_" + songIndex;

                // Находим TextBlock по имени
                TextBlock DStxtBox_name = (TextBlock)this.FindName(nameTextBlockName);
                TextBlock DStxtBox_time = (TextBlock)this.FindName(timeTextBlockName);

                // Если TextBlock найден, загружаем данные
                if (DStxtBox_name != null){
                    DStxtBox_name.Text = songName; // Название песни
                }

                if (DStxtBox_time != null){
                    DStxtBox_time.Text = songDuration; // Длительность песни
                }

                songIndex++; // Увеличиваем индекс для следующей песни
            }
        }
        private string GetSongDuration(string filePath){
            try{
                using (var file = System.IO.File.OpenRead(filePath)){
                    var tagFile = TagLib.File.Create(new StreamFileAbstraction(Path.GetFileName(filePath), file, file));
                    var duration = tagFile.Properties.Duration;
                    return string.Format("{0:mm\\:ss}", duration);
                }
            }catch (Exception ex){
                // Обработка ошибок, если файл не является аудиофайлом или произошла другая ошибка
                return "00:00";
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            // Syncronizathion Functions
            DailySinger_sync();

            // Timer for Player 
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000); // Интервал обновления значения слайдера (например, каждые 100 мс)
            timer.Tick += Timer_Tick;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e){
            if(e.ChangedButton == MouseButton.Left)
                try{
                    this.DragMove();
                }catch (Exception ex){
                    MessageBox.Show(ex.Message);
                }            
        }

        // > CORE:
        private void Timer_Tick(object sender, EventArgs e){
            if (currentValue < targetValue){
                // Увеличиваем значение слайдера
                currentValue += 1; 
                slider.Value = currentValue;
            }else{
                // Достигли целевого значения, останавливаем таймер
                timer.Stop();
             }
        }

        // > NAVIGATION
        private void OpenPlaylists_bn(object sender, RoutedEventArgs e)
        {
            
        }

        // > CATEGORY: HOME 
        private void border_MouseEnter(object sender, MouseEventArgs e){
            Border myborder = (Border)sender;
            myborder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#03bf69"));
            
        }
        private void border_MouseLeave(object sender, MouseEventArgs e){
            Border myborder = (Border)sender;
            myborder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ecf4f0"));
        }
        private void collect_yes(object sender, MouseButtonEventArgs e){
            DailyLike.Foreground = new SolidColorBrush((Color) ColorConverter.ConvertFromString("Green"));
            DailyDislike.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
        }
        private void collect_no(object sender, MouseButtonEventArgs e){
            DailyDislike.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
            DailyLike.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Black"));
        }


        // > CATEGORY: MUSIC PLAYER
        public void PlayMusic(string musicpath = "Resources\\Music\\", string filename = ""){     
            string exePath = Assembly.GetExecutingAssembly().Location,
                   directory = Path.GetDirectoryName(exePath),
                   fulldirectory = directory.Replace("bin\\Debug", "");

            Uri uri = new Uri(fulldirectory + musicpath + filename);
          
            media.Volume = 50;

            // Start Song Playing
            media.Open(uri);
            media.Play();
            ChangeSongSlider();

            // Change Icon for "Pause Button"
            PlayStopButton.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
        }

   
        // > CATEGORY: DAILY SINGER 
        private void DailyMusic_Play(object sender, MouseButtonEventArgs e){
            media.Stop();
            string id = "1";

            if (sender is Border border){
                id = border.Name.Replace("dailytrack_", "");
            }
            
            // Находим TextBlock по имени
            TextBlock DStxtBox_name = (TextBlock)this.FindName("DSname_slot_" + id);
            TextBlock DStxtBox_time = (TextBlock)this.FindName("DStime_slot_" + id);

            string songname = DStxtBox_name.Text + ".mp3";

            player_songauthor.Text = DailySinger.Text.Replace("Group", "");
            player_songname.Text = DStxtBox_name.Text;
            player_songduration.Text = DStxtBox_time.Text;

            PlayMusic("Resources\\Music\\Daily_Singer\\" +  songname);
        }

        public void ChangeSongSlider(){
            // Changing Slider Value
            currentValue = 1;
            slider.Value = currentValue;

            // Change targer Value
            targetValue = 1000;

            // Start Timer
            timer.Start();
        }

        bool isPaused = false;
        private void PauseUnPause(object sender, RoutedEventArgs e){
            if (isPaused){            
                media.Play();
                timer.Start();
               
                PlayStopButton.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause; isPaused = false;
            }else {                
                media.Pause();
                timer.Stop();
                
                PlayStopButton.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play; isPaused = true;
            }
        }
    
        bool isMuted= false;
        private void MuteAndUnmute(object sender, MouseButtonEventArgs e){
            if (!isMuted){
                mutebutton.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.VolumeMute;
                media.Volume = 0;
                isMuted = true;
            }else{
                mutebutton.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.VolumeHigh;
                media.Volume = 0.5;
                isMuted = false;
            }
        }
    }
}
