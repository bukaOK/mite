using AutoMapper;
using Mite.BLL.Core;
using Mite.CodeData.Enums;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace Mite.BLL.Helpers
{
    public static class PostsHelper
    {
        static readonly string imagesFolder;
        static readonly string documentsFolder;
        static PostsHelper()
        {
            imagesFolder = HostingEnvironment.ApplicationVirtualPath + "Public/images/";
            documentsFolder = HostingEnvironment.ApplicationVirtualPath + "Public/documents/";
        }
        /// <summary>
        /// Удаление, создание изображения
        /// </summary>
        /// <param name="post">Старый пост</param>
        /// <param name="model">Модель с новыми данными</param>
        /// <returns></returns>
        public static void UpdateImage(Post post, PostModel model)
        {
            if (post.Content == model.Content || string.IsNullOrEmpty(model.Content))
                return;
            var tuple = ImagesHelper.UpdateImage(post.Content, post.Content_50, model.Content);
            post.Content = tuple.vPath;
            post.Content_50 = tuple.compressedVPath;
        }
        /// <summary>
        /// Обновляем контент документа, а также обложку
        /// </summary>
        /// <param name="post"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static void UpdateDocument(Post post, PostModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Content))
                FilesHelper.UpdateDocument(post.Content, model.Content);

            //Заменяем
            if (!string.IsNullOrEmpty(model.Cover) && !string.IsNullOrEmpty(post.Cover))
            {
                var tuple = ImagesHelper.UpdateImage(post.Cover, post.Cover_50, model.Cover);
                post.Cover = tuple.vPath;
                post.Cover_50 = tuple.compressedVPath;
            }
            //Добавляем
            else if (!string.IsNullOrEmpty(model.Cover) && string.IsNullOrEmpty(post.Cover))
            {
                var tuple = ImagesHelper.CreateImage(imagesFolder, model.Cover);
                post.Cover = tuple.vPath;
                post.Cover_50 = tuple.compressedVPath;
            }
            //Удаляем
            else if (string.IsNullOrEmpty(model.Cover) && !string.IsNullOrEmpty(post.Cover))
            {
                ImagesHelper.DeleteImage(post.Cover, post.Cover_50);
                post.Cover = null;
            }
        }
        /// <summary>
        /// Обновляем элементы коллекции
        /// </summary>
        /// <param name="post"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static void UpdateImageCollection(Post post, PostModel model)
        {
            var itemsToUpdate = post.Collection.Where(x => model.Collection.Any(y => y.Id == x.Id));
            var itemsToAdd = model.Collection.Where(x => x.Id == Guid.Empty && !post.Collection.Any(y => y.Id == x.Id));
            var itemsToDel = post.Collection.Except(itemsToUpdate);
            foreach (var postItem in itemsToUpdate)
            {
                var modelItem = model.Collection.First(x => x.Id == postItem.Id);
                postItem.Description = modelItem.Description;
                if (postItem.ContentSrc != modelItem.Content || string.IsNullOrEmpty(model.Content))
                {
                    var tuple = ImagesHelper.UpdateImage(postItem.ContentSrc, postItem.ContentSrc_50, modelItem.Content);
                    postItem.ContentSrc = tuple.vPath;
                    postItem.ContentSrc_50 = tuple.compressedVPath;
                }
            }
            foreach (var modelItem in itemsToAdd)
            {
                modelItem.Id = Guid.NewGuid();
                var postItem = Mapper.Map<PostCollectionItem>(modelItem);
                var tuple = ImagesHelper.CreateImage(imagesFolder, modelItem.Content);
                postItem.PostId = post.Id;
                postItem.ContentSrc = tuple.vPath;
                postItem.ContentSrc_50 = tuple.compressedVPath;
                post.Collection.Add(postItem);
            }
            foreach (var postItem in itemsToDel)
            {
                ImagesHelper.DeleteImage(postItem.ContentSrc, postItem.ContentSrc_50);
            }
            post.Collection = post.Collection.Except(itemsToDel).ToList();
        }
        /// <summary>
        /// Обновляем страницы комикса
        /// </summary>
        /// <param name="post"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static void UpdateComicsItems(Post post, PostModel model)
        {
            var itemsToUpdate = post.ComicsItems.Where(x => model.ComicsItems.Any(y => y.Id == x.Id));
            var itemsToAdd = model.ComicsItems.Where(x => x.Id == Guid.Empty && !post.ComicsItems.Any(y => y.Id == x.Id));
            var itemsToDel = post.ComicsItems.Except(itemsToUpdate);
            foreach (var postItem in itemsToUpdate)
            {
                var modelItem = model.ComicsItems.First(x => x.Id == postItem.Id);
                postItem.Page = modelItem.Page;
                if (postItem.ContentSrc != modelItem.Content)
                {
                    var tuple = ImagesHelper.UpdateImage(postItem.ContentSrc, postItem.ContentSrc_50, modelItem.Content);
                    postItem.ContentSrc = tuple.vPath;
                    postItem.ContentSrc_50 = tuple.compressedVPath;
                }
            }
            foreach (var modelItem in itemsToAdd)
            {
                modelItem.Id = Guid.NewGuid();
                var postItem = Mapper.Map<ComicsItem>(modelItem);
                var tuple = ImagesHelper.CreateImage(imagesFolder, modelItem.Content);
                postItem.PostId = post.Id;
                postItem.ContentSrc = tuple.vPath;
                postItem.ContentSrc_50 = tuple.compressedVPath;
                post.ComicsItems.Add(postItem);
            }
            foreach (var postItem in itemsToDel)
            {
                ImagesHelper.DeleteImage(postItem.ContentSrc, postItem.ContentSrc_50);
            }
            post.ComicsItems = post.ComicsItems.Except(itemsToDel).ToList();
        }
        public static void CreatePostCollection(Post post, PostContentTypes contentType)
        {
            IEnumerable<IContentEntity> contentItems;
            switch (contentType)
            {
                case PostContentTypes.ImageCollection:
                    contentItems = post.Collection;
                    break;
                case PostContentTypes.Comics:
                    contentItems = post.ComicsItems;
                    break;
                default:
                    throw new ArgumentException("Неизвестный тип контента");
            }
            var tuple = ImagesHelper.CreateImage(imagesFolder, post.Content);
            post.Content = tuple.vPath;
            post.Content_50 = tuple.compressedVPath;

            var lastPost = 0;
            foreach (var item in contentItems)
            {
                try
                {
                    tuple = ImagesHelper.CreateImage(imagesFolder, item.ContentSrc);
                    item.ContentSrc = tuple.vPath;
                    item.ContentSrc_50 = tuple.compressedVPath;
                    lastPost++;
                }
                catch (Exception e)
                {
                    foreach(var delItem in contentItems.Where(x => !string.IsNullOrEmpty(x.ContentSrc_50)))
                    {
                        FilesHelper.DeleteFile(delItem.ContentSrc);
                        FilesHelper.DeleteFile(delItem.ContentSrc_50);
                    }
                    throw e;
                }
            }
        }

        public static void CreateDocument(Post post)
        {
            if (!string.IsNullOrEmpty(post.Cover))
            {
                var tuple = ImagesHelper.CreateImage(imagesFolder, post.Cover);
                post.Cover = tuple.vPath;
                post.Cover_50 = tuple.compressedVPath;
            }
            post.Content = FilesHelper.CreateDocument(documentsFolder, post.Content);
        }
        public static void CreateImage(Post post)
        {
            try
            {
                post.Content = FilesHelper.CreateImage(imagesFolder, post.Content);

                var fullCPath = HostingEnvironment.MapPath(post.Content);
                ImagesHelper.Compressed.Compress(fullCPath);
                post.Content_50 = ImagesHelper.Compressed.CompressedVirtualPath(fullCPath);
            }
            catch (Exception e)
            {
                FilesHelper.DeleteFile(post.Content);
                FilesHelper.DeleteFile(post.Content_50);
                throw e;
            }
        }
    }
}