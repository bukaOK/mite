using AutoMapper;
using ImageMagick;
using Mite.BLL.Core;
using Mite.BLL.Helpers;
using Mite.CodeData.Enums;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.Models;
using NLog;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Mite.BLL.Services
{
    public interface IWatermarkService : IDataService
    {
        WatermarkEditModel Get(Guid id);
        /// <summary>
        /// Создает водяной знак
        /// </summary>
        /// <param name="fontSize"></param>
        /// <param name="text"></param>
        /// <param name="invert"></param>
        /// <returns>Изображение в base64</returns>
        string Draw(int fontSize, string text, bool invert);
        /// <summary>
        /// Добавить водяной знак
        /// </summary>
        /// <param name="watermark"></param>
        /// <param name="wmPosition"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        Task<DataServiceResult> AddAsync(WatermarkEditModel model);
        Task<DataServiceResult> RemoveAsync(Guid watId);
    }
    public class WatermarkService : DataService, IWatermarkService
    {
        const int ResizedImageSize = 500;
        const string ImagesFolder = "/upload/images/watermarks/";

        private readonly WatermarkRepository repository;

        public WatermarkService(IUnitOfWork database, ILogger logger) : base(database, logger)
        {
            repository = database.GetRepo<WatermarkRepository, Watermark>();
        }

        public async Task<DataServiceResult> AddAsync(WatermarkEditModel model)
        {
            var wat = new Watermark
            {
                Id = Guid.NewGuid(),
                Gravity = model.Gravity
            };
            if (model.UseCustomImage)
            {
                if (string.IsNullOrEmpty(model.WmPath))
                    return DataServiceResult.Failed("Не выбрано изображение для водяного знака");
                var match = Regex.Match(model.WmPath, @"^data:(?<contentType>\w+)/(?<format>\w+);base64,(?<base64>.+)", RegexOptions.IgnoreCase);
                if(!match.Success)
                {
                    if (!File.Exists(HostingEnvironment.MapPath(model.WmPath)))
                        return DataServiceResult.Failed("Неизвестное изображение");

                    //Находим знаки по виртуальному пути(их мб несколько с разными расположениями)
                    var existingWats = await repository.GetByPathAsync(model.WmPath);
                    //Поскольку в модели путь достается из базы, в принципе не может быть такого, чтобы его там не было
                    if (existingWats == null || existingWats.Count == 0)
                        return DataServiceResult.Failed("Не найдены водяные знаки");

                    //Находим конкретный знак по расположению
                    var existingPathWat = existingWats.FirstOrDefault(x => x.Gravity == model.Gravity);
                    if (existingPathWat != null)
                        return DataServiceResult.Success(existingPathWat.Id);
                    wat.VirtualPath = model.WmPath;
                    wat.ImageHash = existingWats.First().ImageHash;
                    wat.Gravity = model.Gravity;
                }
                var contentType = match.Groups["contentType"].Value;
                if (contentType != "image")
                    return DataServiceResult.Failed("Файл должен быть изображением");

                var fileBuffer = Convert.FromBase64String(match.Groups["base64"].Value);

                using (var sha1 = new SHA1Managed())
                {
                    var hashBytes = sha1.ComputeHash(fileBuffer);
                    wat.ImageHash = string.Join("", hashBytes.Select(x => x.ToString("x2")));
                }
                var existingWat = await repository.GetByParamsAsync(wat.ImageHash, model.Gravity);
                if (existingWat != null)
                    return DataServiceResult.Success(existingWat.Id);
                else
                {
                    var existingHashWat = await repository.GetByHashAsync(wat.ImageHash);
                    if(existingHashWat != null)
                        wat.VirtualPath = existingHashWat.VirtualPath;
                }

                var ext = match.Groups["format"].Value;
                if (ext == "jpeg")
                    ext = "jpg";
                var imgPath = "";
                var fullImgPath = "";
                do
                {
                    imgPath = Path.Combine(ImagesFolder, $"{Guid.NewGuid()}.{ext}");
                    fullImgPath = HostingEnvironment.MapPath(imgPath);
                } while (File.Exists(fullImgPath));
                try
                {
                    wat.VirtualPath = imgPath;
                    using(var saveStream = File.Create(fullImgPath))
                    {
                        await saveStream.WriteAsync(fileBuffer, 0, fileBuffer.Length);
                        saveStream.Position = 0;
                        new ImageOptimizer().LosslessCompress(saveStream);
                    }

                    await repository.AddAsync(wat);
                }
                catch(Exception e)
                {
                    return CommonError("Ошибка при сохранении водяного знака", e);
                }

                return DataServiceResult.Success(wat.Id);
            }
            else
            {
                wat = Mapper.Map<Watermark>(model);
                if (string.IsNullOrEmpty(model.WmText))
                    return DataServiceResult.Failed("Заполните текст");
                if (model.FontSize == 0)
                    return DataServiceResult.Failed("Выберите размер шрифта");

                var existingWat = await repository.GetByParamsAsync(model.WmText, model.Gravity, model.FontSize ?? 0, model.Inverted);
                if (existingWat != null)
                    return DataServiceResult.Success(existingWat.Id);
                else
                {
                    try
                    {
                        await repository.AddAsync(wat);
                        return DataServiceResult.Success(wat.Id);
                    }
                    catch(Exception e)
                    {
                        return CommonError("Ошибка при сохранении водяного знака", e);
                    }
                }
            }
        }

        public string Draw(int fontSize, string text, bool invert)
        {
            return "data:image/png;base64," + ImagesHelper.DrawWatermark(fontSize, text, invert);
        }

        public WatermarkEditModel Get(Guid id)
        {
            var watermark = repository.Get(id);
            return new WatermarkEditModel
            {
                Id = watermark.Id,
                WmPath = watermark.VirtualPath,
                Gravity = watermark.Gravity
            };
        }

        public async Task<DataServiceResult> RemoveAsync(Guid watId)
        {
            var count = await repository.GetPostsCountAsync(watId);

            if (count == 0)
            {
                try
                {
                    var wat = await repository.GetAsync(watId);
                    var isSameExists = await repository.SameExistsAsync(watId, wat.VirtualPath);
                    if(!isSameExists)
                        FilesHelper.DeleteFile(wat.VirtualPath);
                    await repository.RemoveAsync(watId);
                    return Success;
                }
                catch (Exception e)
                {
                    return CommonError("Ошибка при удалении водяного знака", e);
                }
            }
            return Success;
        }
    }
}