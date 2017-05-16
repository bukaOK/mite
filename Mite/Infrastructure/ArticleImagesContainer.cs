using Mite.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Mite.Infrastructure
{
    public interface IImagesContainer
    {
        void Delete(Image image);
        void Delete(string path, bool saved);
        void DeleteList(IEnumerable<string> paths, bool saved);
        void Add(Image image);
    }
    /// <summary>
    /// Контейнер изображений, которые отправляются при редактировании статьи
    /// </summary>
    public class ArticleImagesContainer : IImagesContainer
    {
        private readonly List<Image> images;
        public ArticleImagesContainer()
        {
            images = new List<Image>();
        }
        public void Delete(Image image)
        {
            images.Remove(image);
        }
        public void Delete(string imgPath, bool saved)
        {
            images.First(x => x.Path == imgPath).Saved = true;
            images.RemoveAll(x => x.Path == imgPath);
        }
        public void DeleteList(IEnumerable<string> paths, bool saved)
        {
            var savedImages = images.Where(x => paths.Any(y => y == x.Path));
            foreach(var img in savedImages)
            {
                img.Saved = saved;
            }
            images.RemoveAll(x => paths.Any(y => y == x.Path));
        }
        public void Add(Image image)
        {
            images.Add(image);
        }
        public void Add(string path)
        {
            var image = new Image(path, this)
            {
                Saved = false
            };
        }
        ~ArticleImagesContainer()
        {
            images.Clear();
        }
    }
    public class Image 
    {
        public string Path { get; set; }
        public bool Saved { get; set; }

        private IImagesContainer container;
        private Timer timer;
        private int interval = 1000 * 60 * 60 * 12;

        public Image(string path, IImagesContainer container)
        {
            Path = path;
            this.container = container;
            timer = new Timer();
            timer.Interval = interval;
            timer.Elapsed += DeleteImageEvent;
            timer.AutoReset = false;
            timer.Start();
        }
        private void DeleteImageEvent(object source, ElapsedEventArgs args)
        {
            Saved = false;
            container.Delete(this);
        }
        ~Image()
        {
            if(!Saved)
                FilesHelper.DeleteFile(Path);
        }
    }
}