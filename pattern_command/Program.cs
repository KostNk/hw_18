using System;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using System.Collections.Generic;

namespace Task_18_4     
{
    class Program
    {

        public interface ICommand
        {
            public Task Execute();
        }

        internal class MySndr
        {
            ICommand command;
            public void UseCommand(ICommand command) => this.command = command;
            public async Task Execute() => await command.Execute();
        }

        public class myDwnldCmd : ICommand
        {
            MyRcvr rcvr;
            public myDwnldCmd(MyRcvr rcvr, string urlVideo)
            {
                this.rcvr = rcvr;
                this.rcvr.myVideoUrl = urlVideo;
            }

            public async Task Execute() => await rcvr.DownloadVideo();
        }

        public class myGetInfoCmd : ICommand
        {
            MyRcvr rcvr;
            public myGetInfoCmd(MyRcvr rcvr, string urlVideo)
            {
                this.rcvr = rcvr;
                this.rcvr.myVideoUrl = urlVideo;
            }

            public async Task Execute() => rcvr.GetVideoInfo();
        }

        static async Task Main()
        {
            MyRcvr rcvr = new();
            MySndr sndr = new();
            string urlVideo;

            while (true)
            {
                Console.Write("\n Please, type url for youtube video: ");
                urlVideo = Console.ReadLine();
                if (string.IsNullOrEmpty(urlVideo)) 
                {
                    Console.Write("\n You've entered empty url, try again...\n");
                    continue;
                }

                Console.Write("\n What do you want?: \n 1) - get video info \n 2) - download video \n Please, enter your choice: ");
                switch (Console.ReadLine())
                {
                    case "1":
                        myGetInfoCmd myGetInfoCmd = new(rcvr, urlVideo);
                        sndr.UseCommand(myGetInfoCmd);
                        await sndr.Execute();
                        break;

                    case "2":
                        myDwnldCmd myDwnldCmd = new(rcvr, urlVideo);
                        sndr.UseCommand(myDwnldCmd);
                        await sndr.Execute();
                        break;

                    default: 
                        Console.Write("\n You've entered incorrect number, try again...\n");
                        continue;
                }
            }
        }


        internal class MyRcvr
        {
            YoutubeClient myYoutubeObj = new();
            public string myVideoUrl { get; set; }
            VideoId videoNum;

            bool checkURL()
            {
                try
                {
                    this.videoNum = VideoId.Parse(this.myVideoUrl);
                    Console.WriteLine("Video num: {0}\n", videoNum);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Not found {0}, Exception {1}\n",this.myVideoUrl, ex.Message);
                    return false;
                }
            }
            public void GetVideoInfo()
            {
                if (!checkURL()) return;

                var info = myYoutubeObj.Videos.GetAsync(videoNum);
                Console.WriteLine(info.Result.Description);
            }
            public async Task DownloadVideo()
            {
                if (!checkURL()) return;

                try
                {
                    var myManifest = await myYoutubeObj.Videos.Streams.GetManifestAsync(videoNum);
                    var videoMuxedStream = myManifest.GetMuxedStreams().TryGetWithHighestVideoQuality();
                    string saveFile;
                    if (videoMuxedStream is null)
                    {
                        Console.Error.WriteLine("No muxed streams\n");
                        return;
                    }

                    Console.Write("Start downloading stream {0}", videoMuxedStream.Container.Name);
                    saveFile = videoNum+'.'+videoMuxedStream.Container.Name;
                    await myYoutubeObj.Videos.Streams.DownloadAsync(videoMuxedStream, saveFile);
                    Console.WriteLine("Video downloaded successfully\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception {0}\n", ex.Message);
                }
            }
        }
    }
}
