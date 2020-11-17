using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sony.Vegas;
using System.Windows.Forms;
// test comment
namespace VideoEdit1
{
    public class EntryPoint
    {
        public void FromVegas(Vegas vegas)
        {
            VegasTest(vegas);
        }

        /*
         Goal: generates a looping video using a still image. Adds zooming effect on image.
          */
        public void VegasTest(Vegas v)
        {
            
            string backgroundPath = @"C:\Users\captn\Pictures\test.jpg";
            VideoTrack backgroundTrack = AddNewVideoTrackAtIndex(v, 0, "mybackground");
            Timecode start = Timecode.FromFrames(0);
            Timecode end = Timecode.FromSeconds((double)(60 * 5)); // 5 minute long video
            VideoEvent backgroundVideoEvent = AddVideoEvent(v, backgroundPath, start, end, backgroundTrack);

            ZoomInAndOutAtCenter(v, ref backgroundVideoEvent, 75, 30);
        }
        VideoEvent AddVideoEvent(Vegas vegas, String mediaFile, Timecode start, Timecode length, VideoTrack track)
        {
            Media media = new Media(mediaFile);
            VideoEvent videoEvent = track.AddVideoEvent(start, length);
            Take take = videoEvent.AddTake(media.GetVideoStreamByIndex(0));
            return videoEvent;
        }
        public static VideoTrack AddNewVideoTrackAtIndex(Vegas vegas, int index, string label)
        {
            VideoTrack vTrack = new VideoTrack(index, label);

            vegas.Project.Tracks.Add(vTrack);
            return vTrack;
        }
        public static void ZoomInAndOutAtCenter(Vegas vegas, ref VideoEvent ev, int zoomSlidePercentMiddle, int everyXSec)
        {
            
            // backup start original zoom level
            VideoMotionKeyframe startMotionKeyFrameBackup = ev.VideoMotion.Keyframes[0];

            Timecode startTimecode = ev.Start;
            Timecode endTimecode = ev.End;


            Timecode zoomInterval = Timecode.FromSeconds((double)everyXSec);
            Timecode currentTimecode = startTimecode;


            
            while (currentTimecode < endTimecode)
            {
                if (currentTimecode + zoomInterval < endTimecode)
                {
                    if (currentTimecode + (zoomInterval + zoomInterval) < endTimecode)
                    {
                        ZoomInToTimecode(vegas, ref ev, zoomSlidePercentMiddle, currentTimecode + zoomInterval);
                        currentTimecode += zoomInterval;
                    }

                    else
                    {
                        ZoomInToTimecode(vegas, ref ev, zoomSlidePercentMiddle, endTimecode);
                        currentTimecode += zoomInterval;
                    }


                }
                else
                {
                    double remainingTimeSeconds = (endTimecode.ToMilliseconds()
                        / 1000.0) - (currentTimecode.ToMilliseconds() / 1000.0);
                    int newZoomPercent = 100 - (int)(((double)(100 - zoomSlidePercentMiddle) * remainingTimeSeconds)
                                            / (double)everyXSec);
                    ZoomInToTimecode(vegas, ref ev, newZoomPercent, endTimecode);
                    currentTimecode += zoomInterval;
                }
                if (currentTimecode + zoomInterval < endTimecode)
                {
                    ZoomOutToTimecode(vegas, ref ev, startMotionKeyFrameBackup, currentTimecode + zoomInterval);
                    currentTimecode += zoomInterval;
                }
            }



        }
        public static void ZoomInToTimecode(Vegas vegas, ref VideoEvent ev,
            int zoomSlidePercentMiddle, Timecode endZoomTimecode)
        {
            VideoMotionKeyframe middleMotionKeyFrame = new VideoMotionKeyframe(endZoomTimecode);

            ev.VideoMotion.Keyframes.Add(middleMotionKeyFrame);


            VideoMotionVertex scaleMiddle = new VideoMotionVertex((float)(zoomSlidePercentMiddle) /
                (float)100.0, (float)(zoomSlidePercentMiddle) / (float)100.0);
            middleMotionKeyFrame.ScaleBy(scaleMiddle);
        }
        public static void ZoomOutToTimecode(Vegas vegas, ref VideoEvent ev,
            VideoMotionKeyframe startMotionKeyFrameBackup, Timecode endZoomOutTimecode)
        {
            VideoMotionKeyframe endMotionKeyFrame = new VideoMotionKeyframe(endZoomOutTimecode);
            ev.VideoMotion.Keyframes.Add(endMotionKeyFrame);
            ev.Name = "zoomOut";
            endMotionKeyFrame.Bounds = new VideoMotionBounds(startMotionKeyFrameBackup.TopLeft,
                startMotionKeyFrameBackup.TopRight, startMotionKeyFrameBackup.BottomRight,
                startMotionKeyFrameBackup.BottomLeft);
        }
    }
}